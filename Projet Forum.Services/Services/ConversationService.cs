using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Forum.Services.Services
{
    /// <summary>
    /// Service permettant de gérer les conversations
    /// </summary>
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository _repository;
        private readonly IIdentityService _identityService;

        public ConversationService(IConversationRepository repository, IIdentityService identityService)
        {
            _repository = repository;
            _identityService = identityService;
        }

        /// <summary>
        /// Permet de récupérer les conversations d'un utilisateur via son token
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<MyResponse> GetConversationsByUsername(string username)
        {
            if (!String.IsNullOrWhiteSpace(username))
            {
                var user = await _identityService.GetUserByUsername(username);
                if (user != null)
                {
                    List<ConversationResponse> conversations = new List<ConversationResponse>();
                    var convs = _repository.GetConversationsByUserId(user.Id);

                    // On met chaque conversation dans une liste de ConversationResponse pour pouvoir y ajouter le nom du contact avec qui l'utilisateur a une conversation
                    foreach (var conv in convs)
                    {
                        var contact = await _identityService.GetUserById((user.Id == conv.FirstUser ? conv.SecondUser : conv.FirstUser));
                        string contactUsername = (contact != null ? contact.UserName : "Utilisateur inexistant");

                        conversations.Add(new ConversationResponse
                        {
                            Id = conv.Id,
                            LastMessageDate = conv.LastMessageDate,
                            ContactUsername = contactUsername
                        });
                    }

                    var response = new MyResponse { Succeeded = true };
                    response.Result = conversations;

                    return response;

                }
            }

            return new MyResponse { Succeeded = false };
        }
    }
}
