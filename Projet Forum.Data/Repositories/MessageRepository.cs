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
    /// Classe contenant les fonctions CRUD pour les messages
    /// </summary>
    public class MessageRepository : IMessageRepository
    {
        private readonly MyDbContext _context;

        public MessageRepository(MyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Permet d'enregistrer un message en bdd
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>Bollean</returns>
        public async Task<bool> CreateMessage(Message msg)
        {
            if (msg != null)
            {
                _context.Messages.Add(msg);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Permet de supprimer un message
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> DeleteMessage(int messageId)
        {
            var msg = _context.Messages.FirstOrDefault(msg => msg.Id == messageId);
            if (msg != null)
            {
                _context.Messages.Remove(msg);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Permet de supprimer tous les messages d'une conversation via son Id
        /// </summary>
        /// <param name="convId"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> DeleteMessagesByConversation(int convId)
        {
            List<Message> msgs = GetMessagesByConversation(convId);
            if (msgs != null)
            {
                msgs.ForEach(msg =>
                {
                    _context.Messages.Remove(msg);
                });
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Permet de récupérer un message via son id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Message</returns>
        public Message GetMessage(int id)
        {
            var message = _context.Messages.FirstOrDefault(msg => msg.Id == id);

            return message;
        }

        /// <summary>
        /// Récupère la liste des messages d'une conversation
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        public List<Message> GetMessagesByConversation(int conversationId)
        {
            var messages = _context.Messages.Where(msg => msg.Conversation == conversationId).OrderBy(msg => msg.Date).ToList();

            return messages;
        }
    }
}
