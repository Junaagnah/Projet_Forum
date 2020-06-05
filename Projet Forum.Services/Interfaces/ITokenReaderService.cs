using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projet_Forum.Services.Interfaces
{
    public interface ITokenReaderService
    {
        public string GetTokenUsername(string accesstoken);
        public string GetTokenRole(string accesstoken);
    }
}
