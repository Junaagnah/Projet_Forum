using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;
using Projet_Forum.Services.Interfaces;
using Projet_Forum.Services.Models;

namespace Projet_Forum.Services.Services
{
    /// <summary>
    /// Service permettant de gérer les notifications
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IIdentityService _identityService;

        public NotificationService(INotificationRepository repository, IIdentityService identityService)
        {
            _repository = repository;
            _identityService = identityService;
        }

        /// <summary>
        /// Permet de créer une notification pour un utilisateur
        /// </summary>
        /// <param name="notification"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> CreateNotification(Notification notification)
        {
            if (notification != null && !String.IsNullOrWhiteSpace(notification.UserId) && !String.IsNullOrWhiteSpace(notification.Content))
            {
                await _repository.CreateNotification(notification);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Permet de supprimer une notification via son Id
        /// </summary>
        /// <param name="notifId"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> DeleteNotification(int notifId)
        {
            var result = await _repository.DeleteNotification(notifId);

            return new MyResponse { Succeeded = result };
        }

        /// <summary>
        /// Permet de récupérer les notifications d'un utilisateur via son token
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> GetNotificationsByUsername(string username)
        {
            if (!String.IsNullOrWhiteSpace(username))
            {
                var user = await _identityService.GetUserByUsername(username);
                if (user != null)
                {
                    var notifs = _repository.GetNotificationsByUserId(user.Id);

                    return new MyResponse { Succeeded = true, Result = notifs };
                }
            }

            return new MyResponse { Succeeded = false };
        }

        /// <summary>
        /// Permet de marquer les notifications de l'utilisateur courant comme lues
        /// </summary>
        /// <param name="notifId"></param>
        /// <returns>MyResponse</returns>
        public async Task<MyResponse> MarkAsReadByUsername(string username)
        {
            if (!String.IsNullOrWhiteSpace(username))
            {
                var user = await _identityService.GetUserByUsername(username);
                if (user != null)
                {
                    await _repository.MarkAsReadByUserId(user.Id);
                    return new MyResponse { Succeeded = true };
                }
            }

            return new MyResponse { Succeeded = false };
        }
    }
}
