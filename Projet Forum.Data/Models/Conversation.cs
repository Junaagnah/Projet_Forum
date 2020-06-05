using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Projet_Forum.Data.Models
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstUser { get; set; }

        [Required]
        public string SecondUser { get; set; }

        public DateTime LastMessageDate { get; set; } = DateTime.Now;
    }
}
