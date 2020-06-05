using Projet_Forum.Services.Models;

namespace Projet_Forum.Services.ViewModels
{
    public class PostViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public int Category { get; set; }

        public bool Locked { get; set; }

        public string Image { get; set; }
    }
}
