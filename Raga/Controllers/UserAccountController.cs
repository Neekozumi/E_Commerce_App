
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Afrodite.Controllers
{
    public class UserAccountController : Controller
    {
        private readonly UserManager<Users> _userManager;
        public UserAccountController(UserManager<Users> userManager)
        {
            _userManager = userManager;
        }
        
        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }
    }
}