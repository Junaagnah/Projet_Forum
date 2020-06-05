using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;

namespace Projet_Forum.Data.Repositories
{
    /// <summary>
    /// Classe permettant de gérer les CRUD pour les commentaires
    /// </summary>
    public class CommentRepository : ICommentRepository
    {
        private readonly MyDbContext _context;

        public CommentRepository(MyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Permet de créer un commentaire
        /// </summary>
        /// <param name="comment"></param>
        public async Task CreateComment(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Permet de supprimer un commentaire
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns>bool</returns>
        public async Task DeleteComment(Comment comment)
        {
            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();

        }

        /// <summary>
        /// Permet de modifier un commentaire
        /// </summary>
        /// <param name="comment"></param>
        /// <returns>bool</returns>
        public async Task<bool> EditComment(Comment comment)
        {
            var oldComment = _context.Comments.FirstOrDefault(com => com.Id == comment.Id);
            if (oldComment != null)
            {
                oldComment.Body = comment.Body;
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Permet de récupérer un commentaire
        /// </summary>
        /// <param name="commentId"></param>
        /// <returns>Comment</returns>
        public Comment GetComment(int commentId)
        {
            var comment = _context.Comments.FirstOrDefault(com => com.Id == commentId);

            return comment;
        }

        /// <summary>
        /// Permet de récupérer les commentaires par post, organisés par date (le plus récent en dernier)
        /// </summary>
        /// <param name="postId"></param>
        /// <returns>Liste de commentaires</returns>
        public List<Comment> GetCommentsByPost(int postId)
        {
            // @TODO: gérer la pagination
            List<Comment> comments = _context.Comments.Where(com => com.Post == postId).OrderBy(com => com.Date).ToList();

            return comments;
        }

        /// <summary>
        /// Permet de supprimer tous les commentaires d'un post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns>void</returns>
        public async Task DeleteCommentsByPost(int postId)
        {
            List<Comment> comments = _context.Comments.Where(com => com.Post == postId).ToList();

            foreach(var comment in comments)
            {
                _context.Comments.Remove(comment);
            }

            await _context.SaveChangesAsync();
        }
    }
}
