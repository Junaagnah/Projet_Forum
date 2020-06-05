using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Projet_Forum.Data.Models;
using Projet_Forum.Services.Helpers;
using Projet_Forum.Services.ViewModels;
using Projet_Forum.Services.Services;
using Projet_Forum.Services.Interfaces;
using NSubstitute;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Xunit;
using Projet_Forum.Services.Models;
using System.Threading;

namespace Projet_Forum.Tests
{
    public class TestDataGenerator : IEnumerable<object[]>
    {
        public static IEnumerable<object[]> GetUserViewModelDataFromDataGenerator()
        {
            yield return new object[]
            {
                new RegisterViewModel { Username = "", Email = "test", Password = "test" },
                new RegisterViewModel { Username = "test", Email = "", Password = "test" },
                new RegisterViewModel { Username = "test", Email = "test", Password = "" },
                new RegisterViewModel { Username = "", Email = "", Password = "" }
            };
        }

        public static IEnumerable<object[]> GetUserViewModelDataWithoutPasswordFromDataGenerator()
        {
            yield return new object[]
            {
                new RegisterViewModel { Username = "", Email = "test" },
                new RegisterViewModel { Username = "test", Email = "" },
                new RegisterViewModel { Username = "", Email = "" },
                new RegisterViewModel { Username = "test", Email = "test" }
            };
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class IdentityServiceTests
    {
        #region Substitutes for services
        static IUserStore<User> GetUserStore() => Substitute.For<IUserStore<User>>();
        static UserManager<User> GetUserManager() => Substitute.For<UserManager<User>>(GetUserStore(), null, null, null, null, null, null, null, null);
        static IHttpContextAccessor GetContextAccessor() => Substitute.For<IHttpContextAccessor>();
        static IUserClaimsPrincipalFactory<User> GetUserClaimsPrincipalFactory() => Substitute.For<IUserClaimsPrincipalFactory<User>>();
        static SignInManager<User> GetSignInManager() => Substitute.For<SignInManager<User>>(GetUserManager(), GetContextAccessor(), GetUserClaimsPrincipalFactory(), null, null, null);
        static ISendgridService GetSendgridService() => Substitute.For<ISendgridService>();
        static IOptions<AppSettings> GetAppSettings() => Substitute.For<IOptions<AppSettings>>();
        static IRefreshTokenService GetRefreshTokenService() => Substitute.For<IRefreshTokenService>();
        static IImageService GetImageService() => Substitute.For<IImageService>();
        #endregion

        #region SignInUserAsync
        [Theory(DisplayName = "Signin with false response and null result -> empty email or password")]
        [InlineData("test", null)]
        [InlineData(null, "test")]
        [InlineData(null, null)]
        public async void SigninAsyncWithNullEmailOrPassword(string email, string password)
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.SignInUserAsync(email, password);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
        }

        [Fact(DisplayName = "Signin with false response and null result -> wrong email")]
        public async void SigninAsyncWithUserNotFound()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.SignInUserAsync("email", "password");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
        }

        [Fact(DisplayName = "Signin with false response and null result -> user is banned")]
        public async void SigninAsyncWithBannedUser()
        {
            // Arrange
            var expectedErrorMessage = "UserBanned"; // Message renvoyé par le serveur si l'utilisateur est banni
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByEmailAsync(Arg.Any<string>())
                    .ReturnsForAnyArgs(new User() { IsBanned = true });
            var mockedSigninManager = GetSignInManager();
            mockedSigninManager.PasswordSignInAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).ReturnsForAnyArgs(SignInResult.Success);
            IdentityService service = new IdentityService(mockedUserManager, mockedSigninManager, GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.SignInUserAsync("email", "password");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
            Assert.Equal(result.Messages[0], expectedErrorMessage);
        }

        [Fact(DisplayName = "Signin with false response and null result -> wrong password")]
        public async void SigninAsyncWithWrongPassword()
        {
            // Arrange
            var expectedErrorMessage = "WrongEmailOrPassword";
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync(Arg.Any<string>())
                    .ReturnsForAnyArgs(new User() { IsBanned = false });
            var mockedSignInManager = GetSignInManager();
            mockedSignInManager.PasswordSignInAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>())
                .ReturnsForAnyArgs(SignInResult.Failed);
            IdentityService service = new IdentityService(mockedUserManager, mockedSignInManager, GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.SignInUserAsync("email", "password");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
            Assert.Equal(result.Messages[0], expectedErrorMessage);
        }

        [Fact(DisplayName = "Signin with true response and tokens -> Eveything right")]
        public async void SigninAsyncWithRightCredentials()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByEmailAsync(Arg.Any<string>())
                    .ReturnsForAnyArgs(new User() { UserName = "test", Email = "test@test.fr", IsBanned = false, Role = 0 });
            var mockedSignInManager = GetSignInManager();
            mockedSignInManager.PasswordSignInAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>())
                .ReturnsForAnyArgs(SignInResult.Success);
            var mockedAppSettings = GetAppSettings();
            mockedAppSettings.Value.Returns(new AppSettings { Secret = "ez6f456zd8f1aze6daz54e896za41edq32s1d89z84ef3q2s1d68e4q3s5d1" });

            IdentityService service = new IdentityService(mockedUserManager, mockedSignInManager, GetSendgridService(), mockedAppSettings, GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.SignInUserAsync("NotBannedKnownUser", "NotBannedKnownUserPassword123*");

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
            Assert.IsType<SigningResponse>(result.Result);

            var token = result.Result as SigningResponse;

            Assert.NotNull(token.Token);
            Assert.NotNull(token.RefreshToken);
        }
        #endregion

        #region RegisterUserAsync()
        [Theory(DisplayName = "Register with false response -> username, email or password empty")]
        [MemberData(nameof(TestDataGenerator.GetUserViewModelDataFromDataGenerator), MemberType = typeof(TestDataGenerator))]
        public async void RegisterAsyncWithNullUsernameEmailOrPassword(RegisterViewModel a, RegisterViewModel b, RegisterViewModel c, RegisterViewModel d)
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var resultA = await service.RegisterUserAsync(a);
            var resultB = await service.RegisterUserAsync(b);
            var resultC = await service.RegisterUserAsync(c);
            var resultD = await service.RegisterUserAsync(d);

            // Assert
            Assert.False(resultA.Succeeded);
            Assert.False(resultB.Succeeded);
            Assert.False(resultC.Succeeded);
            Assert.False(resultD.Succeeded);
        }

        [Fact(DisplayName = "Register with false response -> user cannot be created")]
        public async void RegisterAsyncWithFailedUserCreation()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
                .ReturnsForAnyArgs(IdentityResult.Failed());
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var newUser = new RegisterViewModel()
            {
                Username = "test",
                Email = "test",
                Password = "test"
            };

            var result = await service.RegisterUserAsync(newUser);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "Register with false response -> user cannot be found after creation")]
        public async void RegisterAsyncWithUserNotFoundAfterCreation()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
                .ReturnsForAnyArgs(IdentityResult.Success);
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var newUser = new RegisterViewModel()
            {
                Username = "test",
                Email = "test",
                Password = "test"
            };

            var result = await service.RegisterUserAsync(newUser);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "Register with false response -> Confirmation Mail cannot be sent, deleting user if it's the case")]
        public async void RegisterAsyncWithConfirmationEmailNotSentAndUserDeleted()
        {
            // Arrange
            var expectedErrorMessage = "EmailNotSent";
            var mockedUserManager = GetUserManager();
            mockedUserManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
                .ReturnsForAnyArgs(IdentityResult.Success);
            mockedUserManager.FindByEmailAsync(Arg.Any<string>())
                .ReturnsForAnyArgs(new User());
            var mockedSendgridService = GetSendgridService();
            mockedSendgridService.SendValidationEmail(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .ReturnsForAnyArgs(false);
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var newUser = new RegisterViewModel()
            {
                Username = "test",
                Email = "test",
                Password = "test"
            };

            var result = await service.RegisterUserAsync(newUser);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(result.Messages[0], expectedErrorMessage);
            await mockedUserManager.Received().DeleteAsync(Arg.Any<User>());
        }

        [Fact(DisplayName = "Register with true response -> Everything worked well")]
        public async void RegisterAsyncWithTrueResponse()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
                .ReturnsForAnyArgs(IdentityResult.Success);
            mockedUserManager.FindByEmailAsync(Arg.Any<string>())
                .ReturnsForAnyArgs(new User());
            var mockedSendgridService = GetSendgridService();
            mockedSendgridService.SendValidationEmail(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .ReturnsForAnyArgs(true);
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), mockedSendgridService, GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var newUser = new RegisterViewModel()
            {
                Username = "test",
                Email = "test",
                Password = "test"
            };

            var result = await service.RegisterUserAsync(newUser);

            // Assert
            Assert.True(result.Succeeded);
            await mockedUserManager.DidNotReceive().DeleteAsync(Arg.Any<User>());
        }

        #endregion

        #region ConfirmEmail()

        [Theory(DisplayName = "ConfirmEmail with false response -> null userId And Token")]
        [InlineData(null, null)]
        [InlineData("userId", null)]
        [InlineData(null, "token")]
        public async void ConfirmEmailWithNullUserIdOrToken(string userId, string token)
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.ConfirmEmail(userId, token);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "ConfirmEmail with false result -> user not found")]
        public async void ConfirmEmailWithUserNotFound()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.ConfirmEmail("id", "token");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "ConfirmEmail with false result -> invalid token")]
        public async void ConfirmEmailWithInvalidToken()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByIdAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User());
            mockedUserManager.ConfirmEmailAsync(Arg.Any<User>(), Arg.Any<string>()).ReturnsForAnyArgs(IdentityResult.Failed());
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.ConfirmEmail("id", "token");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "ConfirmEmail with true result -> email confirmed")]
        public async void ConfirmEmailWithRightUserIdAndToken()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByIdAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User());
            mockedUserManager.ConfirmEmailAsync(Arg.Any<User>(), Arg.Any<string>()).ReturnsForAnyArgs(IdentityResult.Success);
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.ConfirmEmail("userId", "token");

            Assert.True(result.Succeeded);
        }
        #endregion

        #region AskPasswordRecovery()
        // Useless to test other cases, the function will always return true to avoid bruteforce even if the address don't exists
        [Fact(DisplayName = "AskPasswordRecovery with false response -> email not sent")]
        public async void AskPasswordRecoveryWithMailNotSent()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByEmailAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User { Id = "Id", Email = "email" });
            mockedUserManager.GeneratePasswordResetTokenAsync(Arg.Any<User>()).ReturnsForAnyArgs("token");
            var mockedSendgridService = GetSendgridService();
            mockedSendgridService.SendRecoveryEmail(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(false);

            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), mockedSendgridService, GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.AskPasswordRecovery("email");

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Messages);
        }
        #endregion

        #region RecoverPassword()

        [Theory(DisplayName = "RecoverPassword with false response -> null args")]
        [InlineData(null, null, null)]
        [InlineData("userId", null, null)]
        [InlineData(null, "password", null)]
        [InlineData(null, null, "token")]
        public async void RecoverPasswordWithNullArgs(string userId, string password, string token)
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.RecoverPassword(userId, password, token);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "RecoverPassword with false response -> user not found")]
        public async void RecoverPasswordWithWrongUserId()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.RecoverPassword("userId", "password", "token");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "RecoverPassword with false response -> invalid token")]
        public async void RecoverPasswordWithInvalidToken()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByIdAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User());
            mockedUserManager.ResetPasswordAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(IdentityResult.Failed());

            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.RecoverPassword("useId", "password", "token");

            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "RecoverPassword with true response -> password successfully changed")]
        public async void RecoverPasswordWithRightArgs()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByIdAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User());
            mockedUserManager.ResetPasswordAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(IdentityResult.Success);

            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.RecoverPassword("useId", "password", "token");

            Assert.True(result.Succeeded);
        }
        #endregion

        #region RenewToken()

        [Theory(DisplayName = "RenewToken with false response -> empty refreshToken or username")]
        [InlineData("FakeToken", null)]
        [InlineData(null, "FakeUsername")]
        [InlineData(null, null)]
        public async void RenewTokenWithEmptyRefreshTokenOrUsername(string refreshToken, string username)
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.RenewToken(refreshToken, username);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
        }

        [Fact(DisplayName = "RenewToken with false response -> user not found")]
        public async void RenewTokenWithUserNotFound()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.RenewToken("refreshToken", "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
        }

        [Fact(DisplayName = "RenewToken with false response -> user is banned")]
        public async void RenewTokenWithBannedUser()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync(Arg.Any<string>())
                .ReturnsForAnyArgs(new User { IsBanned = true });
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.RenewToken("refreshToken", "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotEmpty(result.Messages);
            Assert.Null(result.Result);
        }

        [Fact(DisplayName = "RenewToken with false response -> unknown refresh token")]
        public async void RenewTokenWithUnknownToken()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync(Arg.Any<string>())
                .ReturnsForAnyArgs(new User { Id = "id", IsBanned = false });
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.RenewToken("refreshToken", "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
        }

        [Fact(DisplayName = "RenewToken with false response -> RefreshToken is outdated")]
        public async void RenewTokenWithOutdatedToken()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByEmailAsync(Arg.Any<string>())
                .ReturnsForAnyArgs(new User { Id = "id", UserName = "username", IsBanned = false, Role = 0 });
            var mockedTokenService = GetRefreshTokenService();
            mockedTokenService.GetRefreshTokenByValueAndUserId(Arg.Any<string>(), Arg.Any<string>())
                .ReturnsForAnyArgs(new RefreshToken() { TokenValue = "tokenValue", ValidBefore = DateTime.Now.AddMilliseconds(-1) });
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), mockedTokenService, GetImageService());

            // Act
            var result = await service.RenewToken("refreshToken", "username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
        }

        [Fact(DisplayName = "RenewToken with true response -> right token still valid and valid user")]
        public async void RenewTokenWithRightAndValidTokenAndRightUsername()
        {
            // Arrange
            var oldRefreshToken = "oldRefreshToken";
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync(Arg.Any<string>())
                .ReturnsForAnyArgs(new User { Id = "id", UserName = "test", Email = "test@test.fr", IsBanned = false, Role = 0 });
            var mockedTokenService = GetRefreshTokenService();
            mockedTokenService.GetRefreshTokenByValueAndUserId(Arg.Any<string>(), Arg.Any<string>())
                .ReturnsForAnyArgs(new RefreshToken() { TokenValue = oldRefreshToken, ValidBefore = DateTime.Now.AddDays(1) });
            var mockedAppSettings = GetAppSettings();
            mockedAppSettings.Value.Returns(new AppSettings { Secret = "ez6f456zd8f1aze6daz54e896za41edq32s1d89z84ef3q2s1d68e4q3s5d1" });
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), mockedAppSettings, mockedTokenService, GetImageService());

            // Act
            var result = await service.RenewToken(oldRefreshToken, "username");

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
            Assert.IsType<SigningResponse>(result.Result);

            var newToken = result.Result as SigningResponse;

            Assert.NotNull(newToken.RefreshToken);
            Assert.NotNull(newToken.Token);
            Assert.NotEqual(newToken.RefreshToken, oldRefreshToken);
        }
        #endregion

        #region GetUserByUsername()

        [Fact(DisplayName = "GetUserByUsername with null response -> null arg")]
        public async void GetUserByUsernameWithNullArg()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.GetUserByUsername(null);

            Assert.Null(result);
        }

        [Fact(DisplayName = "GetUserByUsername with null response -> user not found")]
        public async void GetUserByUsernameWithUserNotFound()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.GetUserByUsername("username");

            // Assert
            Assert.Null(result);
        }

        [Fact(DisplayName = "GetUserByUsername with User response -> user found")]
        public async void GetUserByUsernameWithUserFound()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User());
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.GetUserByUsername("username");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<User>(result);
        }
        #endregion

        #region GetUserById()

        [Fact(DisplayName = "GetUserById with null response -> null arg")]
        public async void GetUserByIdWithNullArg()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.GetUserById(null);

            Assert.Null(result);
        }

        [Fact(DisplayName = "GetUserById with null response -> user not found")]
        public async void GetUserByIdWithUserNotFound()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.GetUserById("username");

            // Assert
            Assert.Null(result);
        }

        [Fact(DisplayName = "GetUserById with User response -> user found")]
        public async void GetUserByIdWithUserFound()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByIdAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User());
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.GetUserById("username");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<User>(result);
        }
        #endregion

        #region GetUserProfile()

        [Theory(DisplayName = "GetUserProfile with false response -> null args")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async void GetUserProfileWithNullArgs(string username)
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.GetUserProfileByUsername(username);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
        }

        [Fact(DisplayName = "GetUserProfile with false response -> user not found")]
        public async void GetUserProfileWithUserNotFound()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.GetUserProfileByUsername("username");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Result);
        }

        [Fact(DisplayName = "GetUserProfile with true response -> user found")]
        public async void GetUserProfileWithRightUsername()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User { UserName = "username", Role = 0, Description = "description" });
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.GetUserProfileByUsername("username");

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Result);
            Assert.IsType<ProfileResponse>(result.Result);

            var response = result.Result as ProfileResponse;

            Assert.NotNull(response.Username);
            Assert.NotNull(response.Description);
            Assert.NotNull(response.RoleName);
        }
        #endregion

        #region UpdateUserPassword()

        [Theory(DisplayName = "UpdateUserPassword with false response -> null or empty args")]
        [InlineData(null, null, null)]
        [InlineData("username", null, null)]
        [InlineData(null, "oldPassword", null)]
        [InlineData(null, null, "newPassword")]
        public async void UpdateUserPasswordWithNullArgs(string username, string oldPassword, string newPassword)
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserPassword(username, oldPassword, newPassword);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "UpdateUserPassword with false response -> user not found")]
        public async void UpdateUserPasswordWithUserNotFound()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserPassword("username", "oldPassword", "newPassword");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "UpdateUserPassword with false response -> wrong oldPassword")]
        public async void UpdateUserPasswordWithWrongOldPassword()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User());
            mockedUserManager.ChangePasswordAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(IdentityResult.Failed());
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserPassword("username", "oldPassword", "newPassword");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "UpdateUserPassword with true response -> Right user and oldPassword")]
        public async void UpdateUserPasswordWithRightArgs()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User());
            mockedUserManager.ChangePasswordAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>()).ReturnsForAnyArgs(IdentityResult.Success);
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserPassword("username", "oldPassword", "newPassword");

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region UpdateUserProfile()

        [Fact(DisplayName = "UpdateUserProfile with false response -> null args")]
        public async void UpdateUserProfileWithNullArgs()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserProfile(null, null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "UpdateUserProfile with false response -> null data")]
        public async void UpdateUserProfileWithNullData()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserProfile("username", null);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "UpdateUserProfile with false response -> null username")]
        public async void UpdateUserProfileWithNullUsername()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserProfile(null, new UpdateProfileViewModel());

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "UpdateUserProfile with false response -> null data content")]
        public async void UpdateUserProfileWithNullDataContent()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserProfile("username", new UpdateProfileViewModel());

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "UpdateUserProfile with false response -> user not found")]
        public async void UpdateUserProfileWithUserNotFound()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserProfile("username", new UpdateProfileViewModel { Description = "Description" });

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "UpdateUserProfile with false response -> update failed")]
        public async void UpdateUserProfileWithFailedUpdate()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User { UserName = "username", Description = "description" });
            mockedUserManager.UpdateAsync(Arg.Any<User>()).ReturnsForAnyArgs(IdentityResult.Failed());
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserProfile("username", new UpdateProfileViewModel { Description = "newDescription" });

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "UpdateUserProfile with true response -> user successfully updated")]
        public async void UpdateUserProfileWithRightArgs()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User { UserName = "username", Description = "description" });
            mockedUserManager.UpdateAsync(Arg.Any<User>()).ReturnsForAnyArgs(IdentityResult.Success);
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserProfile("username", new UpdateProfileViewModel { Description = "newDescription" });

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region DeleteUserAccount()

        [Theory(DisplayName = "DeleteUserAccount with false response -> null args")]
        [InlineData(null, null)]
        [InlineData("username", null)]
        [InlineData(null, "password")]
        public async void DeleteUserAccountWithNullArgs(string username, string password)
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.DeleteUserAccount(username, password);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "DelteUserAccount with false response -> user not found")]
        public async void DeleteUserAccountWithUserNotFound()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.DeleteUserAccount("username", "password");

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "DeleteUserAccount with false response -> wrong password")]
        public async void DeleteUserAccountWithWrongPassword()
        {
            // Arrange
            var expectedErrorMessage = "IncorrectPassword";
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User());
            var mockedSignInManager = GetSignInManager();
            mockedSignInManager.PasswordSignInAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).ReturnsForAnyArgs(SignInResult.Failed);

            IdentityService service = new IdentityService(mockedUserManager, mockedSignInManager, GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.DeleteUserAccount("username", "password");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(result.Messages[0], expectedErrorMessage);
        }

        [Fact(DisplayName = "DeleteUserAccount with true response -> everything is right")]
        public async void DeleteUserAccountWithRightArgs()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User());
            mockedUserManager.UpdateAsync(Arg.Any<User>()).ReturnsForAnyArgs(IdentityResult.Success);
            var mockedSignInManager = GetSignInManager();
            mockedSignInManager.PasswordSignInAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>()).ReturnsForAnyArgs(SignInResult.Success);

            IdentityService service = new IdentityService(mockedUserManager, mockedSignInManager, GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.DeleteUserAccount("username", "password");

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region SearchUserByUsername()

        [Theory(DisplayName = "SearchUserByUsername with false response -> null or empty username")]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData(null, "")]
        [InlineData("", "")]
        public async void SearchUserByUsernameWithEmptyArg(string username, string currentUsername)
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.SearchUserByUsername(username, currentUsername);

            // Assert
            Assert.False(result.Succeeded);
        }

        //[Fact(DisplayName = "SearchUserByUsername with true response -> right args")]
        //public async void SearchUserByUsernaleWithRightArgs()
        //{
        //    // Arrange
        //    var userMgr = GetUserManager();

        //    IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

        //    // Act
        //    var result = await service.SearchUserByUsername("username");

        //    // Assert
        //    Assert.True(result.Succeeded);
        //    Assert.Null(result.Result);
        //}
        #endregion

        #region UpdateUserBan()

        [Theory(DisplayName = "UpdateUserBan with false response -> empty args")]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData(null, "")]
        [InlineData(" ", null)]
        [InlineData(null, " ")]
        [InlineData(" ", "")]
        [InlineData("", " ")]
        [InlineData("edeed", "")]
        [InlineData("", "eefgzefz")]
        public async void UpdateUserBanWithEmptyArgs(string usernameToBan, string actualUser)
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserBan(usernameToBan, actualUser, true);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "UpdateUserBan with false response -> actualUser not authorized")]
        public async void UpdateUserBanWithActualUserNotFound()
        {
            // Arrange
            var expectedErrorMessage = "UserNotAuthorized";

            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync(Arg.Any<string>()).ReturnsForAnyArgs(new User { UserName = "actual", Role = 1 });
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserBan("toBan", "actual", true);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact(DisplayName = "UpdateUserBan with false response -> user not found")]
        public async void UpdateUserBanWithUserNotFound()
        {
            // Arrange
            var expectedErrorMessage = "UserNotFound";

            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync("actual").Returns(new User { UserName = "actual", Role = 2 });
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserBan("toBan", "actual", true);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal(expectedErrorMessage, result.Messages[0]);
        }

        [Fact(DisplayName = "UpdateUserBan with true response -> user ban successfully updated")]
        public async void UpdateUserBanWithTrueResponse()
        {
            // Arrange
            var mockedUserManager = GetUserManager();
            mockedUserManager.FindByNameAsync("actual").Returns(new User { UserName = "actual", Role = 2 });
            mockedUserManager.FindByNameAsync("toBan").Returns(new User { UserName = "toBan", IsBanned = false });
            mockedUserManager.UpdateAsync(Arg.Any<User>()).ReturnsForAnyArgs(IdentityResult.Success);
            IdentityService service = new IdentityService(mockedUserManager, GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = await service.UpdateUserBan("toBan", "actual", true);

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion

        #region GetUsersWithPagination()

        [Theory(DisplayName = "GetUsersWithPagination with false response -> role not valid")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void GetUsersWithPaginationWithWrongRole(int role)
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = service.GetUsersWithPagination(0, role);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact(DisplayName = "GetUsersWithPagination with true response -> right role (admin)")]
        public void GetUsersWithPaginationWithRightRole()
        {
            // Arrange
            IdentityService service = new IdentityService(GetUserManager(), GetSignInManager(), GetSendgridService(), GetAppSettings(), GetRefreshTokenService(), GetImageService());

            // Act
            var result = service.GetUsersWithPagination(0, 3);

            // Assert
            Assert.True(result.Succeeded);
        }
        #endregion
    }
}