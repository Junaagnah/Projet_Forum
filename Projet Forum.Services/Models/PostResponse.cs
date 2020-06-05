using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_Forum.Services.Models
{
    public class PostResponse
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public bool Locked { get; set; }

        public string Image { get; set; }

        public ProfileResponse AuthorProfile { get; set; }

        public List<CommentResponse> Comments { get; set; }
    }
}
