using Projet_Forum.Data.Models;
using Projet_Forum.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Forum.Services.Interfaces
{
    public interface IConversationService
    {
        public Task<MyResponse> GetConversationsByUsername(string username);
    }
}
