using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using NSubstitute;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Services.Services;
using Projet_Forum.Services.ViewModels;
using Projet_Forum.Data.Models;
using Projet_Forum.Services.Models;

namespace Projet_Forum.Tests
{
    public class MessageServiceTest
    {
        private readonly IMessageRepository _repository = Substitute.For<IMessageRepository>();
        private readonly IConversationRepository _convRepository = Substitute.For<IConversationRepository>();
        private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
        private readonly INotificationService _notifService = Substitute.For<INotificationService>();

        #region CreateMessage()

        [Fact(DisplayName = "CreateMessage with false response -> null args")]
        public async void CreateMessageWithNullArgs()
        {
            // Arrange
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.CreateMessage(null, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "CreateMessage with false response -> null message")]
        public async void CreateMessageWithNullMessage()
        {
            // Arrange
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.CreateMessage(null, "username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "CreateMessage with false response -> null username")]
        public async void CreateMessageWithNullUsername()
        {
            // Arrange
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.CreateMessage(new MessageViewModel(), null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "CreateMessage with false response -> null message content & null receiverId")]
        public async void CreateMessageWithNullMessageContentAndNullReceiverId()
        {
            // Arrange
            var message = new MessageViewModel
            {
                Conversation = 0,
                Content = null,
                ReceiverId = null
            };
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.CreateMessage(message, "username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "CreateMessage with false response -> null message content")]
        public async void CreateMessageWithNullMessageContent()
        {
            // Arrange
            var message = new MessageViewModel
            {
                Conversation = 0,
                Content = null,
                ReceiverId = "id"
            };
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.CreateMessage(message, "username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "CreateMessage with false response -> null message receiverId")]
        public async void CreateMessageWithNullReceiverId()
        {
            // Arrange
            var message = new MessageViewModel
            {
                Conversation = 0,
                Content = "content",
                ReceiverId = null
            };
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.CreateMessage(message, "username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "CreateMessage with false response -> user not found")]
        public async void CreateMessageWithUserNotFound()
        {
            // Arrange
            var message = new MessageViewModel
            {
                Conversation = 0,
                Content = "content",
                ReceiverId = "id"
            };
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.CreateMessage(message, "username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "CreateMessage with false response -> receiver not found")]
        public async void CreateMessageWithReceiverNotFound()
        {
            // Arrange
            var message = new MessageViewModel
            {
                Conversation = 0,
                Content = "content",
                ReceiverId = "id"
            };

            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id", UserName = "username" });
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.CreateMessage(message, "username");

            // Assert
            Assert.False(result.Succeeded);
        }

        #region CreateMessage with new conversation

        [Fact(DisplayName = "CreateMessage with false response -> conversation not created")]
        public async void CreateMessageWithConversationNotCreated()
        {
            // Arrange
            var message = new MessageViewModel
            {
                Conversation = 0,
                Content = "content",
                ReceiverId = "id"
            };
            var expectedErrorMessage = "ConversationNotCreated";

            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id", UserName = "username" });
            _identityService.GetUserById(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "idd", UserName = "username2" });
            _convRepository.CreateConversation(Arg.Any<Conversation>()).ReturnsForAnyArgs(0);
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.CreateMessage(message, "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact(DisplayName = "CreateMessage with false response -> Message not created")]
        public async void CreateMessageWithMessageNotCreated()
        {
            // Arrange
            var message = new MessageViewModel
            {
                Conversation = 0,
                Content = "content",
                ReceiverId = "id"
            };
            var expectedErrorMessage = "MessageNotCreated";

            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id", UserName = "username" });
            _identityService.GetUserById(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "idd", UserName = "username2" });
            _convRepository.CreateConversation(Arg.Any<Conversation>()).ReturnsForAnyArgs(1);
            _repository.CreateMessage(Arg.Any<Message>()).ReturnsForAnyArgs(false);
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.CreateMessage(message, "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact(DisplayName = "CreateMessage with true response -> message created")]
        public async void CreateMessageWithMessageCreated()
        {
            // Arrange
            var message = new MessageViewModel
            {
                Conversation = 0,
                Content = "content",
                ReceiverId = "id"
            };

            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id", UserName = "username" });
            _identityService.GetUserById(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "idd", UserName = "username2" });
            _convRepository.CreateConversation(Arg.Any<Conversation>()).ReturnsForAnyArgs(1);
            _repository.CreateMessage(Arg.Any<Message>()).ReturnsForAnyArgs(true);
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.CreateMessage(message, "username");

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region CreateMessage with existing conversation

        [Fact(DisplayName = "CreateMessage with false response -> existing conv not found")]
        public async void CreateMessageWithConversationNotFound()
        {
            // Arrange
            var message = new MessageViewModel
            {
                Conversation = 1,
                Content = "content",
                ReceiverId = "id"
            };
            var expectedErrorMessage = "ConversationNotFound";

            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id", UserName = "username" });
            _identityService.GetUserById(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "idd", UserName = "username2" });
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.CreateMessage(message, "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact(DisplayName = "CreateMessage with false response -> message not created (2)")]
        public async void CreateMessageWithMessageNotCreated2()
        {
            // Arrange
            var message = new MessageViewModel
            {
                Conversation = 1,
                Content = "content",
                ReceiverId = "id"
            };
            var expectedErrorMessage = "MessageNotCreated";

            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id", UserName = "username" });
            _identityService.GetUserById(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "idd", UserName = "username2" });
            _convRepository.GetConversationByIdAndUsers(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(new Conversation { Id = 1, FirstUser = "id", SecondUser = "id2" });
            _repository.CreateMessage(Arg.Any<Message>()).ReturnsForAnyArgs(false);
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.CreateMessage(message, "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact(DisplayName = "CreateMessage with true response -> message created (2)")]
        public async void CreateMessageWithMessageCreated2()
        {
            // Arrange
            var message = new MessageViewModel
            {
                Conversation = 1,
                Content = "content",
                ReceiverId = "id"
            };

            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id", UserName = "username" });
            _identityService.GetUserById(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "idd", UserName = "username2" });
            _convRepository.GetConversationByIdAndUsers(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(new Conversation { Id = 1, FirstUser = "id", SecondUser = "id2" });
            _repository.CreateMessage(Arg.Any<Message>()).ReturnsForAnyArgs(true);
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.CreateMessage(message, "username");

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion
        #endregion

        #region DeleteMessage()

        [Fact(DisplayName = "DeleteMessage with false response -> null username")]
        public async void DeleteMessageWithNullUsername()
        {
            // Arrange
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.DeleteMessage(1, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "DeleteMessage with false response -> user not found")]
        public async void DeleteMessageWithUserNotFound()
        {
            // Arrange
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.DeleteMessage(1, "username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "DeleteMessage with false response -> message not found")]
        public async void DeleteMessageWithMessageNotFound()
        {
            // Arrange
            var expectedErrorMessage = "MessageNotFound";

            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "idd" });
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.DeleteMessage(1, "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact(DisplayName = "DeleteMessage with false response -> user not authorized")]
        public async void DeleteMessageWithUserNotAuthorized()
        {
            // Arrange
            var expectedErrorMessage = "UserNotAuthorized";

            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "idd" });
            _repository.GetMessage(Arg.Any<int>()).ReturnsForAnyArgs(new Message { Sender = "id" });
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.DeleteMessage(1, "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact(DisplayName = "DeleteMessage with false response -> message not deleted")]
        public async void DeleteMessageWithMessageNotDeleted()
        {
            // Arrange
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id" });
            _repository.GetMessage(Arg.Any<int>()).ReturnsForAnyArgs(new Message { Sender = "id" });
            _repository.DeleteMessage(Arg.Any<int>()).ReturnsForAnyArgs(false);
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.DeleteMessage(1, "username");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "DeleteMessage with true response -> message deleted")]
        public async void DeleteMessageWithMessageDeleted()
        {
            // Arrange
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id" });
            _repository.GetMessage(Arg.Any<int>()).ReturnsForAnyArgs(new Message { Sender = "id" });
            _repository.DeleteMessage(Arg.Any<int>()).ReturnsForAnyArgs(true);
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.DeleteMessage(1, "username");

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region GetMessagesByConversation()

        [Fact(DisplayName = "GetMessagesByConversation with false response -> null username")]
        public async void GetMessagesByConversationWithNullUsername()
        {
            // Arrange
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.GetMessagesByConversation(1, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "GetMessagesByConversation with false response -> conversation not found")]
        public async void GetMessagesByConversationWithConversationNotFound()
        {
            // Arrange
            var expectedErrorMessage = "ConversationNotFound";

            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.GetMessagesByConversation(1, "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact(DisplayName = "GetMessagesByConversation with false response -> user but not in the conversation")]
        public async void GetMessagesByConversationWithUserNotFound()
        {
            // Arrange
            var expectedErrorMessage = "UserNotAuthorized";

            _convRepository.GetConversationById(Arg.Any<int>()).ReturnsForAnyArgs(new Conversation { Id = 1, FirstUser = "id", SecondUser = "id2" });
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "idd", UserName = "username" });
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.GetMessagesByConversation(1, "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact(DisplayName = "GetMessagesByConversation with false response -> no messages found")]
        public async void GetMessagesByConversationWithNoMessagesFound()
        {
            // Arrange
            var expectedErrorMessage = "NoMessageFound";

            _convRepository.GetConversationById(Arg.Any<int>()).ReturnsForAnyArgs(new Conversation { Id = 1, FirstUser = "id", SecondUser = "id2" });
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id", UserName = "username" });
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.GetMessagesByConversation(1, "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact(DisplayName = "GetMessagesByConversation with true response -> messages found")]
        public async void GetMessagesByConversationWithMessagesFound()
        {
            // Arrange
            int expectedMessageCount = 1;

            _convRepository.GetConversationById(Arg.Any<int>()).ReturnsForAnyArgs(new Conversation { Id = 1, FirstUser = "id", SecondUser = "id2" });
            _identityService.GetUserByUsername(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "id", UserName = "username" });
            _repository.GetMessagesByConversation(Arg.Any<int>()).ReturnsForAnyArgs(new List<Message> { new Message { Content = "content", Conversation = 1, Id = 1, Sender = "id" } });
            MessageService service = new MessageService(_repository, _convRepository, _identityService, _notifService);

            // Act
            var result = await service.GetMessagesByConversation(1, "username");

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
            Assert.IsType<List<MessageResponse>>(result.Result);

            var messages = result.Result as List<MessageResponse>;

            Assert.Equal(expectedMessageCount, messages.Count);
        }
        #endregion
    }
}
