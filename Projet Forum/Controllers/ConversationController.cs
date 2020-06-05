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
    /// Controller permettant la gestion des messages et des conversations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _convService;
        private readonly IMessageService _messageService;
        private readonly ITokenReaderService _tokenReaderService;

        public ConversationController(IConversationService convService, IMessageService messageService, ITokenReaderService tokenReaderService)
        {
            _convService = convService;
            _messageService = messageService;
            _tokenReaderService = tokenReaderService;
        }

        /// <summary>
        /// Permet de récupérer les conversations d'un utilisateur via son token
        /// </summary>
        /// <returns>MyResponse</returns>
        [HttpPost("GetConversations")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("GetConversations", Description = "Permet de récupérer les conversations d'un utilisateur via son token")]
        public async Task<IActionResult> GetConversations()
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var result = await _convService.GetConversationsByUsername(username);

            return Ok(result);
        }

        /// <summary>
        /// Permet de récupérer les messages d'une conversation
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("GetMessages")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("GetMessages", Description = "Permet de récupérer les messages d'une conversation")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var result = await _messageService.GetMessagesByConversation(conversationId, username);

            return Ok(result);
        }

        /// <summary>
        /// Permet à un utilisateur d'envoyer un message à un autre utilisateur via son token
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [HttpPost("CreateMessage")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("CreateMessage", Description = "Permet à un utilisateur d'envoyer un message à un autre utilisateur")]
        public async Task<IActionResult> CreateMessage(MessageViewModel msg)
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var result = await _messageService.CreateMessage(msg, username);

            return Ok(result);
        }

        /// <summary>
        /// Permet à un utilisateur de supprimer un message via son id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("DeleteMessage")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("DeleteMessage", Description = "Permet à un utilisateur de supprimer un message via son id")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var result = await _messageService.DeleteMessage(id, username);

            return Ok(result);
        }
    }
}