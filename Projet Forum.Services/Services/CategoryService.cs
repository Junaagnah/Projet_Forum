using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Services.Models;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;
using Projet_Forum.Services.ViewModels;

namespace Projet_Forum.Services.Services
{
    /// <summary>
    /// Service permettant de gérer les catégories
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IPostService _postService;

        public CategoryService(ICategoryRepository repository, IPostService postService)
        {
            _repository = repository;
            _postService = postService;
        }

        /// <summary>
        /// Permet de créer une catégorie
        /// </summary>
        /// <param name="category"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> CreateCategory(CategoryViewModel category, int role)
        {
            // Seul l'administrateur peut créer des catégories
            if (role == 3)
            {
                if (category != null && !String.IsNullOrWhiteSpace(category.Name))
                {
                    Category catToSave = new Category
                    {
                        Name = category.Name,
                        Description = category.Description,
                        Role = category.Role
                    };

                    await _repository.CreateCategory(catToSave);
                    return new MyResponse { Succeeded = true };
                }

                return new MyResponse { Succeeded = false };
            }

            return new MyResponse { Succeeded = false, Messages = { "UserNotAuthorized" } };
        }

        /// <summary>
        /// Permet de supprimer une catégorie
        /// </summary>
        /// <param name="id"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> DeleteCategory(int id, int role)
        {
            // Seuls les administrateurs peuvent supprimer des catégories
            if (role == 3)
            {
                var result = await _repository.DeleteCategory(id);
                // On supprime également tous les posts de la catégorie si la suppression a fonctionné
                if (result) await _postService.DeletePostsByCategory(id);

                return new MyResponse { Succeeded = result };
            }

            return new MyResponse { Succeeded = false, Messages = { "UserNotAuthorized" } };
        }

        /// <summary>
        /// Permet de récupérer les catégories via un rôle
        /// </summary>
        /// <param name="role"></param>
        /// <returns>MyResponse</returns>
        public MyResponse GetCategories(int role = 0)
        {
            var categories = _repository.GetCategories(role);

            if (categories != null)
            {
                MyResponse response = new MyResponse { Succeeded = true };
                response.Result = categories;

                return response;
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de récupérer les catégories et leurs posts
        /// </summary>
        /// <param name="isIndex"></param>
        /// <param name="role"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> GetCategoriesAndPosts(bool isIndex = false, int role = 0)
        {
            var categories = _repository.GetCategories(role);

            if (categories != null)
            {
                // Récupération des posts
                foreach(Category category in categories)
                {
                    category.Posts = await _postService.GetPostsByCategory(category.Id, 0, true);
                }

                MyResponse response = new MyResponse { Succeeded = true };
                response.Result = categories;

                return response;
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de récupérer une catégorie via son id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MyResponse GetCategory(int id)
        {
            var category = _repository.GetCategory(id);

            if (category != null)
            {
                MyResponse response = new MyResponse { Succeeded = true };
                response.Result = category;

                return response;
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de mettre à jour une catégorie
        /// </summary>
        /// <param name="category"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> UpdateCategory(CategoryViewModel category, int role)
        {
            // Seul l'administrateur peut modifier des catégories
            if (role == 3)
            {
                var result = false;

                // On effectue l'appel que si on ne nous renvoie pas une catégorie nulle ou si le name est vide
                if (category != null && !String.IsNullOrWhiteSpace(category.Name))
                {
                    var catToSave = new Category
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Description = category.Description,
                        Role = category.Role
                    };

                    result = await _repository.UpdateCategory(catToSave);
                }
                    

                return new MyResponse { Succeeded = result };
            }

            return new MyResponse { Succeeded = false, Messages = { "UserNotAuthorized" } };
        }

        /// <summary>
        /// Permet de récupérer les catégories avec pagination pour l'administration
        /// </summary>
        /// <param name="role"></param>
        /// <param name="page"></param>
        /// <returns>MyResponse</returns>
        public MyResponse GetCategoriesWithPagination(int role, int page = 0)
        {
            // On vérifie que l'utilisateur est bien administrateur
            if (role == 3)
            {
                var result = new CategoryPaginationResponse
                {
                    Categories = _repository.GetCategoriesWithPagination(page),
                    Count = _repository.GetCategoryCount()
                };
                

                return new MyResponse { Succeeded = true, Result = result };
            }

            return new MyResponse { Succeeded = false, Messages = { "UserNotAuthorized" } };
        }
    }
}
