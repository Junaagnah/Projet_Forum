using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_Forum.Services.Models
{
    public class UserPaginationResponse
    {
        public int Count { get; set; }

        public List<UserResponse> Users { get; set; }
    }
}
