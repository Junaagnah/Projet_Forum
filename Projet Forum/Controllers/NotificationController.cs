using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Services.Models;

namespace Projet_Forum.Controllers
{
    /// <summary>
    /// Contrôleur d'API permettant de gérer les notifications
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;
        private readonly ITokenReaderService _tokenReaderService;

        public NotificationController(INotificationService service, ITokenReaderService tokenReaderService)
        {
            _service = service;
            _tokenReaderService = tokenReaderService;
        }

        /// <summary>
        /// Permet de récupérer les notifications d'un utilisateur via son token
        /// </summary>
        /// <returns>MyResponse</returns>
        [HttpPost("GetNotifications")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetNotifications()
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var response = await _service.GetNotificationsByUsername(username);

            return Ok(response);
        }

        /// <summary>
        /// Permet de marquer les notifications d'un utilisateur comme lues via son token
        /// </summary>
        /// <returns>MyResponse</returns>
        [HttpPost("MarkAsRead")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> MarkAsRead()
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var response = await _service.MarkAsReadByUsername(username);

            return Ok(response);
        }

        /// <summary>
        /// Permet de supprimer une notification via son id
        /// </summary>
        /// <param name="notifId"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("DeleteNotification")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> DeleteNotification(int notifId)
        {
            var response = await _service.DeleteNotification(notifId);

            return Ok(response);
        }
    }
}