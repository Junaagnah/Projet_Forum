using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;

namespace Projet_Forum.Services.Services
{
    /// <summary>
    /// Service permettant de gérer les RefreshTokens
    /// </summary>
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _repo;

        public RefreshTokenService(IRefreshTokenRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Permet de créer un RefreshToken
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> CreateRefreshToken(RefreshToken token)
        {
            var result = false;

            if (token != null)
            {
                if (!String.IsNullOrWhiteSpace(token.TokenValue) && !String.IsNullOrWhiteSpace(token.UserId))
                {
                    await _repo.CreateRefreshToken(token);
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Permet de supprimer un RefreshToken
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> DeleteRefreshToken(string token)
        {
            var result = false;

            if (!String.IsNullOrWhiteSpace(token))
                result = await _repo.DeleteRefreshToken(token);

            return result;
        }

        /// <summary>
        /// Permet de récupérer un RefreshToken via l'id d'un utilisateur
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>RefreshToken</returns>
        public async Task<RefreshToken> GetRefreshTokenByUserId(string userId)
        {
            RefreshToken token = null;

            if (!String.IsNullOrWhiteSpace(userId))
                token = await _repo.GetRefreshTokenByUserId(userId);

            return token;
        }

        /// <summary>
        /// Permet de récupérer un RefreshToken via sa valeur
        /// </summary>
        /// <param name="token"></param>
        /// <returns>RefreshToken</returns>
        public async Task<RefreshToken> GetRefreshTokenByValue(string token)
        {
            RefreshToken refreshToken = null;

            if (!String.IsNullOrWhiteSpace(token))
                refreshToken = await _repo.GetRefreshTokenByValue(token);

            return refreshToken;
        }

        /// <summary>
        /// Permet de récupérer un RefreshToken via sa valeur et l'id d'un utilisateur
        /// </summary>
        /// <param name="token"></param>
        /// <param name="userId"></param>
        /// <returns>RefreshToken</returns>
        public async Task<RefreshToken> GetRefreshTokenByValueAndUserId(string token, string userId)
        {
            RefreshToken refreshToken = null;

            if (!String.IsNullOrWhiteSpace(token) && !String.IsNullOrWhiteSpace(userId))
                refreshToken = await _repo.GetRefreshTokenByValueAndUserId(token, userId);

            return refreshToken;
        }

        public async Task PurgeRefreshTokens()
        {
            await _repo.PurgeRefreshTokens();
        }
    }
}
