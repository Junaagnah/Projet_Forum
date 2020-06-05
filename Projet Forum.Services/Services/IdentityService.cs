using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Services.Models;
using Projet_Forum.Services.Helpers;
using Projet_Forum.Services.ViewModels;
using Projet_Forum.Data.Models;
using Microsoft.Extensions.Options;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace Projet_Forum.Services.Services
{
    /// <summary>
    /// Service permettant de gérer les utilisateurs
    /// </summary>
    public class IdentityService : IIdentityService
    {
        private const string defaultProfilePicture = "images/defaultProfilePicture.jpg";

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ISendgridService _sendgridService;
        private readonly AppSettings _appSettings;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IImageService _imageService;

        public IdentityService(UserManager<User> userManager,
            SignInManager<User> signInManager,
            ISendgridService sendgridService,
            IOptions<AppSettings> appSettings,
            IRefreshTokenService refreshTokenService,
            IImageService imageService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _sendgridService = sendgridService;
            _appSettings = appSettings.Value;
            _refreshTokenService = refreshTokenService;
            _imageService = imageService;
        }

        /// <summary>
        /// Permet de connecter un utilisateur via son email et son mot de passe
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> SignInUserAsync(string email, string password)
        {
            if (String.IsNullOrWhiteSpace(email) || String.IsNullOrWhiteSpace(email))
            {
                MyResponse error = new MyResponse { Succeeded = false };
                if (String.IsNullOrWhiteSpace(email)) error.Messages.Add("EmailCannotBeNull");
                if (String.IsNullOrWhiteSpace(password)) error.Messages.Add("PasswordCannotBeNull");

                return error;
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(user, password, true, false);
                if (signInResult.Succeeded)
                {
                    if (!user.IsBanned)
                    {
                        // On vérifie si l'utilisateur n'a pas déjà un refreshtoken pour pouvoir le supprimer et le remplacer par le nouveau
                        var oldToken = await _refreshTokenService.GetRefreshTokenByUserId(user.Id);
                        if (oldToken != null && oldToken.ValidBefore < DateTime.Now) await _refreshTokenService.DeleteRefreshToken(oldToken.TokenValue); // S'il existe et qu'il n'est plus valide, on le supprime

                        // Génération du jwt
                        string tokenString = GenerateJwtToken(user);
                        string refreshToken = Guid.NewGuid().ToString();
                        if (oldToken != null && oldToken.ValidBefore < DateTime.Now) refreshToken = oldToken.TokenValue;

                        RefreshToken token = new RefreshToken
                        {
                            TokenValue = refreshToken,
                            UserId = user.Id,
                            ValidBefore = DateTime.Now.AddDays(7)
                        };

                        await _refreshTokenService.CreateRefreshToken(token);

                        // Une fois que le token est généré, on renvoie l'username et le role pour la webapp
                        var response = new MyResponse { Succeeded = true };
                        response.Result = new SigningResponse
                        {
                            Token = tokenString,
                            RefreshToken = refreshToken,
                            IsBanned = user.IsBanned,
                        };

                        return response;
                    }
                    MyResponse error = new MyResponse { Succeeded = false };
                    error.Messages.Add("UserBanned");
                    return error;
                }
                else
                {
                    // Si le compte de l'utilisateur n'est pas confirmé, on lui renvoie un mail de confirmation
                    if (!user.EmailConfirmed)
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        bool mailResult = await _sendgridService.SendValidationEmail(user.Email, user.Id, token);

                        if (mailResult)
                        {
                            return new MyResponse { Succeeded = false, Messages = { "ConfirmationEmailResent" } };
                        }
                    }
                    MyResponse error = new MyResponse { Succeeded = false };

                    // Verrouillage désactivé donc ne devrait pas arriver, mais on sait jamais
                    if (signInResult.IsLockedOut) error.Messages.Add("AccountLocked");
                    if (signInResult.IsNotAllowed) error.Messages.Add("AccountNotAllowed");
                    if (!signInResult.Succeeded && !signInResult.IsLockedOut && !signInResult.IsNotAllowed) error.Messages.Add("WrongEmailOrPassword");

                    return error;
                }
            }

            MyResponse resp = new MyResponse { Succeeded = false };
            resp.Messages.Add("WrongEmailOrPassword");

            return resp;
        }

        /// <summary>
        /// Permet de générer un nouveau token
        /// </summary>
        /// <param name="user"></param>
        /// <returns>token</returns>
        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim("username", user.UserName),
                new Claim("role", user.Role.ToString()),
                new Claim("isBanned", user.IsBanned.ToString())
            };

            var algorithm = SecurityAlgorithms.HmacSha512;

            var secret = _appSettings.Secret;
            byte[] byteSecret = Encoding.UTF8.GetBytes(secret);
            var key = new SymmetricSecurityKey(byteSecret);

            var signingCredentials = new SigningCredentials(key, algorithm);

            var objectToken = new JwtSecurityToken(
                issuer: "Projet_Forum",
                audience: "clients",
                claims: claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: signingCredentials
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(objectToken);
            return tokenString;
        }

        /// <summary>
        /// Permet d'enregistrer un utilisateur dans la base de données
        /// </summary>
        /// <param name="user"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> RegisterUserAsync(RegisterViewModel user)
        {
            if (user == null || String.IsNullOrWhiteSpace(user.Email) || String.IsNullOrWhiteSpace(user.Username) || String.IsNullOrWhiteSpace(user.Password))
            {
                MyResponse error = new MyResponse { Succeeded = false };
                if (user == null) error.Messages.Add("UserCannotBeNull");
                if (String.IsNullOrWhiteSpace(user.Email)) error.Messages.Add("EmailCannotBeNull");
                if (String.IsNullOrWhiteSpace(user.Username)) error.Messages.Add("UsernameCannotBeNull");
                if (String.IsNullOrWhiteSpace(user.Password)) error.Messages.Add("PasswordCannotBeNull");

                return error;
            }
            var newUser = new User
            {
                UserName = user.Username,
                Email = user.Email
            };

            var creationResult = await _userManager.CreateAsync(newUser, user.Password);
            if(creationResult.Succeeded)
            {
                // On récupère le nouvel utilisateur pour pouvoir récupérer son id
                newUser = await this._userManager.FindByEmailAsync(newUser.Email);
                if (newUser != null)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    bool mailResult = await _sendgridService.SendValidationEmail(newUser.Email, newUser.Id, token);

                    if (mailResult)
                    {
                        return new MyResponse { Succeeded = true };
                    }
                    else
                    {
                        // Si échec, on supprime l'utilisateur & on lui renvoie une erreur
                        await _userManager.DeleteAsync(newUser);
                        var error = new MyResponse { Succeeded = false };
                        error.Messages.Add("EmailNotSent");

                        return error;
                    }
                }
            }
            else
            {
                MyResponse error = new MyResponse { Succeeded = false };

                creationResult.Errors.ToList().ForEach(e =>
                {
                    error.Messages.Add(e.Code);
                });

                return error;
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de confirmer l'adresse e-mail d'un utilisateur
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> ConfirmEmail(string userId, string token)
        {
            if (!String.IsNullOrWhiteSpace(userId) && !String.IsNullOrWhiteSpace(token))
            {
                User user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var result = await _userManager.ConfirmEmailAsync(user, token);
                    if (result.Succeeded)
                    {
                        return new MyResponse { Succeeded = true };
                    }
                    else
                    {
                        MyResponse error = new MyResponse { Succeeded = false };
                        result.Errors.ToList().ForEach(err =>
                        {
                            error.Messages.Add(err.Code);
                        });

                        return error;
                    }
                }
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet à un utilisateur de demander un token par mail afin de renouveller son mot de passe
        /// </summary>
        /// <param name="email"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> AskPasswordRecovery(string email)
        {
            if (!String.IsNullOrWhiteSpace(email))
            {
                User user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    // Génération du token de récupération de mot de passe & envoi par mail
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var mailSent = await _sendgridService.SendRecoveryEmail(user.Email, user.Id, token);
                    if (!mailSent)
                    {
                        MyResponse error = new MyResponse { Succeeded = false };
                        error.Messages.Add("EmailNotSent");
                        return error;
                    }
                }
            }

            // On renvoie toujours true pour éviter que des gens testent plusieurs adresses e-mail
            return new MyResponse { Succeeded = true };
        }

        /// <summary>
        /// Permet à un utilisateur de récupérer son mot de passe via un token envoyé au préalable
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="token"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> RecoverPassword(string userId, string password, string token)
        {
            if (!String.IsNullOrWhiteSpace(userId) && !String.IsNullOrWhiteSpace(password) && !String.IsNullOrWhiteSpace(token))
            {
                User user = await _userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, token, password);
                    if (result.Succeeded)
                    {
                        return new MyResponse { Succeeded = true };
                    }

                    MyResponse error = new MyResponse { Succeeded = false };

                    result.Errors.ToList().ForEach(err =>
                    {
                        error.Messages.Add(err.Code);
                    });

                    return error;
                }
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de créer un nouveau refreshToken
        /// </summary>
        /// <param name="username"></param>
        /// <param name="refreshToken"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> RenewToken(string username, string refreshToken)
        {
            // Vérification du refreshToken correspondant au user
            var user = await _userManager.FindByNameAsync(username);
            if (user != null)
            {
                if (!user.IsBanned)
                {
                    var token = await _refreshTokenService.GetRefreshTokenByValueAndUserId(refreshToken, user.Id);
                    if (token != null && token.ValidBefore > DateTime.Now) // Si on trouve un token et qu'il est toujours valide
                    {
                        // On supprime l'ancien token
                        await _refreshTokenService.DeleteRefreshToken(token.TokenValue);

                        // On crée le nouveau token & refreshToken
                        var newToken = GenerateJwtToken(user);
                        var refreshTokenValue = Guid.NewGuid().ToString();

                        RefreshToken newRefreshToken = new RefreshToken
                        {
                            TokenValue = refreshTokenValue,
                            UserId = user.Id,
                            ValidBefore = DateTime.Now.AddDays(7)
                        };

                        await _refreshTokenService.CreateRefreshToken(newRefreshToken);

                        MyResponse succeededResponse = new MyResponse { Succeeded = true };
                        succeededResponse.Result = new SigningResponse
                        {
                            RefreshToken = newRefreshToken.TokenValue,
                            Token = newToken,
                            IsBanned = user.IsBanned
                        };

                        return succeededResponse;
                    }

                    return new MyResponse { Succeeded = false };
                }

                MyResponse response = new MyResponse { Succeeded = false };
                response.Messages.Add("UserBanned");
                return response;
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de déconnecter un utilisateur
        /// </summary>
        /// <param name="username"></param>
        /// <returns>Void</returns>
        public async Task Disconnect(string username)
        {
            if (!String.IsNullOrWhiteSpace(username))
            {
                // On supprime le refreshToken de l'utilisateur pour le déconnecter
                var user = await _userManager.FindByNameAsync(username);

                if (user != null)
                {
                    var refreshToken = await _refreshTokenService.GetRefreshTokenByUserId(user.Id);

                    if (refreshToken != null) await _refreshTokenService.DeleteRefreshToken(refreshToken.TokenValue);
                }
            }
        }

        /// <summary>
        /// Permet de récupérer un utilisateur via son nom d'utilisateur
        /// </summary>
        /// <param name="username"></param>
        /// <returns>User</returns>
        public async Task<User> GetUserByUsername(string username)
        {
            User user = null;

            if (!String.IsNullOrWhiteSpace(username))
            {
                user = await _userManager.FindByNameAsync(username);
            }

            return user;
        }

        /// <summary>
        /// Permet de récupérer un utilisateur via son id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>User</returns>
        public async Task<User> GetUserById(string userId)
        {
            User user = null;

            if (!String.IsNullOrWhiteSpace(userId))
            {
                user = await _userManager.FindByIdAsync(userId);
            } 

            return user;
        }

        /// <summary>
        /// Permet de récupérer le profil d'un utilisateur via son nom d'utilisateur
        /// </summary>
        /// <param name="username"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> GetUserProfileByUsername(string username)
        {
            if (!String.IsNullOrWhiteSpace(username))
            {
                var user = await _userManager.FindByNameAsync(username);

                if (user != null)
                {
                    var rolename = "Utilisateur";
                    if (user.Role == 2) rolename = "Modérateur";
                    if (user.Role == 3) rolename = "Administrateur";

                    var picturePath = _imageService.GetImagePathByUserId(user.Id);
                    if (picturePath == null) picturePath = defaultProfilePicture;

                    var toReturn = new ProfileResponse
                    {
                        Username = user.UserName,
                        Role = user.Role,
                        RoleName = rolename,
                        Description = user.Description,
                        ProfilePicture = picturePath,
                        IsBanned = user.IsBanned,
                        Email = user.Email
                    };

                    var response = new MyResponse { Succeeded = true };
                    response.Result = toReturn;

                    return response;
                }
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet à un utilisateur de modifier son mot de passe en renseignant l'ancien
        /// </summary>
        /// <param name="username"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> UpdateUserPassword(string username, string oldPassword, string newPassword)
        {
            if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(oldPassword) && !String.IsNullOrWhiteSpace(newPassword))
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user != null)
                {
                    var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
                    if (result.Succeeded)
                    {
                        return new MyResponse { Succeeded = true };
                    }
                    else
                    {
                        var error = new MyResponse { Succeeded = false };

                        result.Errors.ToList().ForEach(err =>
                        {
                            error.Messages.Add(err.Code);
                        });

                        return error;
                    }
                }
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet à un utilisateur de mettre à jour un profil
        /// </summary>
        /// <param name="username"></param>
        /// <param name="data"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> UpdateUserProfile(string username, UpdateProfileViewModel data, string requestingUser = "")
        {
            if (!String.IsNullOrWhiteSpace(username) && data != null)
            {
                var userToUpdate = await _userManager.FindByNameAsync(username);
                User requestingUserProfile = null;
                if (!String.IsNullOrWhiteSpace(requestingUser)) requestingUserProfile = await _userManager.FindByNameAsync(requestingUser);
                if (userToUpdate != null)
                {
                    // Si la fonction est appelée depuis l'administration, on passe un requestingUser et s'il n'est pas nul, on vérifie qu'il est bien Administrateur
                    if (requestingUserProfile == null || requestingUserProfile.Role == 3)
                    {
                        // Enregistrement de la photo de profil
                        if (!String.IsNullOrWhiteSpace(data.ProfilePicture))
                        {
                            ImageModel profilePicture = JsonConvert.DeserializeObject<ImageModel>(data.ProfilePicture);
                            var imgResult = await _imageService.SaveProfilePicture(profilePicture, userToUpdate.Id);
                            if (!imgResult.Succeeded) return imgResult;
                        }

                        if (userToUpdate.Description != data.Description) userToUpdate.Description = data.Description;
                        if (!String.IsNullOrWhiteSpace(data.Username) && userToUpdate.UserName != data.Username) userToUpdate.UserName = data.Username;
                        if (!String.IsNullOrWhiteSpace(data.Email) && userToUpdate.Email != data.Email) userToUpdate.Email = data.Email;
                        if (data.Role != 0 && userToUpdate.Role != data.Role) userToUpdate.Role = data.Role;
                        if (data.IsBanned != userToUpdate.IsBanned) userToUpdate.IsBanned = data.IsBanned;

                        var result = await _userManager.UpdateAsync(userToUpdate);

                        if (result.Succeeded)
                        {
                            return new MyResponse { Succeeded = true };
                        }
                        else
                        {
                            var error = new MyResponse { Succeeded = false };
                            result.Errors.ToList().ForEach(err =>
                            {
                                error.Messages.Add(err.Code);
                            });
                            return error;
                        }
                    }
                    else
                    {
                        var userError = new MyResponse { Succeeded = false };
                        userError.Messages.Add("UserNotAuthorized");

                        return userError;
                    }
                }
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet à un utilisateur de supprimer son compte en spécifiant son mot de passe actuel pour plus de sécurité
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> DeleteUserAccount(string username, string password)
        {
            if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user != null)
                {
                    // Afin de sécuriser la suppression de compte, on demande à l'utilisateur d'entrer son mot de passe
                    // Si le mot de passe est bon, on supprime son compte
                    var signinResult = await _signInManager.PasswordSignInAsync(user, password, false, false);
                    if (signinResult.Succeeded)
                    {
                        // Comme une suppression pure et simple risquerait de mettre à mal toute l'application, on préférera l'anonymyser
                        user.UserName = $"DeletedUser-{Guid.NewGuid()}"; // On ajoute un guid avec le préfixe DeletedUser pour une gestion plus simple lorsqu'on essaiera de voir les posts de cet utilisateur
                        user.Email = $"deleted-{Guid.NewGuid()}@deleted.com";
                        user.PasswordHash = "";
                        user.Role = 0;
                        user.Description = "";
                        var deleteResult = await _userManager.UpdateAsync(user);
                        if (deleteResult.Succeeded)
                        {
                            return new MyResponse { Succeeded = true };
                        }
                        else
                        {
                            var error = new MyResponse { Succeeded = false };
                            deleteResult.Errors.ToList().ForEach(err =>
                            {
                                error.Messages.Add(err.Code);
                            });
                            return error;
                        }
                    }
                    else
                    {
                        var error = new MyResponse { Succeeded = false };
                        error.Messages.Add("IncorrectPassword");
                        return error;
                    }
                }
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de récupérer une liste d'utilisateurs via leur nom d'utilisateur
        /// </summary>
        /// <param name="username"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> SearchUserByUsername(string username, string currentUsername)
        {
            if (!String.IsNullOrWhiteSpace(username))
            {
                // On pense à excluse l'utilisateur actuel de la liste pour qu'il ne puisse pas s'envoyer de message
                var users = await _userManager.Users.Where(user => user.UserName.Contains(username) && user.UserName != currentUsername).ToListAsync();

                var response = new MyResponse { Succeeded = true };
                response.Result = users;

                return response;
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de bannir / débannir un utilisateur via son pseudo
        /// </summary>
        /// <param name="usernameToBan"></param>
        /// <param name="actualUser"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> UpdateUserBan(string usernameToBan, string actualUser, bool isBanned)
        {
            if (!String.IsNullOrWhiteSpace(usernameToBan) && !String.IsNullOrWhiteSpace(actualUser))
            {
                // Seuls les modérateurs et les admins (rang 2 min) peuvent bannir d'autres utilisateur
                var user = await _userManager.FindByNameAsync(actualUser);
                if (user != null && user.Role >= 2)
                {
                    var userToBan = await _userManager.FindByNameAsync(usernameToBan);
                    if (userToBan != null)
                    {
                        userToBan.IsBanned = isBanned;
                        var updateResult = await _userManager.UpdateAsync(userToBan);

                        return new MyResponse { Succeeded = updateResult.Succeeded };
                    }

                    var userError = new MyResponse { Succeeded = false };
                    userError.Messages.Add("UserNotFound");

                    return userError;
                }

                var error = new MyResponse { Succeeded = false };
                error.Messages.Add("UserNotAuthorized");

                return error;
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de récupérer la liste des utilisateurs (administrateurs uniquement)
        /// </summary>
        /// <param name="page"></param>
        /// <param name="role"></param>
        /// <returns>MyResponse</returns>
        public MyResponse GetUsersWithPagination(int page, int role, string search = "", int filter = 0)
        {
            var response = new MyResponse { Succeeded = false };

            if (role == 3)
            {
                int userPage = page * 10;
                int count = 0;
                List<User> users = new List<User>();
                
                // Paramètre optionnel recherche
                if (String.IsNullOrWhiteSpace(search))
                {
                    count = _userManager.Users.Where(user => filter == 0 ? true : filter == 1 ? !user.IsBanned : user.IsBanned).Count();
                    users = _userManager.Users.Where(user => filter == 0 ? true : filter == 1 ? !user.IsBanned : user.IsBanned).OrderBy(user => user.UserName).Skip(userPage).Take(10).ToList();
                }
                else
                {
                    count = _userManager.Users.Where(user => user.UserName.Contains(search)).Where(user => filter == 0 ? true : filter == 1 ? !user.IsBanned : user.IsBanned).Count();
                    users = _userManager.Users.Where(user => user.UserName.Contains(search)).Where(user => filter == 0 ? true : filter == 1 ? !user.IsBanned : user.IsBanned).OrderBy(user => user.UserName).Skip(userPage).Take(10).ToList();
                }

                var usersToSend = new List<UserResponse>();

                foreach(var user in users)
                {
                    usersToSend.Add(new UserResponse
                    {
                        Id = user.Id,
                        Email = user.Email,
                        UserName = user.UserName,
                        Role = user.Role
                    });
                }

                // On renvoie avec le nombre total d'utilisateurs pour la pagination
                var toRespond = new UserPaginationResponse
                {
                    Count = count,
                    Users = usersToSend
                };

                response = new MyResponse { Succeeded = true, Result = toRespond };
            }

            return response;
        }
    }
}
