namespace Afrodite.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountControllerWebApi : ControllerBase
    {
        private readonly UserManager<Users> userManager;
        private readonly SignInManager<Users> signInManager;

        public AccountControllerWebApi(UserManager<Users> userManager, SignInManager<Users> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        [HttpGet("register")]
        public IActionResult GetRegister()
        {
            return Ok(); 
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (ModelState.IsValid)
            {
                var user = new Users
                {
                    UserName = model.Email,
                    Email = model.Email,
                };

                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return Ok(new { message = "Registration successful" }); 
                }

                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { errors }); 
            }

            return BadRequest(ModelState); 
        }
        [HttpGet("is-email-available")]
        public async Task<IActionResult> IsEmailAvailable([FromQuery] string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Ok(new { available = true });
            }
            return Conflict(new { message = $"Email {email} is already in use." }); 
        }
        [HttpGet("login")]
        public IActionResult GetLogin()
        {
            return Ok(); 
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return Ok(new { message = "Login successful" }); 
                }
                return Unauthorized(new { message = "Invalid login attempt." }); 
            }
            return BadRequest(ModelState); 
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok(new { message = "Logout successful" }); 
        }
        [HttpGet("access-denied")]
        public IActionResult AccessDenied()
        {
            return Forbid(); 
        }
    }
}