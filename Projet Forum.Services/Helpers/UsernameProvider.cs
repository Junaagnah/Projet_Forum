using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_Forum.Services.Helpers
{
    public class UsernameProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(claim => claim.Type == "username")?.Value;
        }
    }
}
