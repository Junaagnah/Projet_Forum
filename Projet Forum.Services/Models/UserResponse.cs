using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_Forum.Services.Models
{
    public class UserResponse
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public int Role { get; set; }
    }
}
