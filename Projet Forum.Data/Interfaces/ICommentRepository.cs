using Projet_Forum.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projet_Forum.Data.Interfaces
{
    public interface ICommentRepository
    {
        public List<Comment> GetCommentsByPost(int postId);

        public Task CreateComment(Comment comment);

        public Task<bool> EditComment(Comment comment);

        public Task DeleteComment(Comment comment);

        public Task DeleteCommentsByPost(int postId);

        public Comment GetComment(int commentId);
    }
}
