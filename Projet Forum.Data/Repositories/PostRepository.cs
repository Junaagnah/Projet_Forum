using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;

namespace Projet_Forum.Data.Repositories
{
    /// <summary>
    /// Repository permettant de gérer les posts
    /// </summary>
    public class PostRepository : IPostRepository
    {
        private readonly MyDbContext _context;

        public PostRepository(MyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Permet de créer un post
        /// </summary>
        /// <param name="post"></param>
        /// <returns>Integer</returns>
        public async Task<int> CreatePost(Post post)
        {
            // On set la date d'enregistrement du post côté serveur pour plus de fiabilité
            post.Date = DateTime.Now;
            post.LastUpdated = DateTime.Now;
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post.Id;
        }

        /// <summary>
        /// Permet de supprimer un post
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> DeletePost(int id)
        {
            var post = _context.Posts.ToList().FirstOrDefault(p => p.Id == id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Permet de récupérer un post via son id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Post</returns>
        public Post GetPost(int id)
        {
            return _context.Posts.FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Permet de récupérer un post via son id et sa catégorie
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="catId"></param>
        /// <returns>Post</returns>
        public Post GetPostByCategory(int postId, int catId)
        {
            return _context.Posts.FirstOrDefault(p => p.Category == catId && p.Id == postId);
        }

        /// <summary>
        /// Permet de récupérer tous les posts
        /// </summary>
        /// <returns></returns>
        public List<Post> GetPosts()
        {
            return _context.Posts.OrderByDescending(p => p.LastUpdated).ToList();
        }

        /// <summary>
        /// Permet de récupérer les posts d'une catégorie
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="page"></param>
        /// <param name="isIndex"></param>
        /// <returns>List<Post></returns>
        public List<Post> GetPostsByCategory(int categoryId, int page = 0, bool isIndex = false)
        {
            // On limite le nombre de posts retournés si l'appel vient de l'index
            List<Post> result = null;

            if (isIndex) result = _context.Posts.Where(p => p.Category == categoryId).OrderByDescending(p => p.LastUpdated).Take(10).ToList();
            else
            {
                // Système de pagination
                var postsPage = page * 10;
                result = _context.Posts.Where(p => p.Category == categoryId).OrderByDescending(p => p.LastUpdated).Skip(postsPage).Take(10).ToList();
            }

            return result;
        }

        /// <summary>
        /// Permet de mettre à jour un post
        /// </summary>
        /// <param name="post"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> UpdatePost(Post post)
        {
            var oldPost = _context.Posts.FirstOrDefault(p => p.Id == post.Id);
            if (oldPost != null)
            {
                oldPost.Title = post.Title;
                oldPost.Body = post.Body;
                oldPost.Category = post.Category;
                oldPost.Locked = post.Locked;

                // On met à jour la valeur LastUpdated pour faire remonter le post tout en faut
                oldPost.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Permet de supprimer tous les posts d'une catégorie (si on supprime une catégorie
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> DeletePostsByCategory(int categoryId)
        {
            var posts = this.GetPostsByCategory(categoryId);

            if (posts != null)
            {
                posts.ForEach(p => {
                    _context.Posts.Remove(p);
                });

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Permet de mettre à jour la date d'un post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> UpdatePostDate(int postId)
        {
            var result = false;

            var post = _context.Posts.FirstOrDefault(post => post.Id == postId);
            if (post != null)
            {
                post.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Permet de récupérer le nombre de posts dans une catégorie
        /// </summary>
        /// <param name="catId"></param>
        /// <returns>Integer</returns>
        public int GetPostsCountByCategory(int catId)
        {
            int count = _context.Posts.Where(p => p.Category == catId).ToList().Count();

            return count;
        }
    }
}
