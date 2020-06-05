using System;
using Xunit;
using NSubstitute;
using Projet_Forum.Services.Services;
using Projet_Forum.Services.Models;
using Projet_Forum.Data.Interfaces;

namespace Projet_Forum.Tests
{
    public class ImageServiceTest
    {
        public IImageRepository repository = Substitute.For<IImageRepository>();

        #region GetImagePathByUserId()

        [Theory(DisplayName = "GetImagePathByUserId with null response -> null or empty arg")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetImagePathByUserIdWithNullArg(string userId)
        {
            // Arrange
            ImageService service = new ImageService(repository);

            // Act
            var result = service.GetImagePathByUserId(userId);

            // Assert
            Assert.Null(result);
        }

        [Fact(DisplayName = "GetImagePathByUserId with null response -> image not found")]
        public void GetImagePathByUserIdWithUserNotFound()
        {
            // Arrange
            ImageService service = new ImageService(repository);
            var expectedResult = "";

            // Act
            var result = service.GetImagePathByUserId("id");

            // Assert
            Assert.Equal(result, expectedResult);
        }

        [Fact(DisplayName = "GetImagePathByUserId with non null response -> image found")]
        public void GetImagePathByUserIdWithImageNotFound()
        {
            // Arrange
            repository.GetImagePathByUserId(Arg.Any<string>()).ReturnsForAnyArgs("path");
            ImageService service = new ImageService(repository);

            // Act
            var result = service.GetImagePathByUserId("id");

            // Assert
            Assert.NotNull(result);
        }
        #endregion

        #region SaveProfilePicture()

        [Fact(DisplayName = "SaveProfilePicture with false response -> null args")]
        public async void SaveProfilePictureWhithNullArgs()
        {
            // Arrange
            ImageService service = new ImageService(repository);

            // Act
            var result = await service.SaveProfilePicture(null, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "SaveProfilePicture with false response -> null image")]
        public async void SaveProfilePictureWhithNullImage()
        {
            // Arrange
            ImageService service = new ImageService(repository);

            // Act
            var result = await service.SaveProfilePicture(null, "userId");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Theory(DisplayName = "SaveProfilePicture with false response -> null image content")]
        [InlineData(null, null, null)]
        [InlineData("", null, null)]
        [InlineData(null, "", null)]
        [InlineData(null, null, "")]
        [InlineData(" ", null, null)]
        [InlineData(null, " ", null)]
        [InlineData(null, null, " ")]
        [InlineData("", "", "")]
        [InlineData(" ", " ", " ")]
        public async void SaveProfilePictureWithNullImgContent(string fname, string ftype, string val)
        {
            // Arrange
            var img = new ImageModel
            {
                FileName = fname,
                FileType = ftype,
                Value = val
            };
            ImageService service = new ImageService(repository);

            // Act
            var result = await service.SaveProfilePicture(img, "uname");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "SaveProfilePicture with false response -> null user Id")]
        public async void SaveProfilePictureWhithNullUserId()
        {
            // Arrange
            var img = new ImageModel
            {
                FileName = "File.jpg",
                FileType = "image/jpeg",
                Value = "dmFs"
            };
            ImageService service = new ImageService(repository);

            // Act
            var result = await service.SaveProfilePicture(img, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "SaveProfilePicture with false response -> invalid file type")]
        public async void SaveProfilePictureWhithInvalidFileType()
        {
            // Arrange
            var img = new ImageModel
            {
                FileName = "File.jpg",
                FileType = "image/gif", // Seuls les jpeg et les png sont acceptés
                Value = "dmFs"
            };
            var expectedErrorMessage = "InvalidFileType";

            ImageService service = new ImageService(repository);

            // Act
            var result = await service.SaveProfilePicture(img, "userId");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(result.Messages[0], expectedErrorMessage);
        }

        [Fact(DisplayName = "SaveProfilePicture with false response -> save in db failed")]
        public async void SaveProfilePictureWithDbSaveFailed()
        {
            // Arrange
            var img = new ImageModel
            {
                FileName = "File.jpg",
                FileType = "image/png", // Seuls les jpeg et les png sont acceptés
                Value = "dmFs"
            };

            repository.SaveProfilePicture(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(false);
            ImageService service = new ImageService(repository);

            // Act
            var result = await service.SaveProfilePicture(img, "userId", true);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "SaveProfilePicture with true response -> profile picture successfully saved")]
        public async void SaveProfilePictureWithRightArgs()
        {
            // Arrange
            var img = new ImageModel
            {
                FileName = "File.jpg",
                FileType = "image/png", // Seuls les jpeg et les png sont acceptés
                Value = "dmFs"
            };

            repository.SaveProfilePicture(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(true);
            ImageService service = new ImageService(repository);

            // Act
            var result = await service.SaveProfilePicture(img, "userId", true);

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region SavePostImage()

        [Fact(DisplayName = "SavePostImage with false response -> null post image")]
        public async void SavePostImageWithNullImage()
        {
            // Arrange
            ImageService service = new ImageService(repository);

            // Act
            var result = await service.SavePostImage(null, 0, true);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Theory(DisplayName = "SavePostImage with false response -> null image content")]
        [InlineData(null, null, null)]
        [InlineData("", null, null)]
        [InlineData(null, "", null)]
        [InlineData(null, null, "")]
        [InlineData(" ", null, null)]
        [InlineData(null, " ", null)]
        [InlineData(null, null, " ")]
        [InlineData("", "", "")]
        [InlineData(" ", " ", " ")]
        public async void SavePostImageWithNullImgArgs(string fname, string ftype, string val)
        {
            // Arrange
            var img = new ImageModel
            {
                FileName = fname,
                FileType = ftype,
                Value = val
            };
            ImageService service = new ImageService(repository);

            // Act
            var result = await service.SavePostImage(img, 0, true);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "SavePostImage with false response -> invalid file type")]
        public async void SavePostImageWithInvalidFileType()
        {
            // Arrange
            var img = new ImageModel
            {
                FileName = "File.jpg",
                FileType = "image/gif", // Seuls les jpeg et les png sont acceptés
                Value = "dmFs"
            };
            var expectedErrorMessage = "InvalidFileType";

            ImageService service = new ImageService(repository);

            // Act
            var result = await service.SavePostImage(img, 1, true);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(result.Messages[0], expectedErrorMessage);
        }

        [Fact(DisplayName = "SavePostImage with false response -> save in db failed")]
        public async void SavePostImageWithSaveInDbFailed()
        {
            // Arrange
            var img = new ImageModel
            {
                FileName = "File.jpg",
                FileType = "image/png", // Seuls les jpeg et les png sont acceptés
                Value = "dmFs"
            };

            repository.SavePostImage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).ReturnsForAnyArgs(false);
            ImageService service = new ImageService(repository);

            // Act
            var result = await service.SavePostImage(img, 1, true);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "SavePostImage with true response -> image successfully saved")]
        public async void SavePostImageWithTrueResponse()
        {
            // Arrange
            var img = new ImageModel
            {
                FileName = "File.jpg",
                FileType = "image/png", // Seuls les jpeg et les png sont acceptés
                Value = "dmFs"
            };

            repository.SavePostImage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).ReturnsForAnyArgs(true);
            ImageService service = new ImageService(repository);

            // Act
            var result = await service.SavePostImage(img, 1, true);

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region GetPostImage()

        [Fact(DisplayName = "GetPostImage with empty result -> image not found")]
        public void GetPostImageWithNullResult()
        {
            // Arrange
            var expectedResult = "";
            ImageService service = new ImageService(repository);

            // Act
            var result = service.GetPostImagePath(1);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact(DisplayName = "GetPostImage with non null result -> image found")]
        public void GetPostImageWithNonNullResult()
        {
            // Arrange
            repository.GetPostImagePath(Arg.Any<int>()).ReturnsForAnyArgs("path");
            ImageService service = new ImageService(repository);

            // Act
            var result = service.GetPostImagePath(1);

            // Assert
            Assert.NotNull(result);
        }
        #endregion

        #region DeletePostImage()

        [Fact(DisplayName = "DeletePostImage with false response -> entry not deleted in db")]
        public async void DeletePostImageWithEntryNotDeletedInDb()
        {
            // Arrange
            repository.DeletePostImage(Arg.Any<int>()).ReturnsForAnyArgs(false);
            ImageService service = new ImageService(repository);

            // Act
            var result = await service.DeletePostImage(1, true);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "DeletePostImage with true response -> image successfully deleted")]
        public async void DeletePostImageWithTrueResponse()
        {
            // Arrange
            repository.DeletePostImage(Arg.Any<int>()).ReturnsForAnyArgs(true);
            ImageService service = new ImageService(repository);

            // Act
            var result = await service.DeletePostImage(1, true);

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion
    }
}
