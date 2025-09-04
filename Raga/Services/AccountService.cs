using Afrodite.Models;
using Afrodite.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Afrodite.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterDto model)
        {
            var user = new Users
            {
                UserName = model.Email,
                Email = model.Email
            };
            return await _accountRepository.RegisterUserAsync(user, model.Password);
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            var user = await _accountRepository.FindByEmailAsync(email);
            return user == null;
        }

        public async Task<Microsoft.AspNetCore.Identity.SignInResult> LoginUserAsync(LoginDto model)
        {
            return await _accountRepository.LoginUserAsync(model.Email, model.Password, model.RememberMe);
        }

        public async Task LogoutUserAsync()
        {
            await _accountRepository.LogoutUserAsync();
        }
    }
}
