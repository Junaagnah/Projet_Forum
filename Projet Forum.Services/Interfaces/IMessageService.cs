using Projet_Forum.Data.Models;
using Projet_Forum.Services.Models;
using Projet_Forum.Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Forum.Services.Interfaces
{
    public interface IMessageService
    {
        public Task<MyResponse> GetMessagesByConversation(int convId, string username);

        public Task<MyResponse> DeleteMessage(int msgId, string username);

        public Task<MyResponse> CreateMessage(MessageViewModel msg, string username);
    }
}
