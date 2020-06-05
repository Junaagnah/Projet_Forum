using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projet_Forum.Services.Models
{
    public class SigningResponse
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public bool IsBanned { get; set; }
    }
}
