using System;
using Xunit;
using NSubstitute;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Services.Services;
using Projet_Forum.Services.ViewModels;
using Projet_Forum.Services.Models;
using System.Collections.Generic;

/**
 * Tester, c'est douter - Un sage
 */
namespace Projet_Forum.Tests
{
    public class PostServiceTest
    {
        private readonly IPostRepository _postRepository = Substitute.For<IPostRepository>();
        private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
        private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
        private readonly ICommentService _commentService = Substitute.For<ICommentService>();
        private readonly IImageService _imageService = Substitute.For<IImageService>();

        #region CreatePost()
        [Fact]
        public async void CreatePostWithNullPost()
        {
            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.CreatePost(null, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void CreatePostWithNullTitle()
        {
            // Arrange
            PostViewModel post = new PostViewModel
            {
                Title = null,
                Body = "Body"
            };

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.CreatePost(post, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void CreatePostWithNullBody()
        {
            // Arrange
            PostViewModel post = new PostViewModel
            {
                Title = "Title",
                Body = null
            };

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.CreatePost(post, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void CreatePostWithNullUsername()
        {
            // Arrange
            PostViewModel post = new PostViewModel
            {
                Title = "Title",
                Body = null
            };

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.CreatePost(post, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void CreatePostWithWrongUsername()
        {
            // Arrange
            PostViewModel post = new PostViewModel
            {
                Title = "Title",
                Body = null
            };

            // Act
            // On n'a pas besoin de setter les retours du postRepository et de l'identityService puisque l'identityService renverra null et le postService ne sera pas appelé
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.CreatePost(post, "username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void CreatePostWithCategoryNotFound()
        {
            // Arrange
            PostViewModel post = new PostViewModel
            {
                Title = "Title",
                Body = "Body",
                Category = 0
            };
            string username = "Username";

            // On renvoie juste un utilisateur avec un id car c'est la seule valeur nécessaire
            _identityService.GetUserByUsername(username).Returns(new User { Id = "id" });

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.CreatePost(post, username);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "CreateCategory with true response -> right args")]
        public async void CreatePostWithrightArgs()
        {
            // Arrange
            PostViewModel post = new PostViewModel
            {
                Title = "Title",
                Body = "Body",
                Category = 0
            };
            string username = "Username";

            // On renvoie juste un utilisateur avec un id car c'est la seule valeur nécessaire
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id" });
            _categoryRepository.GetCategory(Arg.Any<int>()).ReturnsForAnyArgs(new Category());
            _postRepository.CreatePost(Arg.Any<Post>()).ReturnsForAnyArgs(1);

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.CreatePost(post, username);

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region DeletePost()
        [Fact]
        public async void DeletePostWithWrongId()
        {
            // Arrange
            _postRepository.DeletePost(10).Returns(false);
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { UserName = "username", Id = "id", Role = 3 });

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.DeletePost(10, "username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void DeletePostWithRightId()
        {
            // Arrange
            _postRepository.DeletePost(10).Returns(true);
            _postRepository.GetPost(Arg.Any<int>()).ReturnsForAnyArgs(new Post { Id = 1, Author = "id", Body = "body", Category = 1, Title = "title" });
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { UserName = "username", Id = "id", Role = 3 });

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.DeletePost(10, "username");
            
            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region GetPost()
        [Fact]
        public async void GetPostWithWrongId()
        {
            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.GetPost(10);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
        }

        [Fact]
        public async void GetPostWithGoodIdButAuthorNotFound()
        {
            // Arrange
            Post post = new Post
            {
                Title = "Title",
                Body = "Body",
                Author = "1"
            };

            _postRepository.GetPost(10).Returns(post);

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.GetPost(10);

            // Assert
            // Même si on ne trouve pas d'auteur, on assigne quand même la vauleur 'Utilisateur inexistant' à l'AuthorUsername
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
            Assert.IsType<Post>(result.Result);

            var returned = result.Result as Post;
            var expectedUsername = "Utilisateur inexistant";

            Assert.Equal(returned.AuthorUsername, expectedUsername);
        }

        [Fact]
        public async void GetPostWithGoodId()
        {
            // Arrange
            // On set les valeurs de retour
            Post post = new Post
            {
                Title = "Title",
                Body = "Body",
                Author = "1"
            };

            User user = new User
            {
                UserName = "UserName"
            };

            int postId = 10;

            _postRepository.GetPost(postId).Returns(post);
            _identityService.GetUserById(post.Author).Returns(user);

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.GetPost(postId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
            Assert.IsType<Post>(result.Result);

            var gotPost = result.Result as Post;
            var nonExpectedUsername = "Utilisateur inconnu";

            Assert.NotEqual(gotPost.AuthorUsername, nonExpectedUsername);
        }

        [Fact]
        public async void GetPostsWithNullReturn()
        {
            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.GetPosts();

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
        }
        #endregion

        #region GetPostByCategory()

        [Fact(DisplayName = "GetPostByCategory with false response -> category or post not found")]
        public async void GetPostByCategoryWithPostOrCategoryNotFound()
        {
            // Arrange
            var expectedErrorMessage = "PostNotFound";
            PostService service = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            // Act
            var result = await service.GetPostByCategory(0, 0);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
            Assert.Equal(result.Messages[0], expectedErrorMessage);
        }

        [Fact(DisplayName = "GetPostByCategory with true response -> post found but not the author")]
        public async void GetPostByCategoryWithUserNotFound()
        {
            // Arrange
            var postToReturn = new Post
            {
                Id = 1,
                Title = "Title",
                Body = "Body",
                Author = "authorId"
            };
            var expectedAuthorUsername = "Utilisateur inexistant";
            _postRepository.GetPostByCategory(Arg.Any<int>(), Arg.Any<int>()).ReturnsForAnyArgs(postToReturn);
            PostService service = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            // Act
            var result = await service.GetPostByCategory(0, 0);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
            Assert.IsType<PostResponse>(result.Result);

            var response = result.Result as PostResponse;
            Assert.Equal(response.Id, postToReturn.Id);
            Assert.Equal(response.Title, postToReturn.Title);
            Assert.Equal(response.Body, postToReturn.Body);
            Assert.NotNull(response.AuthorProfile);
            Assert.IsType<ProfileResponse>(response.AuthorProfile);
            Assert.Equal(response.AuthorProfile.Username, expectedAuthorUsername);
        }

        [Fact(DisplayName = "GetPostByCategory with true response -> post and author found")]
        public async void GetPostByCategoryWithPostAndAuthorFound()
        {
            // Arrange
            var postToReturn = new Post
            {
                Id = 1,
                Title = "Title",
                Body = "Body",
                Author = "authorId"
            };
            var profileToReturn = new ProfileResponse
            {
                Username = "username",
                Description = "description",
                Role = 1,
                RoleName = "Utilisateur",
                ProfilePicture = "images/1zad4azd6541adz6a.jpg"
            };
            _postRepository.GetPostByCategory(Arg.Any<int>(), Arg.Any<int>()).ReturnsForAnyArgs(postToReturn);
            _identityService.GetUserById(Arg.Any<string>()).ReturnsForAnyArgs(new User { UserName = "username" });
            _identityService.GetUserProfileByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new MyResponse { Succeeded = true, Result = profileToReturn });
            PostService service = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            // Act
            var result = await service.GetPostByCategory(0, 0);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
            Assert.IsType<PostResponse>(result.Result);

            var response = result.Result as PostResponse;
            Assert.Equal(response.Id, postToReturn.Id);
            Assert.Equal(response.Title, postToReturn.Title);
            Assert.Equal(response.Body, postToReturn.Body);
            Assert.NotNull(response.AuthorProfile);
            Assert.IsType<ProfileResponse>(response.AuthorProfile);
            Assert.Equal(response.AuthorProfile.Username, profileToReturn.Username);
            Assert.Equal(response.AuthorProfile.Description, profileToReturn.Description);
            Assert.Equal(response.AuthorProfile.Role, profileToReturn.Role);
            Assert.Equal(response.AuthorProfile.RoleName, profileToReturn.RoleName);
            Assert.Equal(response.AuthorProfile.ProfilePicture, profileToReturn.ProfilePicture);
        }
        #endregion

        #region GetPosts()
        [Fact]
        public async void GetPostsWithAuthorNotFound()
        {
            // Arrange
            var posts = new List<Post>()
            {
                new Post
                {
                    Title = "Title",
                    Body = "Body",
                    Author = "1"
                },
                new Post
                {
                    Title = "Title",
                    Body = "Body",
                    Author = "1"
                },
                new Post
                {
                    Title = "Title",
                    Body = "Body",
                    Author = "1"
                }
            };

            _postRepository.GetPosts().Returns(posts);

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.GetPosts();
            
            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
            Assert.IsType<List<Post>>(result.Result);

            var gotPosts = result.Result as List<Post>;
            var expectedUsername = "Utilisateur inexistant";

            // On teste si le service a bien inséré 'Utilisateur inexistant' dans tous les posts
            gotPosts.ForEach(post =>
            {
                Assert.Equal(post.AuthorUsername, expectedUsername);
            });
        }

        [Fact]
        public async void GetPostsWithNotNullAndUserFound()
        {
            // Arrange
            var posts = new List<Post>()
            {
                new Post
                {
                    Title = "Title",
                    Body = "Body",
                    Author = "1"
                },
                new Post
                {
                    Title = "Title",
                    Body = "Body",
                    Author = "1"
                },
                new Post
                {
                    Title = "Title",
                    Body = "Body",
                    Author = "1"
                }
            };

            User user = new User
            {
                UserName = "UserName"
            };

            _identityService.GetUserById("1").Returns(user);
            _postRepository.GetPosts().Returns(posts);

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.GetPosts();

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
            Assert.IsType<List<Post>>(result.Result);

            var gotPosts = result.Result as List<Post>;
            var nonExpectedUsername = "Utilisateur inexistant";

            // On vérifie bien que le service n'a pas assigné utilisateur inconnue à la propriété AuthorUsername
            gotPosts.ForEach(post =>
            {
                Assert.NotEqual(post.AuthorUsername, nonExpectedUsername);
            });
        }
        #endregion

        #region GetPostsByCategory()
        [Fact]
        public async void GetPostsByCategoryWithWrondCategoryId()
        {
            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.GetPostsByCategory(20);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async void GetPostsByCategoryWithRightCategoryId()
        {
            // Arrange
            var posts = new List<Post>()
            {
                new Post
                {
                    Title = "Title",
                    Body = "Body",
                    Author = "1"
                },
                new Post
                {
                    Title = "Title",
                    Body = "Body",
                    Author = "1"
                },
                new Post
                {
                    Title = "Title",
                    Body = "Body",
                    Author = "1"
                }
            };

            _postRepository.GetPostsByCategory(20).Returns(posts);

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.GetPostsByCategory(20);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<Post>>(result);
        }
        #endregion

        #region UpdatePost()
        [Fact]
        public async void UpdatePostWithNull()
        {
            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.UpdatePost(null, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void UpdatePostWithNullTitle()
        {
            // Arrange
            var post = new PostViewModel
            {
                Title = null,
                Body = "Body"
            };

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.UpdatePost(post, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void UpdatePostWithNullBody()
        {
            // Arrange
            var post = new PostViewModel
            {
                Title = "Title",
                Body = null
            };

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.UpdatePost(post, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void UpdatePostWithCategoryNotFound()
        {
            // Arrange
            var post = new PostViewModel
            {
                Title = "Title",
                Body = "Body"
            };

            _postRepository.UpdatePost(Arg.Any<Post>()).ReturnsForAnyArgs(true);
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { UserName = "username", Id = "id", Role = 3 });

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.UpdatePost(post, "username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "UpdateCategory with true response -> right args")]
        public async void UpdateCategoryWithRightArgs()
        {
            // Arrange
            var post = new PostViewModel
            {
                Title = "Title",
                Body = "Body"
            };

            _postRepository.UpdatePost(Arg.Any<Post>()).ReturnsForAnyArgs(true);
            _postRepository.GetPost(Arg.Any<int>()).ReturnsForAnyArgs(new Post { Author = "id", Id = 1, Body = "body", Category = 1, Title = "title" });
            _categoryRepository.GetCategory(Arg.Any<int>()).ReturnsForAnyArgs(new Category());
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { UserName = "username", Id = "id", Role = 3 });

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.UpdatePost(post, "username");

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region DeltePostsByCategory()
        [Fact]
        public async void DeletePostsByCategoryWithWrongId()
        {
            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.DeletePostsByCategory(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async void DeletePostsByCategoryWithRightId()
        {
            // Arrange
            _postRepository.DeletePostsByCategory(1).Returns(true);
            _postRepository.GetPost(Arg.Any<int>()).ReturnsForAnyArgs(new Post { Id = 1, Author = "id", Body = "body", Category = 1, Title = "title" });
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { UserName = "username", Id = "id", Role = 3 });

            // Act
            PostService postService = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            var result = await postService.DeletePostsByCategory(1);

            // Assert
            Assert.True(result);
        }
        #endregion

        #region DeletePostImage()

        [Theory(DisplayName = "DeletePostImage with false response -> null username")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async void DeletePostImageWithNullUsername(string username)
        {
            // Arrange
            PostService service = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            // Act
            var result = await service.DeletePostImage(1, username);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "DeletePostImage with false response -> post not found")]
        public async void DeletePostImageWithPostNotFound()
        {
            // Arrange
            var expectedErrorMessage = "PostNotFound";
            PostService service = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            // Act
            var result = await service.DeletePostImage(1, "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact(DisplayName = "DeletePostImage with false response -> user not authorized")]
        public async void DeletePostImageWithUserNotAuthorized()
        {
            // Arrange
            var expectedErrorMessage = "UserNotAuthorized";
            var post = new Post
            {
                Author = "notSame"
            };

            _postRepository.GetPost(Arg.Any<int>()).ReturnsForAnyArgs(post);
            PostService service = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            // Act
            var result = await service.DeletePostImage(1, "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact(DisplayName = "DeletePostImage with true response -> post image deleted")]
        public async void DeletePostImageWithImageDeleted()
        {
            // Arrange
            var post = new Post
            {
                Author = "id"
            };

            var user = new User
            {
                Id = "id"
            };

            _postRepository.GetPost(Arg.Any<int>()).ReturnsForAnyArgs(post);
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(user);
            _imageService.DeletePostImage(Arg.Any<int>()).ReturnsForAnyArgs(new MyResponse { Succeeded = true });
            PostService service = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            // Act
            var result = await service.DeletePostImage(1, "username");

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region GetPostsWithPagination()

        [Fact(DisplayName = "GetPostsWithPagination with false response -> category not found")]
        public async void GetPostsWithPaginationWithCategoryNotFound()
        {
            // Arrange
            PostService service = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            // Act
            var result = await service.GetPostsWithPagination(1, 0, 0);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "GetPostsWithPagination with true response -> category found")]
        public async void GetPostsWithPaginationWithCategoryFound()
        {
            // Arrange
            _categoryRepository.GetCategory(Arg.Any<int>()).ReturnsForAnyArgs(new Category { Id = 1 });
            PostService service = new PostService(_postRepository, _identityService, _categoryRepository, _commentService, _imageService);

            // Act
            var result = await service.GetPostsWithPagination(1, 0, 0);

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion
    }
}
