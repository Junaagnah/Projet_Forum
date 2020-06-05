using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Projet_Forum.Data.Models;

namespace Projet_Forum.Data.Interfaces
{
    public interface IRefreshTokenRepository
    {
        public Task CreateRefreshToken(RefreshToken token);

        public Task<bool> DeleteRefreshToken(string token);

        // Suppression de tous les tokens de la bdd (force la reconnection de tous les utilisateurs)
        public Task PurgeRefreshTokens();

        public Task<RefreshToken> GetRefreshTokenByUserId(string userId);

        public Task<RefreshToken> GetRefreshTokenByValue(string token);

        public Task<RefreshToken> GetRefreshTokenByValueAndUserId(string token, string userId);
    }
}
