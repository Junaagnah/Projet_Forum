namespace Projet_Forum.Services.ViewModels
{
    public class UpdateProfileViewModel
    {
        public string Username { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }

        public string ProfilePicture { get; set; }

        public bool IsBanned { get; set; }

        public int Role { get; set; }
    }
}
