using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Services.Models;
using Projet_Forum.Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Forum.Services.Services
{
    /// <summary>
    /// Service permettant de gérer les actions sur les messages
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _repository;
        private readonly IConversationRepository _convRepository;
        private readonly IIdentityService _identityService;
        private readonly INotificationService _notifService;

        private const string defaultProfilePicture = "images/defaultProfilePicture.jpg";

        public MessageService(IMessageRepository repository, IConversationRepository convRepository, IIdentityService identityService, INotificationService notifService)
        {
            _repository = repository;
            _convRepository = convRepository;
            _identityService = identityService;
            _notifService = notifService;
        }

        /// <summary>
        /// Permet de créer un message en base de données
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> CreateMessage(MessageViewModel msg, string username)
        {
            if (msg != null && !String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(msg.Content) && !String.IsNullOrWhiteSpace(msg.ReceiverId))
            {
                // On récupère l'utilisateur courant via son nom d'utilisateur dans le token
                var user = await _identityService.GetUserByUsername(username);

                // On pense également à récupérer l'utilisateur à qui l'on veut envoyer le message pour être sûr qu'il existe
                var receiver = await _identityService.GetUserById(msg.ReceiverId);
                if (user != null && receiver != null)
                {
                    // On vérifie si une conversation n'existe pas déjà avec les deux utilisateurs pour éviter de créer des doublons
                    var conv = _convRepository.GetConversationByUsers(user.Id, receiver.Id);
                    if (conv != null && msg.Conversation == 0) msg.Conversation = conv.Id;

                    // Si la conversation est égale à 0 c'est qu'on doit en créer une nouvelle
                    if (msg.Conversation == 0)
                    {
                        Conversation newConv = new Conversation
                        {
                            FirstUser = user.Id,
                            SecondUser = receiver.Id,
                            LastMessageDate = DateTime.Now
                        };

                        var createdConvId = await _convRepository.CreateConversation(newConv);

                        // On vérifie si la conversation a bien été créée
                        if (createdConvId != 0)
                        {
                            Message newMsg = new Message
                            {
                                Conversation = createdConvId,
                                Content = msg.Content,
                                Date = DateTime.Now,
                                Sender = user.Id
                            };

                            if (await _repository.CreateMessage(newMsg))
                            {
                                // Si le message a bien été créé, on envoie une notification au destinataire
                                Notification notif = new Notification
                                {
                                    Context = Notification.Type.Message,
                                    ContextId = createdConvId,
                                    Content = $"<strong>{user.UserName}</strong> vous a envoyé un message !",
                                    UserId = receiver.Id,
                                    Date = DateTime.Now
                                };

                                await _notifService.CreateNotification(notif);

                                // On ajoute l'id de la conversation dans la réponse pour pouvoir y rediriger l'utilisateur
                                var response = new MyResponse { Succeeded = true };
                                response.Result = createdConvId;

                                return response;
                            }

                            var creationError = new MyResponse { Succeeded = false };
                            creationError.Messages.Add("MessageNotCreated");

                            return creationError;
                        }

                        var error = new MyResponse { Succeeded = false };
                        error.Messages.Add("ConversationNotCreated");

                        return error;
                    }
                    // Sinon, c'est que la conversation existe déjà
                    else
                    {
                        // On vérifie que la conversation existe bien et que l'envoyeur et le receveur du message y sont bien renseignés
                        var existingConv = _convRepository.GetConversationByIdAndUsers(msg.Conversation, user.Id, msg.ReceiverId);

                        // Si la conv existe, on continue
                        if (existingConv != null)
                        {
                            Message newMsg = new Message
                            {
                                Conversation = msg.Conversation,
                                Content = msg.Content,
                                Date = DateTime.Now,
                                Sender = user.Id
                            };

                            if (await _repository.CreateMessage(newMsg))
                            {
                                // On met à jour la date du dernier message de la conversation pour la faire remonter en haut
                                await _convRepository.UpdateConversationDate(msg.Conversation);

                                // Si le message a bien été créé, on envoie une notification au destinataire
                                Notification notif = new Notification
                                {
                                    Context = Notification.Type.Message,
                                    ContextId = existingConv.Id,
                                    Content = $"<strong>{user.UserName}</strong> vous a envoyé un message !",
                                    UserId = receiver.Id,
                                    Date = DateTime.Now
                                };

                                await _notifService.CreateNotification(notif);

                                var response = new MyResponse { Succeeded = true };
                                response.Result = existingConv.Id;

                                return response;
                            }

                            var creationError = new MyResponse { Succeeded = false };
                            creationError.Messages.Add("MessageNotCreated");

                            return creationError;
                        }

                        var error = new MyResponse { Succeeded = false };
                        error.Messages.Add("ConversationNotFound");

                        return error;
                    }
                }
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de supprimer un message via son id
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> DeleteMessage(int msgId, string username)
        {
            if (!String.IsNullOrWhiteSpace(username))
            {
                var user = await _identityService.GetUserByUsername(username);
                if (user != null)
                {
                    // On récupère l'utilisateur qui veut supprimer le message et on vérifie qu'il en est bien l'auteur
                    var msg = _repository.GetMessage(msgId);
                    if (msg != null)
                    {
                        if (user.Id == msg.Sender)
                        {
                            var result = await _repository.DeleteMessage(msgId);

                            return new MyResponse { Succeeded = result };
                        }

                        var userError = new MyResponse { Succeeded = false };
                        userError.Messages.Add("UserNotAuthorized");

                        return userError;
                    }

                    var error = new MyResponse { Succeeded = false };
                    error.Messages.Add("MessageNotFound");

                    return error;
                }
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de récupérer la liste des messages d'une conversations
        /// </summary>
        /// <param name="convId"></param>
        /// <returns></returns>
        public async Task<MyResponse> GetMessagesByConversation(int convId, string username)
        {
            if (!String.IsNullOrWhiteSpace(username))
            {
                var conversation = _convRepository.GetConversationById(convId);
                if (conversation != null)
                {
                    var user = await _identityService.GetUserByUsername(username);

                    // On vérifie que l'utilisateur existe et qu'il participe bien à la conversation
                    if (user != null && (conversation.FirstUser == user.Id || conversation.SecondUser == user.Id))
                    {
                        var messages = _repository.GetMessagesByConversation(convId);
                        if (messages != null && messages.Count > 0)
                        {
                            List<MessageResponse> messageResponses = new List<MessageResponse>();

                            foreach (var msg in messages)
                            {
                                // Récupération du profil de l'envoyeur
                                var sender = await _identityService.GetUserById(msg.Sender);
                                var receiverId = (conversation.FirstUser == msg.Sender ? conversation.SecondUser : conversation.FirstUser);
                                ProfileResponse senderProfile;

                                if (sender != null)
                                {
                                    var profileResponse = await _identityService.GetUserProfileByUsername(sender.UserName);
                                    senderProfile = profileResponse.Result as ProfileResponse;
                                }
                                else
                                    senderProfile = new ProfileResponse { Username = "Utilisateur inexistant", ProfilePicture = defaultProfilePicture };

                                // Ajout du message à la liste de messageResponses
                                messageResponses.Add(new MessageResponse
                                {
                                    Id = msg.Id,
                                    Content = msg.Content,
                                    Conversation = msg.Conversation,
                                    Date = msg.Date,
                                    Sender = msg.Sender,
                                    SenderProfile = senderProfile,
                                    Receiver = receiverId
                                });
                            }

                            var response = new MyResponse { Succeeded = true };
                            response.Result = messageResponses;

                            return response;
                        }

                        var msgError = new MyResponse { Succeeded = false };
                        msgError.Messages.Add("NoMessageFound");

                        return msgError;
                    }

                    // Sinon, il n'est pas autorisé à visualiser la conversation
                    var userError = new MyResponse { Succeeded = false };
                    userError.Messages.Add("UserNotAuthorized");

                    return userError;
                }

                var error = new MyResponse { Succeeded = false };
                error.Messages.Add("ConversationNotFound");

                return error;
            }

            return new MyResponse { Succeeded = false };
        }
    }
}
