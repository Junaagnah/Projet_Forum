using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projet_Forum.Data.Interfaces
{
    public interface IImageRepository
    {
        public Task<bool> SaveProfilePicture(string name, string path, string username);

        public string GetImagePathByUserId(string userId);

        public Task<bool> SavePostImage(string name, string path, int postId);

        public string GetPostImagePath(int postId);

        public Task<bool> DeletePostImage(int postId);
    }
}
