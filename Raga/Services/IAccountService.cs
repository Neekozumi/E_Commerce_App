using Afrodite.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Afrodite.Services
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterUserAsync(RegisterDto model);
        Task<bool> IsEmailAvailableAsync(string email);
        Task<Microsoft.AspNetCore.Identity.SignInResult> LoginUserAsync(LoginDto model);
        Task LogoutUserAsync();
    }
}
