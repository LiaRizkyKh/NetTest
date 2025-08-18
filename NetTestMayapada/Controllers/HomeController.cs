using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTestMayapada.Data;
using NetTestMayapada.Models;
using NetTestMayapada.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace NetTestMayapada.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _db;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env, AppDbContext db)
        {
            _logger = logger;
            _env = env;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            if (User == null) return RedirectToAction("Login", "Auth");

            var idUser = User.FindFirstValue("IDUser") ?? "";

            var user = await _db.Users.Where(x => x.Id == idUser).FirstOrDefaultAsync();
            var vm = new HomeViewModel
            {
                Name = user.FullName,
                Email = user.Email,
                Level = user.Level,
                PhotoProfile = user.PhotoProfile
            };

            return View(vm);
        }

        public async Task<IActionResult> Edit()
        {
            if (User == null) return RedirectToAction("Login", "Auth");

            var idUser = User.FindFirstValue("IDUser") ?? "";

            var user = await _db.Users.Where(x => x.Id == idUser).FirstOrDefaultAsync();
            var vm = new EditProfileViewModel
            {
                Name = user.FullName,
                Email = user.Email,
                Level = user.Level,
                PhotoProfile = user.PhotoProfile
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrEmpty(User.FindFirstValue("IDUser"))){
                return View(model);
            }
            var idUser = User.FindFirstValue("IDUser") ?? "";

            var user = await _db.Users.Where(x => x.Id == idUser).FirstOrDefaultAsync();
            if (user == null) return NotFound();

            user.FullName = model.Name;
            user.Level = model.Level;

            if (model.PhotoProfileFile != null && model.PhotoProfileFile.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{model.PhotoProfileFile.FileName}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.PhotoProfileFile.CopyToAsync(stream);
                }

                user.PhotoProfile = $"/uploads/{fileName}";
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
