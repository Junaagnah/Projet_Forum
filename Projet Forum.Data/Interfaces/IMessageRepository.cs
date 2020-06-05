using Projet_Forum.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Forum.Data.Interfaces
{
    public interface IMessageRepository
    {
        public List<Message> GetMessagesByConversation(int conversationId);

        public Task<bool> DeleteMessage(int messageId);

        public Task<bool> CreateMessage(Message msg);

        public Task<bool> DeleteMessagesByConversation(int convId);

        public Message GetMessage(int id);
    }
}
