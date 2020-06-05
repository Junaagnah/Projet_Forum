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
    /// Controller permettant la gestion des catégories
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ITokenReaderService _tokenReaderService;

        public CategoryController(ICategoryService categoryService, ITokenReaderService tokenReaderService)
        {
            _categoryService = categoryService;
            _tokenReaderService = tokenReaderService;
        }

        /// <summary>
        /// Permet de récupérer la liste des catégories conformément au rôle de l'utilisateur
        /// </summary>
        /// <returns>MyResponse</returns>
        [HttpPost("GetCategories")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("GetCategories", Description = "Permet de récupérer la liste des catégories")]
        public IActionResult GetCategories()
        {
            var role = _tokenReaderService.GetTokenRole(HttpContext.Request.Headers["Authorization"]);
            if (role == null) role = "0";

            var response = _categoryService.GetCategories(Convert.ToInt32(role));

            return Ok(response);
        }

        /// <summary>
        /// Permet de récupérer une catégorie
        /// </summary>
        /// <param name="catId"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("GetCategory")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("GetCategory", Description = "Permet de récupérer une catégorie")]
        public IActionResult GetCategory(int catId)
        {
            var result = _categoryService.GetCategory(catId);

            return Ok(result);
        }

        /// <summary>
        /// Permet à l'administrateur de récupérer la liste des catégories dans l'administration 
        /// </summary>
        /// <param name="page"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("GetCategoriesWithPagination")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("GetCategoriesWithPagination", Description = "Permet à l'administrateur de récupérer la liste des catégories dans l'administration")]
        public IActionResult GetCategoriesWithPagination(int page)
        {
            int role = Convert.ToInt32(_tokenReaderService.GetTokenRole(HttpContext.Request.Headers["Authorization"]));

            var result = _categoryService.GetCategoriesWithPagination(role, page);

            return Ok(result);
        }

        /// <summary>
        /// Permet à l'administrateur de supprimer des catégories
        /// </summary>
        /// <param name="id"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("DeleteCategory")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("DeleteCategory", Description = "Permet à l'administrateur de supprimer des catégories")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            int role = Convert.ToInt32(_tokenReaderService.GetTokenRole(HttpContext.Request.Headers["Authorization"]));

            var result = await _categoryService.DeleteCategory(id, role);

            return Ok(result);
        }

        /// <summary>
        /// Permet à l'administrateur de créer des catégories
        /// </summary>
        /// <param name="category"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("CreateCategory")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("CreatCategory", Description = "Permet à l'administrateur de créer des catégories")]
        public async Task<IActionResult> CreateCategory(CategoryViewModel category)
        {
            int role = Convert.ToInt32(_tokenReaderService.GetTokenRole(HttpContext.Request.Headers["Authorization"]));

            var result = await _categoryService.CreateCategory(category, role);

            return Ok(result);
        }

        /// <summary>
        /// Permet à l'administrateur de modifier des catégories
        /// </summary>
        /// <param name="category"></param>
        /// <returns>MyResponse</returns>
        [HttpPost("UpdateCategory")]
        [ProducesResponseType(typeof(MyResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [OpenApiTag("UpdateCategory", Description = "Permet à l'administrateur de modifier des catégories")]
        public async Task<IActionResult> UpdateCategory(CategoryViewModel category)
        {
            int role = Convert.ToInt32(_tokenReaderService.GetTokenRole(HttpContext.Request.Headers["Authorization"]));

            var result = await _categoryService.UpdateCategory(category, role);

            return Ok(result);
        }
    }
}