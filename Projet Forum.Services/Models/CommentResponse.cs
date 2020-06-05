using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_Forum.Services.Models
{
    public class CommentResponse
    {
        public int Id { get; set; }

        public int Post { get; set; }

        public ProfileResponse AuthorProfile { get; set; }

        public string Body { get; set; }

        public string Author { get; set; }
    }
}
