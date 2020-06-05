using System;
using Xunit;
using NSubstitute;
using Projet_Forum.Services.Models;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;
using Projet_Forum.Services.Services;
using System.Collections.Generic;
using Projet_Forum.Services.ViewModels;

namespace Projet_Forum.Tests
{
    public class CategoryServiceTest
    {
        // On instancie les subsitutes
        private readonly ICategoryRepository _categoryRepository = Substitute.For<ICategoryRepository>();
        private readonly IPostService _postService = Substitute.For<IPostService>();

        #region CreateCategory()

        [Theory(DisplayName = "CreateCategory with false response -> user not authorized")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async void CreateCategoryWithUserNotAuthorized(int role)
        {
            // Arrange
            var expectedErrorMessage = "UserNotAuthorized";
            CategoryService service = new CategoryService(_categoryRepository, _postService);

            // Act
            var result = await service.CreateCategory(null, role);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact]
        public async void CreateCategoryWithNull()
        {
            // Arrange
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            // Act
            var result = await categoryService.CreateCategory(null, 3);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void CreateCategoryWithNotNull()
        {
            // Arrange
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            // Act
            // Les paramètres rôle et description peuvent être vides (settés par défaut), on ne remplit donc que le nom
            CategoryViewModel category = new CategoryViewModel
            {
                Name = "Catégorie"
            };

            var result = await categoryService.CreateCategory(category, 3);

            // Assert
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async void CreateCategoryWithNullName()
        {
            // Arrange
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            // Act
            // Les paramètres rôle et description peuvent être vides (settés par défaut), on ne remplit donc que le nom
            CategoryViewModel category = new CategoryViewModel
            {
                Name = null
            };

            var result = await categoryService.CreateCategory(category, 3);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void CreateCategoryWithWhiteSpaceName()
        {
            // Arrange
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            // Act
            CategoryViewModel category = new CategoryViewModel
            {
                Name = " "
            };

            var result = await categoryService.CreateCategory(category, 3);

            // Asserte
            Assert.False(result.Succeeded);
        }
        #endregion

        #region DeleteCategory()

        [Theory(DisplayName = "DeleteCategory with false response -> user not authorized")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async void DeleteCategoryWithUserNotAuthorized(int role)
        {
            // Arrange
            var expectedErrorMessage = "UserNotAuthorized";
            CategoryService service = new CategoryService(_categoryRepository, _postService);

            // Act
            var result = await service.DeleteCategory(1, role);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact]
        public async void DeleteCategoryWithWrongId()
        {
            // Arrange
            // On set le retour à false dans le cas où le delete n'a pas fonctionné (pas de catégorie trouvée)
            _categoryRepository.DeleteCategory(1).Returns(false);

            // Act
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            var result = await categoryService.DeleteCategory(1, 3);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void DeleteCategoryWithRightId()
        {
            // Arrange
            // On set le retour à true dans le cas où le delete a fonctionné (catégorie trouvée)
            _categoryRepository.DeleteCategory(1).Returns(true);

            // Act
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            var result = await categoryService.DeleteCategory(1, 3);

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region GetCategories()
        [Fact]
        public void GetCategoriesWithNotNullReturn()
        {
            // Arrange
            // On remplit une liste de catégories factices qui sera renvoyée par le repository
            var toReturn = new List<Category>()
            {
                new Category
                {
                    Id = 1,
                    Name = "Catégorie 1",
                    Description = "Description",
                    Role = 0
                },
                new Category
                {
                    Id = 2,
                    Name = "Catégorie 2",
                    Description = "Description",
                    Role = 0
                }
            };

            _categoryRepository.GetCategories().Returns(toReturn);

            // Act
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            var result = categoryService.GetCategories();

            // Assert
            // On vérifie que le succeeded est bien sur true et si la liste des catégories n'est pas vide
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public void GetCategoriesWithNullReturn()
        {
            // Act
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            var result = categoryService.GetCategories();
            
            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
        }
        #endregion

        #region GetCategoriesAndPosts()
        [Fact]
        public async void GetCategoriesAndPostsWithNullReturn()
        {
            // Act
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            var result = await categoryService.GetCategoriesAndPosts();

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
        }

        [Fact]
        public async void GetCategoriesAndPostsWithNonNullReturn()
        {
            // Arrange
            // On remplit une liste de catégories factices qui sera renvoyée par le repository
            var toReturn = new List<Category>()
            {
                new Category
                {
                    Id = 1,
                    Name = "Catégorie 1",
                    Description = "Description",
                    Role = 0
                },
                new Category
                {
                    Id = 2,
                    Name = "Catégorie 2",
                    Description = "Description",
                    Role = 0
                }
            };

            // On peut se permettre de retourner une liste de posts nulle cat le fonctionnement ne sera pas bloqué
            _categoryRepository.GetCategories().Returns(toReturn);

            // Act
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            var result = await categoryService.GetCategoriesAndPosts();

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
        }
        #endregion

        #region GetCategory()
        [Fact]
        public void GetCategoryWithWrongId()
        {
            // Act
            // Le résultat doit être null, on ne set donc pas la réponse du repository
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            var result = categoryService.GetCategory(10);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
        }

        [Fact]
        public void GetCategoryWithRightId()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Catégorie 1",
                Description = "Description",
                Role = 0
            };

            _categoryRepository.GetCategory(10).Returns(category);

            // Act
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            var result = categoryService.GetCategory(10);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
        }
        #endregion

        #region UpdateCategory()

        [Theory(DisplayName = "UpdateCategory with false response -> user not authorized")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async void UpdateCategoryWithUserNotAuthorized(int role)
        {
            // Arrange
            var expectedErrorMessage = "UserNotAuthorized";
            CategoryService service = new CategoryService(_categoryRepository, _postService);

            // Act
            var result = await service.UpdateCategory(null, role);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact]
        public async void UpdateCategoryWithNull()
        {
            // Act
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            var result = await categoryService.UpdateCategory(null, 3);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void UpdateCategoryWithEmptyName()
        {
            // Arrange
            CategoryViewModel category = new CategoryViewModel
            {
                Name = ""
            };

            // Act
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            var result = await categoryService.UpdateCategory(category, 3);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void UpdateCategoryWithNotNull()
        {
            // Arrange
            CategoryViewModel category = new CategoryViewModel
            {
                Name = "Catégorie"
            };

            _categoryRepository.UpdateCategory(Arg.Any<Category>()).Returns(true);

            // Act
            CategoryService categoryService = new CategoryService(_categoryRepository, _postService);

            var result = await categoryService.UpdateCategory(category, 3);

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region GetCategoriesWithPagination()

        [Theory(DisplayName = "GetCategoriesWithPagination with false response -> user not administrator")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void GetCategoriesWithPaginationWithUserNotAdmin(int role)
        {
            // Arrange
            var expectedErrorMessage = "UserNotAuthorized";
            CategoryService service = new CategoryService(_categoryRepository, _postService);

            // Act
            var result = service.GetCategoriesWithPagination(role);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact(DisplayName = "GetCategoriesWithPagination with true response -> user is admin")]
        public void GetCategoriesWithPaginationWithUserAdmin()
        {
            // Arrange
            CategoryService service = new CategoryService(_categoryRepository, _postService);

            // Act
            var result = service.GetCategoriesWithPagination(3);

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion
    }
}
