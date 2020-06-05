using System.Collections.Generic;
using System.Threading.Tasks;
using Projet_Forum.Data.Models;

namespace Projet_Forum.Data.Interfaces
{
    public interface IPostRepository
    {
        public List<Post> GetPosts();

        public List<Post> GetPostsByCategory(int categoryId, int page = 0, bool isIndex = false);

        public int GetPostsCountByCategory(int categoryId);

        public Post GetPost(int id);

        public Post GetPostByCategory(int postId, int catId);

        public Task<int> CreatePost(Post post);

        public Task<bool> UpdatePost(Post post);

        public Task<bool> DeletePost(int id);

        public Task<bool> DeletePostsByCategory(int categoryId);

        public Task<bool> UpdatePostDate(int postId);
    }
}
