using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Projet_Forum.Services.Models;
using Projet_Forum.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace Projet_Forum.Services.Hubs
{
    /// <summary>
    /// Hub permettant de gérer les fonctionnalités du chat
    /// </summary>
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ChatHub : Hub
    {
        private readonly IIdentityService _identityService;
        private readonly IMemoryCache _cache;
        private const string MESSAGES = "messages";

        public ChatHub(IIdentityService identityService, IMemoryCache cache)
        {
            _identityService = identityService;
            _cache = cache;
        }

        /// <summary>
        /// Permet de récupérer les messages enregistrés dans le cache
        /// </summary>
        /// <returns>List<MessageModel></returns>
        public async Task GetMessages()
        {
            // On vérifie que la liste des messages existe bien
            if (!_cache.TryGetValue(MESSAGES, out List<MessageModel> messages))
                _cache.Set(MESSAGES, new List<MessageModel>());

            await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessages", (List<MessageModel>)_cache.Get(MESSAGES));
        }

        /// <summary>
        /// Permet d'envoyer un message sur le chat
        /// </summary>
        /// <param name="msg"></param>
        public async Task SendMessage(MessageModel msg)
        {
            var user = await _identityService.GetUserByUsername(Context.UserIdentifier);
            if (user == null || user.IsBanned) return; // Si l'utilisateur n'existe pas ou qu'il est banni, on n'envoie rien

            List<MessageModel> messages = (List<MessageModel>)_cache.Get(MESSAGES);

            // Si on est à 100 messages, on supprime le premier de la liste
            if (messages.Count == 100)
                messages.RemoveAt(0);

            messages.Add(msg);

            _cache.Set(MESSAGES, messages);

            msg.UserRole = user.Role;
            msg.Username = user.UserName;

            await Clients.All.SendAsync("MessageReceived", msg);
        }

        /// <summary>
        /// Permet de supprimer un message dans le chat
        /// </summary>
        /// <param name="msg"></param>
        public async Task DeleteMessage(MessageModel msg)
        {
            var user = await _identityService.GetUserByUsername(Context.UserIdentifier);
            if (user == null) return;

            if (msg.Username == user.UserName || user.Role >= 2)
            {
                // On met également à jour le message dans le cache
                var messages = (List<MessageModel>)_cache.Get(MESSAGES);
                foreach(var message in messages)
                {
                    if (message.Message == msg.Message && message.Date == msg.Date && message.Username == msg.Username)
                    {
                        message.Message = "Ce message a été supprimé";
                    }
                }

                _cache.Set(MESSAGES, messages);

                await Clients.All.SendAsync("DeleteMessage", msg);
            }
        }
    }
}
