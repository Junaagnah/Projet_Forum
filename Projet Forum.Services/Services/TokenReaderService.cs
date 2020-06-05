using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Projet_Forum.Services.Interfaces;

namespace Projet_Forum.Services.Services
{
    /// <summary>
    /// Classe permettant de récupérer des informations contenues dans les tokens
    /// </summary>
    public class TokenReaderService : ITokenReaderService
    {
        /// <summary>
        /// Permet de récupérer le nom d'utilisateur depuis le token
        /// </summary>
        /// <param name="accesstoken"></param>
        /// <returns>String</returns>
        public string GetTokenUsername(string accesstoken)
        {
            if (!String.IsNullOrWhiteSpace(accesstoken))
            {
                accesstoken = accesstoken.Replace("Bearer ", "");
                return new JwtSecurityTokenHandler().ReadJwtToken(accesstoken).Claims.First(claim => claim.Type == "username").Value;
            }

            return null;
        }

        /// <summary>
        /// Permet de récupérer le rôle depuis le token
        /// </summary>
        /// <param name="accesstoken"></param>
        /// <returns>String</returns>
        public string GetTokenRole(string accesstoken)
        {
            if (!String.IsNullOrWhiteSpace(accesstoken))
            {
                accesstoken = accesstoken.Replace("Bearer ", "");
                return new JwtSecurityTokenHandler().ReadJwtToken(accesstoken).Claims.First(claim => claim.Type == "role").Value;
            }

            return null;
        }
    }
}
