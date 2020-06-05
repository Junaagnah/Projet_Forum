using System.Threading.Tasks;
using Projet_Forum.Services.Models;
using Projet_Forum.Services.ViewModels;
using Projet_Forum.Data.Models;

namespace Projet_Forum.Services.Interfaces
{
    public interface IIdentityService
    {
        public Task<MyResponse> SignInUserAsync(string email, string password);

        public Task<MyResponse> RegisterUserAsync(RegisterViewModel user);

        public Task<MyResponse> ConfirmEmail(string userId, string token);

        public Task<MyResponse> AskPasswordRecovery(string email);

        public Task<MyResponse> RecoverPassword(string userId, string password, string token);

        public Task<MyResponse> RenewToken(string username, string refreshToken);

        public Task Disconnect(string username);

        public Task<User> GetUserByUsername(string username);

        public Task<User> GetUserById(string userId);

        public Task<MyResponse> GetUserProfileByUsername(string username);

        public Task<MyResponse> UpdateUserPassword(string username, string oldPassword, string newPassword);

        public Task<MyResponse> UpdateUserProfile(string username, UpdateProfileViewModel data, string requestingUser = "");

        public Task<MyResponse> DeleteUserAccount(string username, string password);

        public Task<MyResponse> SearchUserByUsername(string username, string currentUsername);

        public Task<MyResponse> UpdateUserBan(string usernameToBan, string actualUser, bool isBanned);

        public MyResponse GetUsersWithPagination(int page, int role, string search = "", int filter = 0);
    }
}
