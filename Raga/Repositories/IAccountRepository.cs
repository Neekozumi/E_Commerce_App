using Afrodite.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Afrodite.Repositories
{
    public interface IAccountRepository
    {
        Task<IdentityResult> RegisterUserAsync(Users user, string password);
        Task<Users> FindByEmailAsync(string email);
        Task<Microsoft.AspNetCore.Identity.SignInResult> LoginUserAsync(string email, string password, bool rememberMe);
        Task LogoutUserAsync();
    }
}
