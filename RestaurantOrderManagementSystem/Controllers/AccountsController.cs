using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using RestaurantOrderManagementSystem.Accounts;
using RestaurantOrderManagementSystem.Data;
using RestaurantOrderManagementSystem.Models.Menu;

namespace RestaurantOrderManagementSystem.Controllers
{
    [Authorize(Policy = PolicyTypes.Users.Admin)]
    public class AccountsController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager; 
        
        public AccountsController(RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        //In case someone was dumb and deleted the admin user. Here is a fixer url
        [HttpGet][AllowAnonymous] public async Task<IActionResult> FixAdmin()
        {
            ApplicationUser user = await _userManager.FindByIdAsync(Guid.Empty.ToString());
            if(user == null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.Empty.ToString(),
                    FirstName = "Admin",
                    LastName = "John Doe",
                    UserName = "admin",
                    IsAdmin = true
                };
                await _userManager.CreateAsync(user, "Password@123");
            }

            IdentityRole role = await _roleManager.FindByIdAsync(Guid.Empty.ToString());
            if(role == null){
                role = new IdentityRole
                {
                    Id = Guid.Empty.ToString(),
                    Name = "Admin"
                };
                await _roleManager.CreateAsync(role);
            }

            await _userManager.AddToRoleAsync(user, role.Name);
            SetRoleClaims(new RoleViewModel {Role = role, IsAdmin = true}, true);
            
            return BadRequest();
        }
        
        #region Accounts
        [HttpGet] public IActionResult UserDashboard() => View("UserDashboard", _userManager.Users.ToList());
        [HttpGet] public IActionResult ViewAccountDetails() => RedirectToAction("Index", "Menu");
        [HttpGet] public IActionResult CreateUser() => View("User/Create", new UserViewModel {Roles = _roleManager.Roles.Select(x=> x.Name).ToList()});
        [HttpGet] public async Task<IActionResult> EditUser(string id)
        {
            if (id == null) return BadRequest();

            ApplicationUser user = await _userManager.FindByIdAsync(id);
            List<string> accountRoles = (List<string>) await _userManager.GetRolesAsync(user);
            List<string> roles = _roleManager.Roles.Select(x=> x.Name).ToList();
            
            return View("User/Edit", new UserViewModel
                {
                    User = user,
                    AccountRoles = accountRoles,
                    Roles = roles
                });
        }

        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> CreateUser(UserViewModel model)
        {
            var user = new ApplicationUser { UserName = model.User.UserName };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                model.User = user;
                AssignRoles(model);             
                return RedirectToAction("UserDashboard", "Accounts");
            }
            else
                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError("", error.Description);

            return View("User/Create", model);
        }
        
        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> UpdateUser(string id, UserViewModel model)
        {
            if (id == null) return BadRequest();
            var user = await _userManager.FindByIdAsync(id);
            {
                user.FirstName = model.User.FirstName;
                user.MiddleNames = model.User.MiddleNames;
                user.LastName = model.User.LastName;
                user.DOB = model.User.DOB;
                user.IDNumber = model.User.IDNumber;
                user.PhoneNumber = model.User.PhoneNumber;
                user.UserName = model.User.UserName;
            }
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                model.User = user;
                AssignRoles(model, true);
                return RedirectToAction("UserDashboard", "Accounts");
            }
            else
                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError("", error.Description);

            return View("User/Edit", model);
        }

        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> DeleteUser(string id)
        {
            IdentityResult result = await _userManager.DeleteAsync(await _userManager.FindByIdAsync(id));

            if (!result.Succeeded)
                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            
            return RedirectToAction("UserDashboard", "Accounts");
        }
        
        #endregion

        #region Roles

        [HttpGet] public IActionResult RoleDashboard() => View("RoleDashboard", _roleManager.Roles.ToList());
        [HttpGet] public IActionResult CreateRole() => View("Role/Create");
        [HttpGet] public async Task<IActionResult> EditRole(string id)
        {
            if (id == null) return BadRequest();

            bool isAdmin = false, isWaitstaff = false, isChef = false, isManager = false;
            
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            
            
            foreach (Claim claim in await _roleManager.GetClaimsAsync(role))
            {
                switch (claim.Type)
                {
                    case CustomClaimTypes.Permission.Admin:
                        isAdmin = bool.Parse(claim.Value);
                        break;
                    case CustomClaimTypes.Permission.WaitStaff:
                        isWaitstaff = bool.Parse(claim.Value);
                        break;
                    case CustomClaimTypes.Permission.Chef:
                        isChef = bool.Parse(claim.Value);
                        break;
                    case CustomClaimTypes.Permission.Manager:
                        isManager = bool.Parse(claim.Value);
                        break;
                    default:
                        break;
                }
            }
            
            return View("Role/Edit", new RoleViewModel
            {
                Role = _roleManager.FindByIdAsync(id).Result,
                IsAdmin = isAdmin,
                IsWaitStaff = isWaitstaff,
                IsChef = isChef,
                IsManager = isManager,
            });
        }

        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> CreateRole(RoleViewModel model)
        {
            IdentityResult result = await _roleManager.CreateAsync(model.Role);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors) ModelState.AddModelError("", error.Description);
                return View("Role/Create", model);
            }

            SetRoleClaims(model);
            
            return RedirectToAction("RoleDashboard", "Accounts");
        }

        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> UpdateRole(string id, RoleViewModel model)
        {
            if (id == null) return BadRequest();

            IdentityRole role = await _roleManager.FindByIdAsync(id);
            {
                role.Name = model.Role.Name;
            }
            
            IdentityResult result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors) ModelState.AddModelError("", error.Description);
                return View("Role/Create", model);
            }

            model.Role = role;
            SetRoleClaims(model, true);

            return RedirectToAction("RoleDashboard", "Accounts");
        }

        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> DeleteRole(string id)
        {
            if (id == null) return BadRequest();

            IdentityResult result = await _roleManager.DeleteAsync(await _roleManager.FindByIdAsync(id));

            if (!result.Succeeded)
                foreach (IdentityError error in result.Errors) ModelState.AddModelError("", error.Description);

            return RedirectToAction("RoleDashboard", "Accounts");
        }

        #endregion

        #region Login
        
        [HttpGet] [AllowAnonymous] public IActionResult Login()
        {
            return View(new LoginViewModel {RememberMe = true});
        } 
        
        [HttpPost] [AllowAnonymous] public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost] [AllowAnonymous] public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(viewModel.Username, viewModel.Password, viewModel.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("","Invalid login attempt.");
            return View(viewModel);
        }
        
        #endregion

        #region Utils

        public async void SetRoleClaims(RoleViewModel model, bool clearExisting = false)
        {
            if (clearExisting) foreach (Claim claim in await _roleManager.GetClaimsAsync(model.Role))
                    await _roleManager.RemoveClaimAsync(model.Role, claim);
            
            await _roleManager.AddClaimAsync(model.Role, new Claim(CustomClaimTypes.Permission.Admin, model.IsAdmin.ToString()));
            await _roleManager.AddClaimAsync(model.Role, new Claim(CustomClaimTypes.Permission.WaitStaff, model.IsWaitStaff.ToString()));
            await _roleManager.AddClaimAsync(model.Role, new Claim(CustomClaimTypes.Permission.Chef, model.IsChef.ToString()));
            await _roleManager.AddClaimAsync(model.Role, new Claim(CustomClaimTypes.Permission.Manager, model.IsManager.ToString()));
        }

        public async void AssignRoles(UserViewModel model, bool clearExisting = false)
        {
            if (clearExisting)
                await _userManager.RemoveFromRolesAsync(model.User, await _userManager.GetRolesAsync(model.User));
            await _userManager.AddToRolesAsync(model.User, model.AccountRoles);
        }

        #endregion
        
    }

    public static class CustomClaimTypes
    {
        public static class Permission
        {
            public const string Admin = "Application.Permission.Admin";
            public const string WaitStaff = "Application.Permission.WaitStaff";
            public const string Chef = "Application.Permission.Chef";
            public const string Manager = "Application.Permission.Manager";
        }
    }
    
    public class UserViewModel
    {
        public ApplicationUser User { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        public List<string> AccountRoles { get; set; } = new List<string>();
        public List<string> Roles { get; set; }
    }

    public class RoleViewModel
    {
        public IdentityRole Role { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsWaitStaff { get; set; }
        public bool IsChef { get; set; }
        public bool IsManager { get; set; }
    }

    public class LoginViewModel
    {
        [Required, MaxLength(256)]
        public string Username { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; } = true;
    }

    public static class PolicyTypes
    {
        public static class Users
        {
            public const string Admin = "user.admin.policy";
            public const string Waitstaff = "user.waitstaff.policy";
            public const string Chef = "user.cheff.policy";
            public const string Manager = "user.manager.policy";
        }
    }
}