using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projet_Forum.Services.Models;

namespace Projet_Forum.Services.Interfaces
{
    public interface IImageService
    {
        public Task<MyResponse> SaveProfilePicture(ImageModel img, string userId, bool isTest = false);

        public Task<MyResponse> SavePostImage(ImageModel img, int postId, bool isTest = false);

        public string GetImagePathByUserId(string userId);

        public string GetPostImagePath(int postId);

        public Task<MyResponse> DeletePostImage(int postId, bool isTest = false);
    }
}
