using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_Forum.Services.ViewModels
{
    public class CommentViewModel
    {
        public int Id { get; set; }

        public int Post { get; set; }

        public string Body { get; set; }

        public string Author { get; set; }
    }
}
