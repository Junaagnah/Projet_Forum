using System;
using Xunit;
using NSubstitute;
using Projet_Forum.Services.Services;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;

namespace Projet_Forum.Tests
{
    public class RefreshTokenServiceTest
    {
        private readonly IRefreshTokenRepository _repo = Substitute.For<IRefreshTokenRepository>();

        [Fact]
        public async void CreateRefreshTokenWithNull()
        {
            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.CreateRefreshToken(null);

            Assert.False(result);
        }

        [Fact]
        public async void CreateRefreshTokenWithNullTokenValue()
        {
            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var token = new RefreshToken
            {
                TokenValue = null,
                UserId = "id"
            };

            var result = await refreshService.CreateRefreshToken(token);

            Assert.False(result);
        }

        [Fact]
        public async void CreateRefreshTokenWithNullUserId()
        {
            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var token = new RefreshToken
            {
                TokenValue = "value",
                UserId = null
            };

            var result = await refreshService.CreateRefreshToken(token);

            Assert.False(result);
        }

        [Fact]
        public async void CreateRefreshTokenWithNonNull()
        {
            var token = new RefreshToken
            {
                TokenValue = "value",
                UserId = "userId"
            };

            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.CreateRefreshToken(token);

            Assert.True(result);
        }

        [Fact]
        public async void DeleteRefreshTokenWithNull()
        {
            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.DeleteRefreshToken(null);

            Assert.False(result);
        }

        [Fact]
        public async void DeleteRefreshTokenWithWrongToken()
        {
            string token = "token";
            _repo.DeleteRefreshToken(token).Returns(false);

            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.DeleteRefreshToken(token);

            Assert.False(result);
        }

        [Fact]
        public async void DeleteRefreshTokenWithRightToken()
        {
            string token = "token";
            _repo.DeleteRefreshToken(token).Returns(true);

            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.DeleteRefreshToken(token);

            Assert.True(result);
        }

        [Fact]
        public async void GetRefreshTokenByUserIdWithNull()
        {
            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.GetRefreshTokenByUserId(null);

            Assert.Null(result);
        }

        [Fact]
        public async void GetRefreshTokenByUserIdWithWrongId()
        {
            string id = "id";

            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.GetRefreshTokenByUserId(id);

            Assert.Null(result);
        }

        [Fact]
        public async void GetRefreshTokenByUserIdWithRightId()
        {
            string id = "id";
            var token = new RefreshToken
            {
                TokenValue = "token",
                UserId = id
            };

            _repo.GetRefreshTokenByUserId(id).Returns(token);

            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.GetRefreshTokenByUserId(id);

            Assert.NotNull(result);
            Assert.IsType<RefreshToken>(result);
            Assert.NotNull(result.TokenValue);
            Assert.NotNull(result.UserId);
            Assert.Equal(result.UserId, id);
        }

        [Fact]
        public async void GetRefreshTokenByValueWithNull()
        {
            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.GetRefreshTokenByValue(null);

            Assert.Null(result);
        }

        [Fact]
        public async void GetRefreshTokenByValueWithWrongId()
        {
            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.GetRefreshTokenByValue("id");

            Assert.Null(result);
        }

        [Fact]
        public async void GetRefreshTokenByValueWithRightId()
        {
            string tokenValue = "tokenValue";
            var token = new RefreshToken
            {
                TokenValue = tokenValue,
                UserId = "id"
            };

            _repo.GetRefreshTokenByValue(tokenValue).Returns(token);

            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.GetRefreshTokenByValue(tokenValue);

            Assert.NotNull(result);
            Assert.IsType<RefreshToken>(result);
            Assert.NotNull(result.TokenValue);
            Assert.NotNull(result.UserId);
            Assert.Equal(result.TokenValue, tokenValue);
        }

        [Fact]
        public async void GetRefreshTokenByValueAndUserIdWithNull()
        {
            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.GetRefreshTokenByValueAndUserId(null, null);

            Assert.Null(result);
        }

        [Fact]
        public async void GetRefreshTokenByValueAndUserIdWithNullValue()
        {
            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.GetRefreshTokenByValueAndUserId(null, "userId");

            Assert.Null(result);
        }

        [Fact]
        public async void GetRefreshTokenByValueAndUserIdWithNullUserId()
        {
            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.GetRefreshTokenByValueAndUserId("value", null);

            Assert.Null(result);
        }

        [Fact]
        public async void GetRefreshTokenByValueAndUserIdWithWrongValueOrUserId()
        {
            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.GetRefreshTokenByValueAndUserId("value", "userId");

            Assert.Null(result);
        }

        [Fact]
        public async void GetRefreshTokenByValueAndUserIdWithRightValueAndUserId()
        {
            string tokenValue = "tokenValue";
            string userId = "id";
            var token = new RefreshToken
            {
                TokenValue = tokenValue,
                UserId = userId
            };

            _repo.GetRefreshTokenByValueAndUserId(tokenValue, userId).Returns(token);

            RefreshTokenService refreshService = new RefreshTokenService(_repo);

            var result = await refreshService.GetRefreshTokenByValueAndUserId(tokenValue, userId);

            Assert.NotNull(result);
            Assert.IsType<RefreshToken>(result);
            Assert.NotNull(result.TokenValue);
            Assert.NotNull(result.UserId);
            Assert.Equal(result.TokenValue, tokenValue);
            Assert.Equal(result.UserId, userId);
        }
    }
}
