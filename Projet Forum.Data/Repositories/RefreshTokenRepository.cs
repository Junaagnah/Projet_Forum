using Microsoft.EntityFrameworkCore;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Forum.Data.Repositories
{
    /// <summary>
    /// Repository permettant de gérer les repositories
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly MyDbContext _context;
        public RefreshTokenRepository(MyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Permet de créer un refreshToken
        /// </summary>
        /// <param name="token"></param>
        public async Task CreateRefreshToken(RefreshToken token)
        {
            if (token != null)
            {
                _context.RefreshTokens.Add(token);
                await _context.SaveChangesAsync();
            }

            // Change fois qu'un token est créé on supprime les tokens invalides
            DeleteInvalidTokens();
        }

        /// <summary>
        /// Permet de supprimer un refreshToken
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> DeleteRefreshToken(string token)
        {
            var gotToken = await _context.RefreshTokens.FirstOrDefaultAsync(tok => tok.TokenValue == token);
            if (gotToken != null)
            {
                _context.RefreshTokens.Remove(gotToken);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Permet de récupérer un refreshToken via un id utilisateur
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>RefreshToken</returns>
        public async Task<RefreshToken> GetRefreshTokenByUserId(string userId)
        {
            RefreshToken token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);

            return token;
        }

        /// <summary>
        /// Permet de récupérer un refreshToken par sa valeur
        /// </summary>
        /// <param name="token"></param>
        /// <returns>RefreshToken</returns>
        public async Task<RefreshToken> GetRefreshTokenByValue(string token)
        {
            RefreshToken refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenValue == token);

            return refreshToken;
        }

        /// <summary>
        /// Permet de récupérer un refreshToken par sa valeur et un id utilisateur
        /// </summary>
        /// <param name="token"></param>
        /// <param name="userId"></param>
        /// <returns>RefreshToken</returns>
        public async Task<RefreshToken> GetRefreshTokenByValueAndUserId(string token, string userId)
        {
            RefreshToken refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenValue == token && t.UserId == userId);

            return refreshToken;
        }

        // Purge la table des tokens et force la reconnection pour tous les utilisateurs
        public async Task PurgeRefreshTokens()
        {
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE RefreshTokens");
        }

        /// <summary>
        /// Permet de supprimer tous les tokens invalides
        /// </summary>
        private async void DeleteInvalidTokens()
        {
            var invalidTokens = _context.RefreshTokens.Where(token => token.ValidBefore < DateTime.Now).ToList();

            foreach(var token in invalidTokens)
            {
                _context.RefreshTokens.Remove(token);
            }

            if (invalidTokens.Count > 0) await _context.SaveChangesAsync();
        }
    }
}
