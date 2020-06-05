using Projet_Forum.Data.Models;
using Projet_Forum.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Projet_Forum.Services.Interfaces
{
    public interface INotificationService
    {
        public Task<bool> CreateNotification(Notification notification);

        public Task<MyResponse> MarkAsReadByUsername(string username);

        public Task<MyResponse> DeleteNotification(int notifId);

        public Task<MyResponse> GetNotificationsByUsername(string username);
    }
}
