
using System.Threading.Tasks;
using BitcoinClientApp.Models;

namespace BitcoinClientApp.Interfaces
{
    public interface IAuthService
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> CreateUserAsync(string username, string password);
        Task<bool> ValidateUserAsync(string username, string password);
    }
}