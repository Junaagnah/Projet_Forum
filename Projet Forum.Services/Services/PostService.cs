using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Services.Models;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;
using Projet_Forum.Services.ViewModels;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Projet_Forum.Services.Services
{
    /// <summary>
    /// Service permettant de gérer les posts
    /// </summary>
    public class PostService : IPostService
    {
        private readonly IPostRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICommentService _commentService;
        private readonly IImageService _imageService;

        private const string defaultProfilePicture = "images/defaultProfilePicture.jpg";

        public PostService(IPostRepository repository, IIdentityService identityService, ICategoryRepository categoryRepository, ICommentService commentService, IImageService imageService)
        {
            _repository = repository;
            _identityService = identityService;
            _categoryRepository = categoryRepository;
            _commentService = commentService;
            _imageService = imageService;
        }

        /// <summary>
        /// Permet de créer un post
        /// </summary>
        /// <param name="post"></param>
        /// <param name="authorUsername"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> CreatePost(PostViewModel post, string authorUsername)
        {
            if (post != null && !String.IsNullOrWhiteSpace(authorUsername)
                && !String.IsNullOrWhiteSpace(post.Title)
                && !String.IsNullOrWhiteSpace(post.Body))
            {
                var user = await _identityService.GetUserByUsername(authorUsername);
                if (user != null)
                {
                    // On vérifie si la catégorie existe bien
                    var category = _categoryRepository.GetCategory(post.Category);
                    if (category != null)
                    {
                        // On set l'Id de l'auteur
                        var newPost = new Post { Author = user.Id, Title = post.Title, Body = post.Body, Category = post.Category, Locked = post.Locked };
                        var postId = await _repository.CreatePost(newPost);

                        // Enregistrement de l'image liée au post
                        if (!String.IsNullOrWhiteSpace(post.Image))
                        {
                            var img = JsonConvert.DeserializeObject<ImageModel>(post.Image);
                            await _imageService.SavePostImage(img, postId);
                        }

                        var response = new MyResponse { Succeeded = true };
                        response.Result = postId;

                        return response;
                    }
                    else
                    {
                        var error = new MyResponse { Succeeded = false };
                        error.Messages.Add("CategoryNotFound");
                        return error;
                    }
                }
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de supprimer un post
        /// </summary>
        /// <param name="id"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> DeletePost(int id, string username)
        {
            var user = await _identityService.GetUserByUsername(username);
            var postResponse = await GetPost(id);
            // On vérifie que la personne tentant de supprimer le post est bien auteure de celui-ci ou au moins de rang modérateur
            if (postResponse.Result is Post post && user != null && (user.Id == post.Author || user.Role >= 2))
            {
                var result = await _repository.DeletePost(id);

                // On supprime l'image liée au post s'il y en a une
                await _imageService.DeletePostImage(post.Id);

                // Suppression des commentaires du post
                if (result) await _commentService.DeleteCommentsByPost(id);

                return new MyResponse { Succeeded = result };
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de récupérer un post via son Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> GetPost(int id)
        {
            Post post = _repository.GetPost(id);

            if (post != null)
            {
                // On rajoute le nom de l'auteur à la réponse
                var user = await _identityService.GetUserById(post.Author);
                if (user != null)
                    post.AuthorUsername = user.UserName;
                else
                    // Si on ne trouve pas l'utilisateur, on renvoie quand même le post avec le nom d'utilisateur 'Utilisateur inexistant'
                    post.AuthorUsername = "Utilisateur inexistant";

                MyResponse response = new MyResponse { Succeeded = true };
                response.Result = post;

                return response;
            }

            var error = new MyResponse { Succeeded = false };
            error.Messages.Add("PostNotFound");

            return error;
        }

        /// <summary>
        /// Permet de récupérer un post via sa catégorie et son id
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="catId"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> GetPostByCategory(int postId, int catId)
        {
            Post post = _repository.GetPostByCategory(postId, catId);

            if (post != null)
            {
                PostResponse toReturn = new PostResponse
                {
                    Id = post.Id,
                    Title = post.Title,
                    Body = post.Body,
                    Locked = post.Locked
                };

                // On rajoute le nom de l'auteur à la réponse
                var user = await _identityService.GetUserById(post.Author);
                if (user != null)
                {
                    // Si l'utilisateur existe, on ajoute son profil à la réponse pour pouvoir afficher sa photo de profil & son rôle
                    var userProfile = await _identityService.GetUserProfileByUsername(user.UserName);
                    toReturn.AuthorProfile = userProfile.Result as ProfileResponse;

                }
                else
                    // Si on ne trouve pas l'utilisateur, on renvoie quand même le post avec le nom d'utilisateur 'Utilisateur inexistant'
                    toReturn.AuthorProfile = new ProfileResponse { Username = "Utilisateur inexistant", ProfilePicture = defaultProfilePicture };

                // Récupération de l'image liée au post, s'il y en a une
                toReturn.Image = _imageService.GetPostImagePath(post.Id);

                // Récupération des réponses
                toReturn.Comments = await _commentService.GetCommentsByPost(post.Id);

                MyResponse response = new MyResponse { Succeeded = true };
                response.Result = toReturn;

                return response;
            }

            var error = new MyResponse { Succeeded = false };
            error.Messages.Add("PostNotFound");

            return error;
        }

        /// <summary>
        /// Permet de récupérer la liste des posts
        /// </summary>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> GetPosts()
        {
            var posts = _repository.GetPosts();

            if (posts != null)
            {
                // On assigne le nom des auteurs à chaque post
                foreach(var post in posts)
                {
                    var user = await _identityService.GetUserById(post.Author);

                    if (user != null)
                        post.AuthorUsername = user.UserName;
                    else
                        post.AuthorUsername = "Utilisateur inexistant";
                }

                MyResponse response = new MyResponse { Succeeded = true };
                response.Result = posts;

                return response;

            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de récupérer la liste des posts par catégorie
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="isIndex"></param>
        /// <returns>Liste de posts</returns>
        public async Task<List<Post>> GetPostsByCategory(int categoryId, int page = 0, bool isIndex = false)
        {
            var posts = _repository.GetPostsByCategory(categoryId, page, isIndex);

            if (posts != null)
            {
                // On assigne le nom des auteurs à chaque post
                foreach (var post in posts)
                {
                    var user = await _identityService.GetUserById(post.Author);

                    if (user != null)
                        post.AuthorUsername = user.UserName;
                    else
                        post.AuthorUsername = "Utilisateur inexistant";
                }

            }

            return posts;
        }

        /// <summary>
        /// Permet de mettre à jour un post
        /// </summary>
        /// <param name="post"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> UpdatePost(PostViewModel post, string username)
        {
            var result = false;

            if (post != null)
            {
                var user = await _identityService.GetUserByUsername(username);
                var postResponse = await GetPost(post.Id);

                // On modifie le post uniquement si la requête vient de l'auteur ou au moins d'un modérateur
                if (postResponse.Result is Post oldPost && user != null && (oldPost.Author == user.Id || user.Role >= 2))
                {
                    if (!String.IsNullOrWhiteSpace(post.Title) || !String.IsNullOrWhiteSpace(post.Body))
                    {
                        // On vérifie si la catégorie existe bien
                        var category = _categoryRepository.GetCategory(post.Category);
                        if (category != null)
                        {
                            var newPost = new Post { Id = post.Id, Title = post.Title, Body = post.Body, Category = post.Category, Locked = post.Locked };
                            result = await _repository.UpdatePost(newPost);

                            if (!String.IsNullOrWhiteSpace(post.Image))
                            {
                                var img = JsonConvert.DeserializeObject<ImageModel>(post.Image);
                                await _imageService.SavePostImage(img, post.Id);
                            }
                        }
                        else
                        {
                            var error = new MyResponse { Succeeded = false };
                            error.Messages.Add("CategoryNotFound");
                            return error;
                        }
                    }
                }
            }

            return new MyResponse { Succeeded = result };
        }

        /// <summary>
        /// Permet de supprimer tous les posts appartenant à une catégorie
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns>True ou false</returns>
        public async Task<bool> DeletePostsByCategory(int categoryId)
        {
            var posts = _repository.GetPostsByCategory(categoryId);

            // Suppression des commentaires de tous les posts
            if (posts != null)
            {
                foreach (var post in posts)
                {
                    await _commentService.DeleteCommentsByPost(post.Id);
                }
            }

            return await _repository.DeletePostsByCategory(categoryId);
        }

        /// <summary>
        /// Permet de supprimer une image attachée à un post
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="username"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> DeletePostImage(int postId, string username)
        {
            if (!String.IsNullOrWhiteSpace(username))
            {
                var post = _repository.GetPost(postId);
                if (post != null)
                {
                    var user = await _identityService.GetUserByUsername(username);
                    if (user != null && (post.Author == user.Id || user.Role >= 2))
                    {
                        var result = await _imageService.DeletePostImage(postId);

                        return result;
                    }

                    var userError = new MyResponse { Succeeded = false };
                    userError.Messages.Add("UserNotAuthorized");

                    return userError;
                }

                var postError = new MyResponse { Succeeded = false };
                postError.Messages.Add("PostNotFound");

                return postError;
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de récupérer une liste de posts avec pagination via une catégorie
        /// </summary>
        /// <param name="catId"></param>
        /// <param name="page"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> GetPostsWithPagination(int catId, int page, int role)
        {
            var response = new MyResponse { Succeeded = false };

            // On vérifie bien que la catégorie existe et que l'utilisateur a le droit d'y accéder
            var category = _categoryRepository.GetCategory(catId);
            if (category != null && category.Role <= role)
            {
                var posts = await GetPostsByCategory(catId, page);
                var postCount = _repository.GetPostsCountByCategory(catId);

                response = new MyResponse { Succeeded = true, Result = new PostPaginationResponse { Count = postCount, Posts = posts } };

            }

            return response;
        }
    }
}
