using Projet_Forum.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Forum.Data.Interfaces
{
    public interface INotificationRepository
    {
        public Task CreateNotification(Notification notification);

        public Task MarkAsReadByUserId(string userId);

        public Task<bool> DeleteNotification(int notifId);

        public List<Notification> GetNotificationsByUserId(string userId);
    }
}
