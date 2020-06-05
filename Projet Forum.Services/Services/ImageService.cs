using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Services.Models;

namespace Projet_Forum.Services.Services
{
    /// <summary>
    /// Service permettant de gérer les images
    /// </summary>
    public class ImageService : IImageService
    {
        private const string defaultImagePath = "images/";

        private readonly IImageRepository _repository;
        public ImageService(IImageRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Récupère le filepath de la photo de profil d'un utilisateur via son id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>string</returns>
        public string GetImagePathByUserId(string userId)
        {
            string result = null;

            if (!String.IsNullOrWhiteSpace(userId))
            {
                result = _repository.GetImagePathByUserId(userId);
            }

            return result;
        }

        /// <summary>
        /// Récupère le filepath d'une image attachée à un post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns>string</returns>
        public string GetPostImagePath(int postId)
        {
            var result = _repository.GetPostImagePath(postId);

            return result;
        }

        /// <summary>
        /// Permet d'enregistrer une photo de profil
        /// </summary>
        /// <param name="img"></param>
        /// <param name="userId"></param>
        /// <param name="isTest"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> SaveProfilePicture(ImageModel img, string userId, bool isTest = false)
        {

            if (img != null && !String.IsNullOrWhiteSpace(userId) && !String.IsNullOrWhiteSpace(img.FileName) && !String.IsNullOrWhiteSpace(img.FileType) && !String.IsNullOrWhiteSpace(img.Value))
            {
                if (img.FileType == "image/jpeg" || img.FileType == "image/png")
                {
                    var filePath = await WriteProfilePictureToDisk(img, userId, isTest);

                    var result = await _repository.SaveProfilePicture(img.FileName, filePath, userId);

                    return new MyResponse { Succeeded = result };
                }
                else
                {
                    // Type de fichier invalide, seuls jpeg & png acceptés
                    var error = new MyResponse { Succeeded = false };
                    error.Messages.Add("InvalidFileType");
                    return error;
                }
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet d'enregistrer une image pour un post
        /// </summary>
        /// <param name="img"></param>
        /// <param name="postId"></param>
        /// <param name="isTest"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> SavePostImage(ImageModel img, int postId, bool isTest = false)
        {
            if (img != null && !String.IsNullOrWhiteSpace(img.FileName) && !String.IsNullOrWhiteSpace(img.FileType) && !String.IsNullOrWhiteSpace(img.Value))
            {
                if (img.FileType == "image/jpeg" || img.FileType == "image/png")
                {
                    var filePath = await WritePostImageToDisk(img, postId, isTest);

                    var result = await _repository.SavePostImage(img.FileName, filePath, postId);

                    return new MyResponse { Succeeded = result };
                }
                else
                {
                    var error = new MyResponse { Succeeded = false };
                    error.Messages.Add("InvalidFileType");
                    return error;
                }
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de supprimer l'image attachée à un post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> DeletePostImage(int postId, bool isTest = false)
        {
            // Si c'est un test on set à true
            var fileDeleteResult = isTest ? true : DeletePostImageFromDisk(postId);
            var dataDeleteResult = await _repository.DeletePostImage(postId);

            if (fileDeleteResult && dataDeleteResult) return new MyResponse { Succeeded = true };

            var error = new MyResponse { Succeeded = false };
            if (!fileDeleteResult) error.Messages.Add("FileNotDeleted");
            if (!dataDeleteResult) error.Messages.Add("EntryNotDeleted");

            return error;
        }

        /// <summary>
        /// Permet d'enregistrer une photo de profil sur le disque
        /// </summary>
        /// <param name="img"></param>
        /// <param name="userId"></param>
        /// <param name="isTest"></param>
        /// <returns>string</returns>
        private async Task<string> WriteProfilePictureToDisk(ImageModel img, string userId, bool isTest = false)
        {
            string filePath = null;
            if (img != null)
            {
                // Si l'utilisateur a déjà une photo de profil, on supprime l'ancienne
                var oldProfilePath = GetImagePathByUserId(userId);
                if (oldProfilePath != null) File.Delete($"wwwroot/{oldProfilePath}");

                byte[] bytesImg = Convert.FromBase64String(img.Value);
                var type = (img.FileType == "image/jpeg" ? "jpg" : "png"); // Si ce n'est pas un jpg c'est un png
                filePath = $"{defaultImagePath}{Guid.NewGuid()}.{type}";
                var totalPath = $"wwwroot/{filePath}";

                // Si c'est un test, on n'écrit pas le fichier
                if (!isTest) await File.WriteAllBytesAsync(totalPath, bytesImg);
            }

            return filePath;
        }

        /// <summary>
        /// Permet d'enregistrer l'image attachée à un post sur le disque
        /// </summary>
        /// <param name="img"></param>
        /// <param name="postId"></param>
        /// <param name="isTest"></param>
        /// <returns>string</returns>
        private async Task<string> WritePostImageToDisk(ImageModel img, int postId, bool isTest = false)
        {
            string filePath = null;
            if (img != null)
            {
                // Si l'utilisateur a déjà une photo de profil, on supprime l'ancienne
                var oldPostFilePath = GetPostImagePath(postId);
                if (oldPostFilePath != null) File.Delete($"wwwroot/{oldPostFilePath}");

                byte[] bytesImg = Convert.FromBase64String(img.Value);
                var type = (img.FileType == "image/jpeg" ? "jpg" : "png"); // Si ce n'est pas un jpg c'est un png
                filePath = $"{defaultImagePath}{Guid.NewGuid()}.{type}";
                var totalPath = $"wwwroot/{filePath}";

                // Si c'est un test, on n'écrit pas le fichier
                if (!isTest) await File.WriteAllBytesAsync(totalPath, bytesImg);
            }

            return filePath;
        }

        /// <summary>
        /// Permet de supprimer une image liée à un post du disque
        /// </summary>
        /// <param name="postId"></param>
        /// <returns>boolean</returns>
        private bool DeletePostImageFromDisk(int postId)
        {
            var imgPath = GetPostImagePath(postId);
            if (imgPath != null && File.Exists($"wwwroot/{imgPath}"))
            {
                File.Delete($"wwwroot/{imgPath}");

                return true;
            }

            return false;
        }
    }
}
