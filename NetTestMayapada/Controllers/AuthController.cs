using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetTestMayapada.Models;
using NetTestMayapada.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace NetTestMayapada.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;

        public AuthController(SignInManager<Users> signInManager, UserManager<Users> userManager, ILogger<AuthController> logger)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _logger = logger;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email tidak ditemukan");
                    return View(model);
                }
                if (user.IsActive == false)
                {
                    ModelState.AddModelError("", "Akun sedang tidak aktif.");
                    return View(model);
                }
                var result = await signInManager.PasswordSignInAsync(user, model.Password, isPersistent: true, lockoutOnFailure: false);

                if (result.Succeeded)
                {

                    // Ambil principal yang sudah dibuat oleh SignInManager
                    var principal = await signInManager.CreateUserPrincipalAsync(user);
                    var identity = (ClaimsIdentity)principal.Identity!;

                    // Tambahkan claim custom
                    identity.AddClaim(new Claim("FullName", user.FullName ?? ""));
                    identity.AddClaim(new Claim("Level", user.Level ?? ""));
                    identity.AddClaim(new Claim("Email", user.Email ?? ""));
                    identity.AddClaim(new Claim("IDUser", user.Id ?? ""));
                    identity.AddClaim(new Claim("PhotoProfile", user.PhotoProfile ?? "/images/default-profile.png"));

                    // Replace principal di cookie
                    await HttpContext.SignInAsync("Identity.Application", principal);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Email or password is incorrect.");
                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Users users = new Users
                {
                    FullName = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                    PhotoProfile = null,
                    Level = "",
                    IsActive = true
                };

                var result = await userManager.CreateAsync(users, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }
            }
            return View(model);
        }

        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Something is wrong!");
                    return View(model);
                }
                else
                {
                    return RedirectToAction("ChangePassword", "Auth", new { username = user.UserName });
                }
            }
            return View(model);
        }

        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("VerifyEmail", "Auth");
            }
            return View(new ChangePasswordViewModel { Email = username });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                    var result = await userManager.RemovePasswordAsync(user);
                    if (result.Succeeded)
                    {
                        result = await userManager.AddPasswordAsync(user, model.NewPassword);
                        return RedirectToAction("Login", "Auth");
                    }
                    else
                    {

                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }

                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Email not found!");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Something went wrong. try again.");
                return View(model);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }
    }
}
