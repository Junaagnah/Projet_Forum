using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projet_Forum.Services.Models
{
    public class ProfileResponse
    {
        public string Username { get; set; }

        public string ProfilePicture { get; set; }

        public int Role { get; set; }

        public string RoleName { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }

        public bool IsBanned { get; set; }
    }
}
