using MediatR;

namespace Afrodite.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<Users> _userManager;
        private readonly IMediator _mediator;

        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<Users> userManager, IMediator mediator)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _mediator = mediator;
        }

        [Authorize( Roles = "AdminTổng" )]
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [Authorize( Roles = "AdminTổng" )]
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleDto roleModel)
        {
            if (ModelState.IsValid)
            {
                bool roleExists = await _roleManager.RoleExistsAsync(roleModel?.RoleName);
                if (roleExists)
                {
                    ModelState.AddModelError("", "Vai trò đã tồn tại");
                }
                else
                {
                    IdentityRole identityRole = new IdentityRole
                    {
                        Name = roleModel?.RoleName
                    };

                    IdentityResult result = await _roleManager.CreateAsync(identityRole);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(roleModel);
        }

        [Authorize( Roles = "AdminTổng" )]
        [HttpGet]
        public async Task<IActionResult> ListRoles()
        {
            List<IdentityRole> roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

//________________________________________________________________________
//
        [Authorize( Roles = "AdminTổng" )]
        [HttpGet]
        public async Task<IActionResult> EditRole(string roleId)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return View("Error");
            }

            var model = new EditRoleDto
            {
                Id = role.Id,
                RoleName = role.Name,
                Users = new List<string>()
            };
            
            foreach (var user in _userManager.Users.ToList())
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }
            return View(model);
        }
//________________________________________________________________________________________
        [Authorize( Roles = "AdminTổng" )]
        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleDto model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    ViewBag.ErrorMessage = $"Role với ID = {model.Id} không tìm thấy ";
                    return View("NotFound");
                }
                else
                {
                    role.Name = model.RoleName;

                    var result = await _roleManager.UpdateAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListRoles");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }
            return BadRequest("An error occurred while updating.");
        }

        [Authorize( Roles = "AdminTổng" )]
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role với ID = {roleId} không tìm thấy ";
                return View("NotFound");
            }

            if (role.Name == "AdminTổng")
            {
                ModelState.AddModelError("", "Cannot delete the AdminTổng role.");
                return View("ListRoles", await _roleManager.Roles.ToListAsync());
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return RedirectToAction("ListRoles");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View("ListRoles", await _roleManager.Roles.ToListAsync());
        }

        [HttpGet]
        [Authorize(Roles = "AdminTổng")]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role với ID = {roleId} không tìm thấy ";
                return View("NotFound");
            }

            ViewBag.RollName = role.Name;
            var model = new List<UserRoleDto>();

            foreach (var user in _userManager.Users.ToList())
            {
                var userRoleDtos = new UserRoleDto
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                };

                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleDtos.IsSelected = true;
                }
                else
                {
                    userRoleDtos.IsSelected = false;
                }
                model.Add(userRoleDtos);
            }
            return View(model);
        }

        [HttpPost]
        [Authorize( Roles = "AdminTổng" )]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleDto> model, string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role với Id là {roleId} không thể tìm thấy!";
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await _userManager.FindByIdAsync(model[i].UserId);
                IdentityResult? result ; 
                if (model[i].IsSelected &&! (await _userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await _userManager.AddToRoleAsync (user, role.Name);
                }
                else if (!model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else 
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                    {
                        continue;
                    }
                    else 
                    return RedirectToAction("EditRole",new {roleId = roleId});
                }
            }
            return BadRequest("An error occurred while updating user roles.");
        }

        [HttpGet]
        [Authorize( Roles = "AdminTổng" )]
        public async Task<IActionResult> ListUsers()
        {
            var users = await _mediator.Send(new ListUsersQuery());
            return View(users);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditUser(string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User với Id là {UserId} không thể tìm thấy!";
                return View("Not Found");
            } 
            
                var userClaims = await _userManager.GetClaimsAsync(user);
                var userRoles = await _userManager.GetRolesAsync(user);
                var model = new EditUserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Address = user.Address,
                    Claims = userClaims.Select(c => c.Value).ToList(),
                    Roles = userRoles
                };
                return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditUser(EditUserDto model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                user.FullName = model.FullName;
                user.Email =model.Email;
                user.Address = model.Address;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteUser(string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }

            if (await _userManager.IsInRoleAsync(user, "AdminTổng"))
            {
                ModelState.AddModelError("", "Cannot delete the AdminTổng user.");
                return View("ListUsers");
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("ListUsers");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View("ListUsers");
        }

        [HttpGet]
        [Authorize( Roles = "AdminTổng" )]
        public async Task<IActionResult> ManageUserRoles(string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User với Id = {UserId} không tìm thấy";
                return View("NotFound");
            }
            ViewBag.UserId = UserId;
            ViewBag.UserName = user.UserName;
            var model = new List<EditUserRolesDto>();
            foreach (var role in await _roleManager.Roles.ToListAsync())
            {
                var userEditUserRolesDto = new EditUserRolesDto
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, "AdminTổng"))
                {
                    userEditUserRolesDto.IsSelected = true;
                }
                else 
                {
                    userEditUserRolesDto.IsSelected = false;
                }
                model.Add(userEditUserRolesDto);
            }
            return View(model);
        }
        [HttpPost]
        [Authorize( Roles = "AdminTổng")]
        public async Task<IActionResult> ManageUserRoles(List<EditUserRolesDto> model, string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }
            List<string> RolesToBeAssigned = model.Where(x => x.IsSelected).Select(y => y.RoleName).ToList();
            if (RolesToBeAssigned.Any())
            {
                result = await _userManager.AddToRolesAsync(user, RolesToBeAssigned);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Cannot Add Selected Roles to User");
                    return View(model);
                }
            }
            return RedirectToAction("EditUser", new { UserId = UserId });
        }

        [HttpGet]
        [Authorize (Roles = "AdminTổng")]
        [Authorize (Policy = "")]
        public async Task<IActionResult> ManageUserClaims (string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {UserId} cannot be found";
                return View("NotFound");
            }
            ViewBag.UserName = user.UserName;
            var model = new UserClaimsStoreDto
            {
                UserId = UserId
            };
            var existingUserClaims = await _userManager.GetClaimsAsync(user);
            foreach (Claim claim in ClaimStore.GetAllClaims())
            {
                UserClaimDto userClaim = new UserClaimDto
                {
                    ClaimType = claim.Type
                };
                if (existingUserClaims.Any(c => c.Type == claim.Type))
                {
                    userClaim.IsSelected = true;
                }
                model.Claims.Add(userClaim);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsStoreDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.UserId} cannot be found";
                return View("NotFound");
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var result = await _userManager.RemoveClaimsAsync(user, claims);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing claims");
                return View(model);
            }

            var AllSelectedClaims = model.Claims.Where(c => c.IsSelected)
                        .Select(c => new Claim(c.ClaimType, c.ClaimType))
                        .ToList();

            if (AllSelectedClaims.Any())
            {
                result = await _userManager.AddClaimsAsync(user, AllSelectedClaims);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Cannot add selected claims to user");
                    return View(model);
                }
            }

            return RedirectToAction("EditUser", new { UserId = model.UserId });
        }
    }
}
