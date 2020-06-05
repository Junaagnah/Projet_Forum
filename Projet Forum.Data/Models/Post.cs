using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Projet_Forum.Data.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Category { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        [ForeignKey("User")]
        public string Author { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }

        public bool Locked { get; set; } = false;

        // Permet de renvoyer facilement le nom d'utilisateur de l'auteur
        [NotMapped]
        public string AuthorUsername { get; set; }
    }
}
