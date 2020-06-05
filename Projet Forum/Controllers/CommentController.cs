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
    /// Controller contenant toutes les fonctions liées aux commentaires
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ITokenReaderService _tokenReaderService;

        public CommentController(ICommentService commentService, ITokenReaderService tokenReaderService)
        {
            _commentService = commentService;
            _tokenReaderService = tokenReaderService;
        }

        /// <summary>
        /// Permet de récupérer un commentaire via son id
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("GetComment")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("GetComment", Description = "Permet de récupérer un commentaire")]
        public async Task<IActionResult> GetComment(int commentId)
        {
            var response = await _commentService.GetComment(commentId);

            return Ok(response);
        }

        /// <summary>
        /// Permet à l'utilisateur de répondre à un post via un commentaire
        /// </summary>
        /// <param name="comment"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("CreateComment")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("CreateComment", Description = "Permet à l'utilisateur de répondre à un post via un commentaire")]
        public async Task<IActionResult> AddComment(CommentViewModel comment)
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var response = await _commentService.CreateComment(comment, username);

            return Ok(response);
        }

        /// <summary>
        /// Permet à l'utilisateur de supprimer un de ses commentaires
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("DeleteComment")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("DeleteComment", Description = "Permet à l'utilisateur de supprimer un de ses commentaires")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var response = await _commentService.DeleteComment(commentId, username);

            return Ok(response);
        }

        /// <summary>
        /// Permet à l'utilisateur de modifier un de ses commentaires
        /// </summary>
        /// <param name="comment"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("EditComment")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("EditComment", Description = "Permet à l'utilisateur de modifier un de ses commentaires")]
        public async Task<IActionResult> EditComment(CommentViewModel comment)
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var response = await _commentService.EditComment(comment, username);

            return Ok(response);
        }
    }
}