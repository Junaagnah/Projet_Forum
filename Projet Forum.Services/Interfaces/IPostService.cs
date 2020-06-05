using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projet_Forum.Services.Models;
using Projet_Forum.Services.ViewModels;
using Projet_Forum.Data.Models;

namespace Projet_Forum.Services.Interfaces
{
    public interface IPostService
    {
        public Task<MyResponse> GetPosts();

        public Task<List<Post>> GetPostsByCategory(int categoryId, int page = 0, bool isIndex = false);

        public Task<MyResponse> GetPost(int id);

        public Task<MyResponse> GetPostByCategory(int postId, int catId);

        public Task<MyResponse> CreatePost(PostViewModel post, string authorUsername);

        public Task<MyResponse> UpdatePost(PostViewModel post, string username);

        public Task<MyResponse> DeletePost(int id, string username);

        public Task<bool> DeletePostsByCategory(int categoryId);

        public Task<MyResponse> DeletePostImage(int postId, string username);

        public Task<MyResponse> GetPostsWithPagination(int catId, int page, int role);
    }
}
