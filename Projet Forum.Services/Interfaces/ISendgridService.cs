using System.Threading.Tasks;
using Projet_Forum.Services.Models;

namespace Projet_Forum.Services.Interfaces
{
    public interface ISendgridService
    {
        public Task<bool> SendValidationEmail(string email, string userId, string token, bool isTest = false);

        public Task<bool> SendRecoveryEmail(string email, string userId, string token, bool isTest = false);

        public Task<MyResponse> SendContactEmail(string email, string message, bool isTest = false);
    }
}
