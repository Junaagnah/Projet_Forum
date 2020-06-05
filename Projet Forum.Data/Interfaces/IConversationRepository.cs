using Projet_Forum.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Forum.Data.Interfaces
{
    public interface IConversationRepository
    {
        public List<Conversation> GetConversationsByUserId(string userId);

        public Task<int> CreateConversation(Conversation conv);

        public Task<bool> UpdateConversationDate(int convId);

        public Conversation GetConversationByUsers(string firstUserId, string secondUserId);

        public Conversation GetConversationById(int id);

        public Conversation GetConversationByIdAndUsers(int id, string firstId, string secondId);
    }
}
