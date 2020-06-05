using Xunit;
using NSubstitute;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Services.Services;
using Projet_Forum.Data.Models;

namespace Projet_Forum.Tests
{
    public class NotificationServiceTest
    {
        private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
        private readonly INotificationRepository _repository = Substitute.For<INotificationRepository>();

        #region CreateNotification()

        [Fact(DisplayName = "CreateNotification with false result -> null arg")]
        public async void CreateNotificationWithNullArg()
        {
            // Arrange
            NotificationService service = new NotificationService(_repository, _identityService);

            // Act
            var result = await service.CreateNotification(null);

            // Assert
            Assert.False(result);
        }

        [Theory(DisplayName = "CreateNotification with false result -> null userId or content")]
        [InlineData(null, null)]
        [InlineData("id", null)]
        [InlineData(null, "content")]
        public async void CreateNotificationWithNullUserIdOrContent(string id, string content)
        {
            // Arrange
            var notif = new Notification
            {
                UserId = id,
                Content = content,
                Context = Notification.Type.Post
            };
            NotificationService service = new NotificationService(_repository, _identityService);

            // Act
            var result = await service.CreateNotification(notif);

            // Assert
            Assert.False(result);
        }

        [Fact(DisplayName = "CreateNotification with true result -> notification successfully created")]
        public async void CreateNotificationWithTrueResult()
        {
            // Arrange
            var notif = new Notification
            {
                UserId = "id",
                Content = "content",
                Context = Notification.Type.Post,
                ContextId = 1,
                CategoryId = 1
            };
            NotificationService service = new NotificationService(_repository, _identityService);

            // Act
            var result = await service.CreateNotification(notif);

            // Assert
            Assert.True(result);
        }
        #endregion

        #region DeleteNotification()

        [Fact(DisplayName = "DeleteNotification with false response -> notification not found")]
        public async void DeleteNotificationWithNotificationNotFound()
        {
            // Arrange
            _repository.DeleteNotification(Arg.Any<int>()).ReturnsForAnyArgs(false);
            NotificationService service = new NotificationService(_repository, _identityService);

            // Act
            var response = await service.DeleteNotification(1);

            // Assert
            Assert.False(response.Succeeded);
        }

        [Fact(DisplayName = "DeleteNotification with true response -> notification successfully deleted")]
        public async void DeleteNotificationWithNotificationFound()
        {
            // Arrange
            _repository.DeleteNotification(Arg.Any<int>()).ReturnsForAnyArgs(true);
            NotificationService service = new NotificationService(_repository, _identityService);

            // Act
            var response = await service.DeleteNotification(1);

            // Assert
            Assert.True(response.Succeeded);
        }
        #endregion

        #region GetNotificationsByUsername()

        [Theory(DisplayName = "GetNotificationsByUsername with false response -> null or empty arg")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async void GetNotificationsByUsernameWithEmptyArg(string username)
        {
            // Arrange
            NotificationService service = new NotificationService(_repository, _identityService);

            // Act
            var result = await service.GetNotificationsByUsername(username);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "GetNotificationsByUsername with false response -> user not found")]
        public async void GetNotificationsByUsernameWithUserNotFound()
        {
            // Arrange
            NotificationService service = new NotificationService(_repository, _identityService);

            // Act
            var result = await service.GetNotificationsByUsername("username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "GetNotificationsByUsername with true response -> user found")]
        public async void GetNotificationsByUsernameWithUserFound()
        {
            // Arrange
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id" });
            NotificationService service = new NotificationService(_repository, _identityService);

            // Act
            var result = await service.GetNotificationsByUsername("username");

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region MarkAsReadByUsername()

        [Theory(DisplayName = "MarkAsReadByUsername with false response -> null or empty arg")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async void MarkAsReadByUsernameWithNullArg(string username)
        {
            // Arrange
            NotificationService service = new NotificationService(_repository, _identityService);

            // Act
            var result = await service.MarkAsReadByUsername(username);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "MarkAsReadByUsername with false response -> user not found")]
        public async void MarkAsReadByUsernameWithUserNotFound()
        {
            // Arrange
            NotificationService service = new NotificationService(_repository, _identityService);

            // Act
            var result = await service.MarkAsReadByUsername("username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "MarkAsReadByUsername with true response -> user found")]
        public async void MarkAsReadByUsernameWithUserFound()
        {
            // Arrange
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id" });
            NotificationService service = new NotificationService(_repository, _identityService);

            // Act
            var result = await service.MarkAsReadByUsername("username");

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion
    }
}
