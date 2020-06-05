using System;
using Xunit;
using NSubstitute;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Services.Services;
using Projet_Forum.Data.Models;
using System.Collections.Generic;
using Projet_Forum.Services.Models;
using Projet_Forum.Services.ViewModels;

namespace Projet_Forum.Tests
{
    public class CommentServiceTest
    {
        private readonly ICommentRepository _repository = Substitute.For<ICommentRepository>();
        private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
        private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
        private readonly INotificationService _notificationService = Substitute.For<INotificationService>();

        #region GetCommentsByPost()

        [Fact(DisplayName = "GetCommentsByPost with null response -> no comments found")]
        public async void GetCommentsByPostWithPostNotFound()
        {
            // Arrange
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var response = await service.GetCommentsByPost(0);

            // Assert
            Assert.Empty(response);
        }

        [Fact(DisplayName = "GetCommentsByPost with non null response -> comments found")]
        public async void GetCommentsByPostWithNonNullResponse()
        {
            // Arrange
            List<Comment> commentsToReturn = new List<Comment>
            {
                new Comment
                {
                    Id = 1,
                    Body = "body",
                    Author = "author",
                    Date = DateTime.Now,
                    Post = 1
                },
                new Comment
                {
                    Id = 2,
                    Body = "body",
                    Author = "author",
                    Date = DateTime.Now,
                    Post = 1
                }
            };
            _repository.GetCommentsByPost(Arg.Any<int>()).ReturnsForAnyArgs(commentsToReturn);
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var result = await service.GetCommentsByPost(1);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<CommentResponse>>(result);
        }
        #endregion

        #region CreateComment()

       [Fact(DisplayName = "Create comment with false response -> null args")]
       public async void CreateCommentWithNullArgs()
        {
            // Arrange
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var result = await service.CreateComment(null, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "CreateComment with false response -> post not found")]
        public async void CreateCommendWithPostNotFound()
        {
            // Arrange
            var comment = new CommentViewModel
            {
                Author = "id",
                Body = "body",
                Post = 1
            };
            var expectedErrorMessage = "PostNotFound";
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var response = await service.CreateComment(comment, "username");

            // Assert
            Assert.False(response.Succeeded);
            Assert.Equal(response.Messages[0], expectedErrorMessage);
        }

        [Fact(DisplayName = "CreateComment with false response -> post locked")]
        public async void CreateCommentWithPostLocked()
        {
            // Arrange
            var comment = new CommentViewModel
            {
                Author = "id",
                Body = "body",
                Post = 1
            };
            var post = new Post
            {
                Author = "id",
                Id = 1,
                Body = "body",
                Locked = true
            };
            var expectedErrorMessage = "PostLocked";
            _postRepository.GetPost(Arg.Any<int>()).ReturnsForAnyArgs(post);
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var response = await service.CreateComment(comment, "username");

            // Assert
            Assert.False(response.Succeeded);
            Assert.Equal(response.Messages[0], expectedErrorMessage);
        }

        [Fact(DisplayName = "CreateComment with false response -> user not found")]
        public async void CreateCommentWithUserNotFound()
        {
            // Arrange
            var comment = new CommentViewModel
            {
                Author = "id",
                Body = "body",
                Post = 1
            };
            var post = new Post
            {
                Author = "id",
                Id = 1,
                Body = "body",
                Locked = false
            };
            _postRepository.GetPost(Arg.Any<int>()).ReturnsForAnyArgs(post);
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var response = await service.CreateComment(comment, "username");

            // Assert
            Assert.False(response.Succeeded);
        }

        [Fact(DisplayName = "CreateComment with true response -> everything was right")]
        public async void CreateCommentWithTrueResponse()
        {
            // Arrange
            var comment = new CommentViewModel
            {
                Author = "id",
                Body = "body",
                Post = 1
            };
            var post = new Post
            {
                Author = "id",
                Id = 1,
                Body = "body",
                Locked = false
            };
            var user = new User
            {
                Id = "id=",
                UserName = "username"
            };
            _postRepository.GetPost(Arg.Any<int>()).ReturnsForAnyArgs(post);
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(user);
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var response = await service.CreateComment(comment, "username");

            // Assert
            Assert.True(response.Succeeded);
        }
        #endregion

        #region DeleteComment()

        [Fact(DisplayName = "DeleteComment with false response -> null username")]
        public async void DeleteCommentWithNullUsername()
        {
            // Arrange
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var result = await service.DeleteComment(1, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "DeleteComment with false response -> user not found")]
        public async void DeleteCommentWithUserNotFound()
        {
            // Arrange
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var result = await service.DeleteComment(1, "username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "DeleteComment with false response -> comment not found")]
        public async void DeleteCommentWithCommentNotFound()
        {
            // Arrange
            var user = new User
            {
                Id = "id",
                UserName = "username",
                Role = 3
            };

            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(user);

            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var result = await service.DeleteComment(1, "username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "DeleteComment with true response -> comment deleted")]
        public async void DeleteCommentWithCommentDeleted()
        {
            // Arrange
            var user = new User
            {
                Id = "id",
                UserName = "username",
                Role = 3
            };
            var comment = new Comment
            {
                Id = 1,
                Body = "body",
                Author = "id"
            };

            _repository.GetComment(Arg.Any<int>()).ReturnsForAnyArgs(comment);
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(user);

            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var result = await service.DeleteComment(1, "username");

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region GetComment()

        [Fact(DisplayName = "GetComment with false response -> comment not found")]
        public async void GetCommentWithCommentNotFound()
        {
            // Arrange
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var result = await service.GetComment(1);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "GetComment with true response -> comment found")]
        public async void GetCommentWithTrueResponse()
        {
            // Arrange
            var comment = new Comment
            {
                Author = "id",
                Body = "body",
                Id = 1,
                Post = 1
            };

            _repository.GetComment(Arg.Any<int>()).ReturnsForAnyArgs(comment);
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var response = await service.GetComment(1);

            // Assert
            Assert.True(response.Succeeded);
            Assert.NotNull(response.Result);
        }
        #endregion

        #region EditComment()

        [Fact(DisplayName = "EditComment with false response -> null args")]
        public async void EditCommentWithNullArgs()
        {
            // Arrange
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var response = await service.EditComment(null, null);

            // Assert
            Assert.False(response.Succeeded);
        }

        [Fact(DisplayName = "EditComment with false response -> user not found")]
        public async void EditCommentWithUserNotFound()
        {
            // Arrange
            var comment = new CommentViewModel
            {
                Id = 1,
                Body = "body",
                Post = 1,
                Author = "id"
            };

            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var response = await service.EditComment(comment, "username");

            // Assert
            Assert.False(response.Succeeded);
        }

        [Fact(DisplayName = "EditComment with false response -> comment not found")]
        public async void EditCommentWithCommentNotFound()
        {
            // Arrange
            var comment = new CommentViewModel
            {
                Id = 1,
                Body = "body",
                Post = 1,
                Author = "id"
            };
            var user = new User
            {
                Id = "id",
                UserName = "username",
                Role = 3
            };

            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(user);
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var response = await service.EditComment(comment, "username");

            // Assert
            Assert.False(response.Succeeded);
        }

        [Fact(DisplayName = "EditComment with false response -> post not found")]
        public async void EditCommentWithPostNotFound()
        {
            // Arrange
            var comment = new CommentViewModel
            {
                Id = 1,
                Body = "body",
                Post = 1,
                Author = "id"
            };
            var user = new User
            {
                Id = "id",
                UserName = "username",
                Role = 3
            };
            var gotComment = new Comment
            {
                Id = 1,
                Author = "id",
                Post = 1,
                Body = "body"
            };
            var expectedErrorMessage = "PostNotFound";

            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(user);
            _repository.GetComment(Arg.Any<int>()).ReturnsForAnyArgs(gotComment);
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var response = await service.EditComment(comment, "username");

            // Assert
            Assert.False(response.Succeeded);
            Assert.Equal(response.Messages[0], expectedErrorMessage);
        }

        [Fact(DisplayName = "EditComment with true response -> comment updated")]
        public async void EditCommentWithTrueResponse()
        {
            // Arrange
            var comment = new CommentViewModel
            {
                Id = 1,
                Body = "body",
                Post = 1,
                Author = "id"
            };
            var user = new User
            {
                Id = "id",
                UserName = "username",
                Role = 3
            };
            var gotComment = new Comment
            {
                Id = 1,
                Author = "id",
                Post = 1,
                Body = "body"
            };
            var post = new Post
            {
                Id = 1,
                Author = "id"
            };

            _postRepository.GetPost(Arg.Any<int>()).ReturnsForAnyArgs(post);
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(user);
            _repository.GetComment(Arg.Any<int>()).ReturnsForAnyArgs(gotComment);
            _repository.EditComment(Arg.Any<Comment>()).ReturnsForAnyArgs(true);
            CommentService service = new CommentService(_repository, _identityService, _postRepository, _notificationService);

            // Act
            var response = await service.EditComment(comment, "username");

            // Assert
            Assert.True(response.Succeeded);
        }
        #endregion
    }
}
