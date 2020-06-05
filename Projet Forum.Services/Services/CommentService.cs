using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projet_Forum.Data.Models;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Services.Models;
using Projet_Forum.Services.ViewModels;

namespace Projet_Forum.Services.Services
{
    /// <summary>
    /// Service permettant de gérer les commentaires
    /// </summary>
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IPostRepository _postRepository;
        private readonly INotificationService _notificationService;

        private const string defaultProfilePicture = "images/defaultProfilePicture.jpg";

        public CommentService(ICommentRepository repository, IIdentityService identityService, IPostRepository postRepository, INotificationService notificationService)
        {
            _repository = repository;
            _identityService = identityService;
            _postRepository = postRepository;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Permet de récupérer la liste des commentaires d'un post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns>List de commentaires</returns>
        public async Task<List<CommentResponse>> GetCommentsByPost(int postId)
        {
            List<CommentResponse> commentsResponse = new List<CommentResponse>();
            var comments = _repository.GetCommentsByPost(postId);

            if (comments != null)
            {
                foreach (var comment in comments)
                {
                    var user = await _identityService.GetUserById(comment.Author);
                    MyResponse userProfileResponse = null;
                    if (user != null) userProfileResponse = await _identityService.GetUserProfileByUsername(user.UserName);
                    ProfileResponse userProfile = null;
                    if (userProfileResponse != null) userProfile = userProfileResponse.Result as ProfileResponse;
                    if (userProfile == null) userProfile = new ProfileResponse { Username = "Utilisateur inexistant", ProfilePicture = defaultProfilePicture };

                    commentsResponse.Add(new CommentResponse
                    {
                        Id = comment.Id,
                        Body = comment.Body,
                        AuthorProfile = userProfile as ProfileResponse
                    });
                }
            }

            return commentsResponse;
        }

        /// <summary>
        /// Permet de créer un post via un PostViewModel
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="authorUsername"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> CreateComment(CommentViewModel comment, string authorUsername)
        {
            if (comment != null && !String.IsNullOrWhiteSpace(comment.Body) && !String.IsNullOrWhiteSpace(authorUsername))
            {
                var post = _postRepository.GetPost(comment.Post);
                if (post != null)
                {
                    if (!post.Locked)
                    {
                        // On met à jour la date de modification du post pour le faire remonter dans la liste
                        await _postRepository.UpdatePostDate(post.Id);

                        var user = await _identityService.GetUserByUsername(authorUsername);
                        if (user != null)
                        {
                            var newComment = new Comment
                            {
                                Author = user.Id,
                                Post = comment.Post,
                                Body = comment.Body,
                                Date = DateTime.Now
                            };
                            await _repository.CreateComment(newComment);

                            // Création d'une notification pour l'auteur du post
                            if (post.Author != user.Id)
                            {
                                // On la crée uniquement si la personne qui commente n'est pas l'auteure du post 
                                var notif = new Notification
                                {
                                    Context = Notification.Type.Post,
                                    CategoryId = post.Category,
                                    ContextId = post.Id,
                                    UserId = post.Author,
                                    Content = $"<strong>{user.UserName}</strong> a répondu à votre post !"
                                };

                                await _notificationService.CreateNotification(notif);
                            }

                            return new MyResponse { Succeeded = true };
                        }
                    }
                    else
                    {
                        var error = new MyResponse { Succeeded = false };
                        error.Messages.Add("PostLocked");
                        return error;
                    }
                }
                else
                {
                    var error = new MyResponse { Succeeded = false };
                    error.Messages.Add("PostNotFound");
                    return error;
                }
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de supprimer un commentaire
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="username"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> DeleteComment(int commentId, string username)
        {
            var result = false;

            if (!String.IsNullOrWhiteSpace(username))
            {
                var user = await _identityService.GetUserByUsername(username);
                if (user != null)
                {
                    var comment = _repository.GetComment(commentId);
                    // On supprime le commentaire uniquement si l'utilisateur est l'auteur du commentaire ou s'il est au moins modérateur
                    if (comment != null && (comment.Author == user.Id || user.Role >= 2))
                    {
                        await _repository.DeleteComment(comment);
                        result = true;
                    }
                }
            }

            return new MyResponse { Succeeded = result };
        }

        /// <summary>
        /// Permet de supprimer tous les commentaires d'un post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public async Task DeleteCommentsByPost(int postId)
        {
            await _repository.DeleteCommentsByPost(postId);
        }

        /// <summary>
        /// Permet de récupérer un commentaire
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> GetComment(int commentId)
        {
            var comment = _repository.GetComment(commentId);
            if (comment != null)
            {
                var user = await _identityService.GetUserById(comment.Author);
                MyResponse userProfileResponse = null;
                if (user != null) userProfileResponse = await _identityService.GetUserProfileByUsername(user.UserName);
                ProfileResponse userProfile = null;
                if (userProfileResponse != null) userProfile = userProfileResponse.Result as ProfileResponse;   
                if (userProfile == null) userProfile = new ProfileResponse { Username = "Utilisateur inexistant", ProfilePicture = defaultProfilePicture };

                var commentResponse = new CommentResponse
                {
                    Id = comment.Id,
                    Body = comment.Body,
                    Post = comment.Post,
                    AuthorProfile = userProfile as ProfileResponse,
                    Author = comment.Author
                };

                return new MyResponse { Succeeded = true, Result = commentResponse };
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de modifier un commentaire
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="username"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> EditComment(CommentViewModel comment, string username)
        {
            var result = false;

            if (comment != null)
            {
                var user = await _identityService.GetUserByUsername(username);
                var commentResponse = await GetComment(comment.Id);
                if (commentResponse.Result is CommentResponse oldComment && user != null && (oldComment.Author == user.Id || user.Role >= 2))
                {
                    var post = _postRepository.GetPost(comment.Post);
                    if (post != null)
                    {
                        var newComment = new Comment
                        {
                            Id = comment.Id,
                            Body = comment.Body
                        };
                        result = await _repository.EditComment(newComment);
                    }
                    else
                    {
                        var error = new MyResponse { Succeeded = false };
                        error.Messages.Add("PostNotFound");
                        return error;
                    }
                }
            }

            return new MyResponse { Succeeded = result };
        }
    }
}
