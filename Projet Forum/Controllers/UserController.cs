using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Services.Models;
using Projet_Forum.Services.ViewModels;

namespace Projet_Forum.Controllers
{
    /// <summary>
    /// Controller permettant la gestion des utilisateurs via identity
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class UserController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenReaderService _tokenReaderService;

        public UserController(IIdentityService identityService, ITokenReaderService tokenReaderService)
        {
            _identityService = identityService;
            _tokenReaderService = tokenReaderService;
        }

        /// <summary>
        /// Permet à l'utilisateur de récupérer son profil via son token
        /// </summary>
        /// <returns>MyResponse</returns>
        [HttpPost("GetSelfProfile")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("GetSelfProfile", Description = "Permet à un utilisateur de récupérer son profil via son token")]
        public async Task<IActionResult> GetSelfProfile()
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var response = await _identityService.GetUserProfileByUsername(username);
            return Ok(response);
        }

        /// <summary>
        /// Renvoie le profil d'un utilisateur via son nom d'utilisateur
        /// </summary>
        /// <param name="username"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("GetUserProfile")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [OpenApiTag("GetUserProfile", Description = "Permet à un utilisateur de récupérer un profil via son nom d'utilisateur")]
        public async Task<IActionResult> GetUserProfile(string username)
        {
            // Accessible par tout le monde car les profils sont publics
            var response = await _identityService.GetUserProfileByUsername(username);

            return Ok(response);
        }

        /// <summary>
        /// Permet à l'utilisateur de modifier son mot de passe via son token
        /// </summary>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("UpdateSelfPassword")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("UpdateSelfPassword", Description = "Permet à un utilisateur de mettre à jour son mot de passe via son token")]
        public async Task<IActionResult> UpdateSelfPassword(string oldPassword, string newPassword)
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var response = await _identityService.UpdateUserPassword(username, oldPassword, newPassword);

            return Ok(response);
        }

        /// <summary>
        /// Permet à un utilisateur de mettre à jour son profil via son token
        /// </summary>
        /// <param name="data"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("UpdateSelfProfile")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("UpdateSelfProfile", Description = "Permet à l'utilisateur de mettre à jour son profil via son token")]
        public async Task<IActionResult> UpdateSelfProfile(UpdateProfileViewModel data)
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var response = await _identityService.UpdateUserProfile(username, data);

            return Ok(response);
        }

        /// <summary>
        /// Permet à un utilisateur de supprimer son compte via son token
        /// </summary>
        /// <param name="password"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("DeleteSelfProfile")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("DeleteSelfProfile", Description = "Permet à l'utilisateur de supprimer son compte via son token")]
        public async Task<IActionResult> DeleteSelfProfile(string password)
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var response = await _identityService.DeleteUserAccount(username, password);

            return Ok(response);
        }

        /// <summary>
        /// Permet de rechercher un utilisateur via son nom d'utilisateur
        /// </summary>
        /// <param name="username"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("SearchUser")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("SearchUser", Description = "Permet de rechercher un utilisateur via son nom d'utilisateur")]
        public async Task<IActionResult> SearchUser(string username)
        {
            var currentUsername = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var users = await _identityService.SearchUserByUsername(username, currentUsername);

            return Ok(users);
        }

        /// <summary>
        /// Permet de mettre à jour le statut banni d'un utilisateur
        /// </summary>
        /// <param name="username"></param>
        /// <param name="isBanned"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("UpdateUserBan")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("UpdateUserBan", Description = "Permet de mettre à jour le statut banni d'un utilisateur")]
        public async Task<IActionResult> UpdateUserBan(string username, bool isBanned)
        {
            var actualUser = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var result = await _identityService.UpdateUserBan(username, actualUser, isBanned);

            return Ok(result);
        }
    }
}