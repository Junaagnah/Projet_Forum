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
    /// Controller contenant toutes les fonctions liées à l'administration
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class AdministrationController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenReaderService _tokenReaderService;

        public AdministrationController(IIdentityService identityService, ITokenReaderService tokenReaderService)
        {
            _identityService = identityService;
            _tokenReaderService = tokenReaderService;
        }

        /// <summary>
        /// Renvoie la liste des utilisateurs avec pagination
        /// </summary>
        /// <param name="page"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("GetUsers")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("GetUsers", Description = "Renvoie la liste des utilisateurs avec pagination")]
        public IActionResult GetUsers(int page, string search = "", int filter = 0)
        {
            var role = Convert.ToInt32(_tokenReaderService.GetTokenRole(HttpContext.Request.Headers["Authorization"]));

            var result = _identityService.GetUsersWithPagination(page, role, search, filter);

            return Ok(result);
        }

        /// <summary>
        /// Permet à un administrateur de mettre à jour un profil utilisateur
        /// </summary>
        /// <param name="username"></param>
        /// <param name="data"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("UpdateUser")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("UpdateUser", Description = "Permet à un administrateur de mettre à jour un profil utilisateur")]
        public async Task<IActionResult> UpdateUser(string username, UpdateProfileViewModel data)
        {
            var requestingUser = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var result = await _identityService.UpdateUserProfile(username, data, requestingUser);

            return Ok(result);
        }
    }
}