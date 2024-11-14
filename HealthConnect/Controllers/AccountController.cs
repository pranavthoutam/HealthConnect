using HealthConnect.Models;
using HealthConnect.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthConnect.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {

                var user = new User
                {
                    UserName = userViewModel.UserName,
                    Email = userViewModel.Email,
                };

                var createUser = await _userManager.CreateAsync(user, userViewModel.Password);

                if (createUser.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("index", "home");
                }

                foreach (var error in createUser.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                //if (userViewModel.Role)
                //{
                //    var roleResult = await _roleManager.CreateAsync(user,"Doctor");
                //}
            }

            return View(userViewModel);
            
        }
    }

}