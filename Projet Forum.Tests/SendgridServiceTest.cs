using Microsoft.Extensions.Options;
using Projet_Forum.Services.Helpers;
using System;
using Xunit;
using NSubstitute;
using Projet_Forum.Services.Services;

namespace Projet_Forum.Tests
{
    public class SendgridServiceTest
    {
        private readonly IOptions<AppSettings> _appSettings = Substitute.For<IOptions<AppSettings>>();

        public SendgridServiceTest()
        {
            _appSettings.Value.Returns(new AppSettings { ApplicationUrl = "http://localhost:4200",
                SendgridApiKey = "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",
                ContactEmail = "nicolas.gehringer@outlook.fr" });
        }

        #region SendValidationEmail()
        [Theory(DisplayName = "Send Validation Email with null values")]
        [InlineData(null, null, null)]
        [InlineData("email", null, null)]
        [InlineData(null, "userId", null)]
        [InlineData(null, null, "token")]
        public async void SendValidationEmailWithNull(string email, string userId, string token)
        {
            // Act
            SendgridService sendgridService = new SendgridService(_appSettings);

            var result = await sendgridService.SendValidationEmail(email, userId, token);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async void SendValidationEmailWithWrongFormatEmail()
        {
            // Arrange
            var email = "email";

            // Act
            SendgridService sendgridService = new SendgridService(_appSettings);

            var result = await sendgridService.SendValidationEmail(email, "userId", "token");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async void SendValidationEmailWithRightFormatEmail()
        {
            // Arrange
            var email = "blablabla@test.com";

            // Act
            SendgridService sendgridService = new SendgridService(_appSettings);

            var result = await sendgridService.SendValidationEmail(email, "userId", "token", true);

            // Assert
            Assert.True(result);
        }
        #endregion

        #region SendRecoveryEmail
        [Theory(DisplayName = "Send Recovery Email with null values")]
        [InlineData(null, null, null)]
        [InlineData("email", null, null)]
        [InlineData(null, "userId", null)]
        [InlineData(null, null, "token")]
        public async void SendRecoveryEmailWithNull(string email, string userId, string token)
        {
            // Act
            SendgridService sendgridService = new SendgridService(_appSettings);

            var result = await sendgridService.SendRecoveryEmail(email, userId, token);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async void SendRecoveryEmailWithWrongFormatEmail()
        {
            // Arrange
            var email = "email";

            // Act
            SendgridService sendgridService = new SendgridService(_appSettings);

            var result = await sendgridService.SendRecoveryEmail(email, "userId", "token");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async void SendRecoveryEmailWithRightFormatEmail()
        {
            // Arrange
            var email = "blablabla@test.com";

            // Act
            SendgridService sendgridService = new SendgridService(_appSettings);

            var result = await sendgridService.SendRecoveryEmail(email, "userId", "token", true);

            // Assert
            Assert.True(result);
        }
        #endregion

        #region SendContactEmail
        [Theory(DisplayName = "Send Validation Email with null values")]
        [InlineData(null, null)]
        [InlineData("email", null)]
        [InlineData(null, "message")]
        public async void SendContactEmailWithNull(string email, string message)
        {
            // Act
            SendgridService sendgridService = new SendgridService(_appSettings);

            var result = await sendgridService.SendContactEmail(email, message);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void SendContactEmailWithWrongFormatEmail()
        {
            // Arrange
            var email = "email";

            // Act
            SendgridService sendgridService = new SendgridService(_appSettings);

            var result = await sendgridService.SendContactEmail(email, "userId");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async void SendContactEmailWithRightFormatEmail()
        {
            // Arrange
            var email = "blablabla@test.com";

            // Act
            SendgridService sendgridService = new SendgridService(_appSettings);

            var result = await sendgridService.SendContactEmail(email, "message");

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion
    }
}
