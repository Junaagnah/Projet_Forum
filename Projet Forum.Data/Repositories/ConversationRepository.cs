using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Projet_Forum.Data.Repositories
{
    /// <summary>
    /// Classe contenant les fonctions CRUD pour les conversations
    /// </summary>
    public class ConversationRepository : IConversationRepository
    {
        private readonly MyDbContext _context;

        public ConversationRepository(MyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Permet de récupérer les conversations d'un utilisateur
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>List<Conversation></returns>
        public List<Conversation> GetConversationsByUserId(string userId)
        {
            List<Conversation> conversations = _context.Conversations.Where(conv => conv.FirstUser == userId || conv.SecondUser == userId).OrderByDescending(conv => conv.LastMessageDate).ToList();

            return conversations;
        }

        /// <summary>
        /// Permet de créer une conversation
        /// </summary>
        /// <param name="conv"></param>
        /// <returns>Conversation.Id</returns>
        public async Task<int> CreateConversation(Conversation conv)
        {
            if (conv != null)
            {
                _context.Conversations.Add(conv);
                await _context.SaveChangesAsync();

                return conv.Id;
            }

            return 0;
        }

        /// <summary>
        /// Permet de mettre à jour la date du dernier message d'une conversation (la fait remonter dans la liste)
        /// </summary>
        /// <param name="convId"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> UpdateConversationDate(int convId)
        {
            var conv = _context.Conversations.FirstOrDefault(conv => conv.Id == convId);
            if (conv != null)
            {
                conv.LastMessageDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return false;
        }

        /// <summary>
        /// Permet de récuperer une converation via deux utilisateurs
        /// </summary>
        /// <param name="firstUserId"></param>
        /// <param name="secondUserId"></param>
        /// <returns>Conversation</returns>
        public Conversation GetConversationByUsers(string firstUserId, string secondUserId)
        {
            var conv = _context.Conversations.FirstOrDefault(conv => (conv.FirstUser == firstUserId || conv.SecondUser == firstUserId) && (conv.FirstUser == secondUserId || conv.SecondUser == secondUserId));

            return conv;
        }

        /// <summary>
        /// Permet de récupérer une conversation via son id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Conversation</returns>
        public Conversation GetConversationById(int id)
        {
            var conversation = _context.Conversations.FirstOrDefault(conv => conv.Id == id);

            return conversation;
        }

        /// <summary>
        /// Permet de récupérer une conversation via son id & ses utilisateurs (permet d'éviter qu'un utilisateur malintentionné essaie d'insérer un de ses messages dans une conversation existante
        /// </summary>
        /// <param name="id"></param>
        /// <param name="firstId"></param>
        /// <param name="secondId"></param>
        /// <returns></returns>
        public Conversation GetConversationByIdAndUsers(int id, string firstId, string secondId)
        {
            var conversation = _context.Conversations.FirstOrDefault(conv => conv.Id == id && ((conv.FirstUser == firstId || conv.SecondUser == firstId) && (conv.FirstUser == secondId || conv.SecondUser == secondId)));

            return conversation;
        }
    }
}
