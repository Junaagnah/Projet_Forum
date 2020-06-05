using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;

namespace Projet_Forum.Data.Repositories
{
    /// <summary>
    /// Gère le CRUD pour les notifications
    /// </summary>
    public class NotificationRepository : INotificationRepository
    {
        private readonly MyDbContext _context;

        public NotificationRepository(MyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Permet de créer une notification pour l'utilisateur
        /// </summary>
        /// <param name="notification"></param>
        public async Task CreateNotification(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Permet de supprimer une notification
        /// </summary>
        /// <param name="notifId"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> DeleteNotification(int notifId)
        {
            var notif = _context.Notifications.FirstOrDefault(notif => notif.Id == notifId);
            if (notif != null)
            {
                _context.Notifications.Remove(notif);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Permet de récupérer les notifications d'un utilisateur
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>List<Notification></returns>
        public List<Notification> GetNotificationsByUserId(string userId)
        {
            var notifs = _context.Notifications.Where(notif => notif.UserId == userId).OrderByDescending(notif => notif.Date).ToList();

            return notifs;
        }

        /// <summary>
        /// Permet de marquer les notifications d'un utilisateur comme lues
        /// </summary>
        /// <param name="notifId"></param>
        /// <returns>Boolean</returns>
        public async Task MarkAsReadByUserId(string userId)
        {
            List<Notification> notif = _context.Notifications.Where(notif => notif.UserId == userId).ToList();

            if (notif.Count > 0)
            {
                notif.ForEach(notif =>
                {
                    notif.Read = true;
                });

                await _context.SaveChangesAsync();
            }
        }
    }
}
