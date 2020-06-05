using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;

namespace Projet_Forum.Data.Repositories
{
    /// <summary>
    /// Gères les fonctions CRUD pour les images
    /// </summary>
    public class ImageRepository : IImageRepository
    {
        private readonly MyDbContext _context;

        public ImageRepository(MyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Permet d'enregistrer une photo de profil dans la bdd
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="userId"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> SaveProfilePicture(string name, string path, string userId)
        {
            if (!String.IsNullOrWhiteSpace(name) && !String.IsNullOrWhiteSpace(path) && !String.IsNullOrWhiteSpace(userId))
            {
                // On vérifie que l'utilisateur n'a pas déjà une photo de profil enregistrée
                var actualProfilePicture = _context.Images.FirstOrDefault(img => img.UserId == userId);
                if (actualProfilePicture != null) _context.Images.Remove(actualProfilePicture); // Si oui, on la supprime

                var profilePicture = new Image
                {
                    Name = name,
                    Path = path,
                    UserId = userId
                };
                _context.Images.Add(profilePicture);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Permet de récupérer le filepath d'une image à partir de l'user id (photo de profil)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>string</returns>
        public string GetImagePathByUserId(string userId)
        {
            string result = null;

            var image = _context.Images.FirstOrDefault(img => img.UserId == userId);
            if (image != null) result = image.Path;

            return result;
        }

        /// <summary>
        /// Permet d'enregister une photo attachée à un post dans la bdd
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="postId"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> SavePostImage(string name, string path, int postId)
        {
            if (!String.IsNullOrWhiteSpace(name) && !String.IsNullOrWhiteSpace(path))
            {
                // On vérifie qu'une image n'est pas déjà attachée au post
                var actualImage = _context.Images.FirstOrDefault(img => img.PostId == postId);
                if (actualImage != null) _context.Images.Remove(actualImage);

                var image = new Image
                {
                    Name = name,
                    Path = path,
                    PostId = postId
                };
                _context.Images.Add(image);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Permet de récupérer le filepath d'une image à partir de l'id du post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns>string</returns>
        public string GetPostImagePath(int postId)
        {
            string result = null;

            var image = _context.Images.FirstOrDefault(img => img.PostId == postId);
            if (image != null) result = image.Path;

            return result;
        }

        /// <summary>
        /// Permet de supprimer l'image d'un post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns>boolean</returns>
        public async Task<bool> DeletePostImage(int postId)
        {
            var img = _context.Images.FirstOrDefault(img => img.PostId == postId);

            if (img != null)
            {
                _context.Images.Remove(img);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }
    }
}
