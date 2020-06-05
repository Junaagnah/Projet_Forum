using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projet_Forum.Data.Models;

namespace Projet_Forum.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        public Task<bool> CreateRefreshToken(RefreshToken token);

        public Task<bool> DeleteRefreshToken(string token);

        public Task PurgeRefreshTokens();

        public Task<RefreshToken> GetRefreshTokenByUserId(string userId);

        public Task<RefreshToken> GetRefreshTokenByValue(string token);

        public Task<RefreshToken> GetRefreshTokenByValueAndUserId(string token, string userId);
    }
}
