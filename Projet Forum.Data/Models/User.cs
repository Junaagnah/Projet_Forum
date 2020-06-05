using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Projet_Forum.Data.Models
{
    public class User : IdentityUser
    {
        [Required]
        public bool IsBanned { get; set; } = false;

        [Required]
        public int Role { get; set; } = 1;

        public string Description { get; set; } = "";
    }
}
