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
    public class PengaturanUserController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<PengaturanUserController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _db;

        public PengaturanUserController(ILogger<PengaturanUserController> logger, IWebHostEnvironment env, AppDbContext db)
        {
            _logger = logger;
            _env = env;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            if (User == null) return RedirectToAction("Login", "Auth");

            var idUser = User.FindFirstValue("IDUser") ?? "";
            if (string.IsNullOrEmpty(idUser))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _db.Users.Where(x => x.Id == idUser).FirstOrDefaultAsync();
            if (user.Level == "Administrator")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        [HttpGet]
        public async Task<IActionResult> LoadData()
        {
            var users = await _db.Users
                .Select((u, index) => new LoadTableUserViewModel
                {
                    UserNumber = u.UserNumber,
                    number = index + 1,
                    FullName = u.FullName,
                    Email = u.Email,
                    Level = u.Level,
                    PhotoProfile = u.PhotoProfile,
                    isActive = u.IsActive
                })
                .ToListAsync();

            return Json(new { data = users });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserViewModel model, IFormFile? PhotoProfileFile)
        {
            if (!ModelState.IsValid) return View(model);

            string photoPath = null;
            if (PhotoProfileFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(PhotoProfileFile.FileName);
                var path = Path.Combine("wwwroot/images/", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await PhotoProfileFile.CopyToAsync(stream);
                photoPath = "/images/" + fileName;
            }

            var user = new Users
            {
                FullName = model.FullName,
                Email = model.Email,
                Level = model.Level,
                PhotoProfile = photoPath,
                IsActive = model.isActive
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            var vm = new UserViewModel
            {
                UserNumber = user.UserNumber,
                FullName = user.FullName,
                Email = user.Email,
                Level = user.Level,
                PhotoProfile = user.PhotoProfile,
                isActive = user.IsActive
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserViewModel model, IFormFile? PhotoProfileFile)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _db.Users.FindAsync(model.UserNumber);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Level = model.Level;
            user.IsActive = model.isActive;

            if (PhotoProfileFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(PhotoProfileFile.FileName);
                var path = Path.Combine("wwwroot/images/", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await PhotoProfileFile.CopyToAsync(stream);
                user.PhotoProfile = "/images/" + fileName;
            }

            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            var vm = new UserViewModel
            {
                UserNumber = user.UserNumber,
                FullName = user.FullName,
                Email = user.Email
            };
            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
