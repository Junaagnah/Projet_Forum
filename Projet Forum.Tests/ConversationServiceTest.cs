using Xunit;
using NSubstitute;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Services.Services;
using System.Collections.Generic;
using Projet_Forum.Data.Models;
using Projet_Forum.Services.Models;

namespace Projet_Forum.Tests
{
    public class ConversationServiceTest
    {
        private readonly IConversationRepository _repository = Substitute.For<IConversationRepository>();
        private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();

        #region GetConversationsByUsername()

        [Theory(DisplayName = "GetConversationsByUsername with false response -> null or empty arg")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async void GetConversationsByUsernameWithNullOrEmptyArg(string username)
        {
            // Arrange
            ConversationService service = new ConversationService(_repository, _identityService);

            // Act
            var result = await service.GetConversationsByUsername(username);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "GetConversationsByUsername with false response -> user not found")]
        public async void GetConversationsByUsernameWithUserNotFound()
        {
            // Arrange
            ConversationService service = new ConversationService(_repository, _identityService);

            // Act
            var result = await service.GetConversationsByUsername("username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "GetConversationsByUsername with true response -> user found but no conversations")]
        public async void GetConversationsByUsernameWithNoConversations()
        {
            // Arrange
            var expectedConversationsCount = 0;
            _repository.GetConversationsByUserId(Arg.Any<string>()).ReturnsForAnyArgs(new List<Conversation>());
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id" });
            ConversationService service = new ConversationService(_repository, _identityService);

            // Act
            var result = await service.GetConversationsByUsername("username");

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
            Assert.IsType<List<ConversationResponse>>(result.Result);

            var convs = result.Result as List<ConversationResponse>;

            Assert.Equal(convs.Count, expectedConversationsCount);
        }

        [Fact(DisplayName = "GetConversationsByUsername with true response -> conversations found but not the contact")]
        public async void GetConversationsByUsernameWithContactNotFound()
        {
            // Arrange
            var expectedContactUsername = "Utilisateur inexistant";
            _repository.GetConversationsByUserId(Arg.Any<string>()).ReturnsForAnyArgs(new List<Conversation> { new Conversation { Id = 1, FirstUser = "id", SecondUser = "secondId" } });
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id" });
            ConversationService service = new ConversationService(_repository, _identityService);

            // Act
            var result = await service.GetConversationsByUsername("username");

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
            Assert.IsType<List<ConversationResponse>>(result.Result);

            var convs = result.Result as List<ConversationResponse>;

            Assert.Equal(convs[0].ContactUsername, expectedContactUsername);
        }

        [Fact(DisplayName = "GetConversationsByUsername with true response -> everything found")]
        public async void GetConversationsByUsernameWithEverythingRight()
        {
            // Arrange
            var expectedContactUsername = "username";
            _repository.GetConversationsByUserId(Arg.Any<string>()).ReturnsForAnyArgs(new List<Conversation> { new Conversation { Id = 1, FirstUser = "id", SecondUser = "secondId" } });
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id" });
            _identityService.GetUserById(Arg.Any<string>()).ReturnsForAnyArgs(new User { UserName = "username" });
            ConversationService service = new ConversationService(_repository, _identityService);

            // Act
            var result = await service.GetConversationsByUsername("username");

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
            Assert.IsType<List<ConversationResponse>>(result.Result);

            var convs = result.Result as List<ConversationResponse>;

            Assert.Equal(convs[0].ContactUsername, expectedContactUsername);
        }
        #endregion
    }
}
