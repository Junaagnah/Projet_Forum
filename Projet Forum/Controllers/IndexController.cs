using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projet_Forum.Services.Models;
using Projet_Forum.Services.ViewModels;
using Projet_Forum.Services.Interfaces;
using NSwag.Annotations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;

namespace Projet_Forum.Controllers
{
    /// <summary>
    /// Controller contenant toutes les fonctionnalités de base (login, signin, etc)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class IndexController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ICategoryService _categoryService;
        private readonly ISendgridService _sendgridService;
        private readonly ITokenReaderService _tokenReaderService;

        public IndexController(IIdentityService identityService, ICategoryService categoryService, ISendgridService sendgridService, ITokenReaderService tokenReaderService)
        {
            _identityService = identityService;
            _categoryService = categoryService;
            _sendgridService = sendgridService;
            _tokenReaderService = tokenReaderService;
        }

        /// <summary>
        /// Récupération de la liste des catégories et des posts
        /// </summary>
        /// <returns>Liste de catégories avec les derniers posts dedans</returns>
        [HttpPost("Index")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [OpenApiTag("Index", Description = "Renvoie la liste des catégories et leurs 10 derniers posts")]
        public async Task<IActionResult> Index()
        {
            var role = _tokenReaderService.GetTokenRole(HttpContext.Request.Headers["Authorization"]);
            var intRole = 0;
            if (role != null) intRole = Convert.ToInt32(role); // On set le role à 0 par défaut s'il est nul

            // On précise qu'on ne veut que les 10 derniers posts des catégories
            MyResponse response = await _categoryService.GetCategoriesAndPosts(true, intRole);

            return Ok(response);
        }

        /// <summary>
        /// Enregistre un utilisateur
        /// </summary>
        /// <returns>MyResponse</returns>
        [HttpPost("Register")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [OpenApiTag("Register", Description = "Contrôleur pour l'inscription d'un utilisateur")]
        public async Task<IActionResult> Register(RegisterViewModel user)
        {
            var registerResult = await _identityService.RegisterUserAsync(user);

            return Ok(registerResult);
        }

        /// <summary>
        /// Connecte un utilisateur
        /// </summary>
        /// <returns>MyResponse</returns>
        [HttpPost("Signin")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [OpenApiTag("Signin", Description = "Contrôleur pour la connexion d'un utilisateur")]
        public async Task<IActionResult> Signin(string email, string password)
        {
            var signinResult = await _identityService.SignInUserAsync(email, password);

            return Ok(signinResult);
        }

        /// <summary>
        /// Confirme le compte d'un utilisateur via son id et son token
        /// </summary>
        /// <returns>MyResponse</returns>
        [HttpPost("ConfirmEmail")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [OpenApiTag("ConfirmEmail", Description = "Confirme le compte d'un utilisateur via son id et un token")]
        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            var confirmResult = await _identityService.ConfirmEmail(id, token);

            return Ok(confirmResult);
        }

        /// <summary>
        /// Envoie un mail à l'utilisateur contenant un lien pour qu'il puisse modifer son mot de passe
        /// </summary>
        /// <returns>MyResponse</returns>
        [HttpPost("AskPasswordRecovery")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [OpenApiTag("AskPasswordRecovery", Description = "Envoie un mail permettant à l'utilisateur de réinitialiser son mot de passe")]
        public async Task<IActionResult> AskPasswordRecovery(string email)
        {
            var confirmResult = await _identityService.AskPasswordRecovery(email);

            return Ok(confirmResult);
        }

        /// <summary>
        /// Permet à l'utilisateur de modifier son mot de passe avec un token
        /// </summary>
        /// <returns>MyResponse</returns>
        [HttpPost("RecoverPassword")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [OpenApiTag("RecoverPassword", Description = "Permet à l'utilisateur de modifier son mot de passe avec un token")]
        public async Task<IActionResult> RecoverPassword(string userId, string password, string token)
        {
            var confirmResult = await _identityService.RecoverPassword(userId, password, token);

            return Ok(confirmResult);
        }

        /// <summary>
        /// Permet à l'utilisateur non inscrit de contacter les administrateurs
        /// </summary>
        /// <returns>MyResponse</returns>
        [HttpPost("Contact")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [OpenApiTag("Contact", Description = "Permet à l'utilisateur non inscrit de contacter l'administrateur")]
        public async Task<IActionResult> Contact(string email, string message)
        {
            var contactResult = await _sendgridService.SendContactEmail(email, message);

            return Ok(contactResult);
        }

        /// <summary>
        /// Renouvellement d'un refreshToken
        /// </summary>
        /// <returns>MyResponse</returns>
        [HttpPost("RenewToken")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [OpenApiTag("RenewToken", Description = "Renouvellement du refreshToken d'un utilisateur")]
        public async Task<IActionResult> RenewToken(string username, string refreshToken)
        {
            var result = await _identityService.RenewToken(username, refreshToken);

            return Ok(result);
        }

        /// <summary>
        /// Déconnection d'un utilisateur
        /// </summary>
        /// <returns>MyResponse</returns>
        [HttpPost("Disconnect")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("Disconnect", Description = "Déconnection d'un utilisateur")]
        public async Task<IActionResult> Disconnect()
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);
            await _identityService.Disconnect(username);

            return Ok();
        }
    }
}