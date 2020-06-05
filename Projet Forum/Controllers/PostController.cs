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
    /// Controller permettant de gérer les posts
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ITokenReaderService _tokenReaderService;

        public PostController(IPostService postService, ITokenReaderService tokenReaderService)
        {
            _postService = postService;
            _tokenReaderService = tokenReaderService;
        }

        /// <summary>
        /// Permet à un utilisateur de créer un post
        /// </summary>
        /// <param name="post"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("CreatePost")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("CreatePost", Description = "Permet à un utilisateur de créer un post")]
        public async Task<IActionResult> CreatePost(PostViewModel post)
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var result = await _postService.CreatePost(post, username);

            return Ok(result);
        }

        /// <summary>
        /// Permet à un utilisateur de récupérer un post via son id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("GetPost")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [OpenApiTag("GetPost", Description = "Permet à un utilisateur de récupérer un post via son id")]
        public async Task<IActionResult> GetPostByCategory(int postId, int catId)
        {
            var result = await _postService.GetPostByCategory(postId, catId);

            return Ok(result);
        }

        /// <summary>
        /// Permet de récupérer les posts d'une catégorie avec pagination
        /// </summary>
        /// <param name="catId"></param>
        /// <param name="page"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("GetPostsByCategory")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [OpenApiTag("GetPostsByCategory", Description = "Permet de récupérer les posts d'une catégorie avec pagination")]
        public async Task<IActionResult> GetPostsByCategory(int catId, int page)
        {
            var role = _tokenReaderService.GetTokenRole(HttpContext.Request.Headers["Authorization"]);
            
            var response = await _postService.GetPostsWithPagination(catId, page, Convert.ToInt32(role));

            return Ok(response);
        }

        /// <summary>
        /// Permet à un utilisateur de supprimer un post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("DeletePost")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("DeletePost", Description = "Permet à un utilisateur de supprimer un post")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var result = await _postService.DeletePost(postId, username);

            return Ok(result);
        }

        /// <summary>
        /// Permet à un utilisateur de modifier un post
        /// </summary>
        /// <param name="post"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("UpdatePost")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("UpdatePost", Description = "Permet à un utilisateur de modifier un post")]
        public async Task<IActionResult> UpdatePost(PostViewModel post)
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var result = await _postService.UpdatePost(post, username);

            return Ok(result);
        }

        /// <summary>
        /// Permet à un utilisateur de supprimer l'image attachée à un post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("DeletePostImage")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("DeletePostImage", Description = "Permet de supprimer une image attachée à un post")]
        public async Task<IActionResult> DeletePostImage(int postId)
        {
            var username = _tokenReaderService.GetTokenUsername(HttpContext.Request.Headers["Authorization"]);

            var result = await _postService.DeletePostImage(postId, username);

            return Ok(result);
        }
    }
}