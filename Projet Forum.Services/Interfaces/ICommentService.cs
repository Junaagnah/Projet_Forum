using Projet_Forum.Data.Models;
using Projet_Forum.Services.Models;
using Projet_Forum.Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projet_Forum.Services.Interfaces
{
    public interface ICommentService
    {
        public Task<List<CommentResponse>> GetCommentsByPost(int postId);

        public Task<MyResponse> CreateComment(CommentViewModel comment, string authorUsername);

        public Task<MyResponse> DeleteComment(int commentId, string username);

        public Task DeleteCommentsByPost(int postId);

        public Task<MyResponse> GetComment(int commentId);

        public Task<MyResponse> EditComment(CommentViewModel comment, string username);
    }
}
