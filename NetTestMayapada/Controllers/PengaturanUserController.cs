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
        private readonly UserManager<Users> userManager;
        private readonly ILogger<PengaturanUserController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _db;

        public PengaturanUserController(UserManager<Users> userManager, ILogger<PengaturanUserController> logger, IWebHostEnvironment env, AppDbContext db)
        {
            _logger = logger;
            _env = env;
            _db = db;
            this.userManager = userManager;
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
            try
            {
                var list = await _db.Users
                .AsNoTracking()
                .OrderBy(u => u.UserNumber)
                .Select(u => new
                {
                    u.UserNumber,
                    u.FullName,
                    u.Email,
                    u.Level,
                    u.PhotoProfile,
                    u.IsActive
                })
                .ToListAsync();

                var users = list.Select((u, i) => new LoadTableUserViewModel
                {
                    UserNumber = u.UserNumber,
                    number = i + 1,
                    FullName = u.FullName,
                    Email = u.Email,
                    Level = u.Level,
                    PhotoProfile = u.PhotoProfile,
                    IsActive = u.IsActive
                }).ToList();

                return Json(new { data = users });
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("Create");
        }

        [HttpPost]
        public async Task<IActionResult> SubmitCreate(CreateUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ToastrError"] = "Harap isi semua field!";
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.AddModelError(nameof(model.Password), "Password dan konfirmasi harus sama.");
                    return View(model);
                }

                string? photoRelativePath = null;
                if (model.PhotoProfileFile != null && model.PhotoProfileFile.Length > 0)
                {
                    var uploadFolder = Path.Combine(_env.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadFolder);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.PhotoProfileFile.FileName)}";
                    var filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await model.PhotoProfileFile.CopyToAsync(stream);
                    }

                    photoRelativePath = $"/uploads/{fileName}".Replace("\\", "/");
                }

                Users user = new Users
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Level = model.Level,
                    IsActive = model.IsActive,
                    PhotoProfile = model.PhotoProfileFile.FileName ?? "/images/default-profile.png"
                };

                var result = await userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var err in result.Errors) ModelState.AddModelError("", err.Description);
                    return View(model);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return NotFound();

            var vm = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Level = user.Level,
                IsActive = user.IsActive
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitEdit(EditUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            _db.Attach(user);
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Level = model.Level;
            user.IsActive = model.IsActive;

            if (model.PhotoProfileFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.PhotoProfileFile.FileName);
                var path = Path.Combine("wwwroot/uploads/", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await model.PhotoProfileFile.CopyToAsync(stream);
                user.PhotoProfile = "/uploads/" + fileName;
            }

            var entry = _db.Entry(user);
            entry.Property(x => x.FullName).IsModified = true;
            entry.Property(x => x.Level).IsModified = true;
            entry.Property(x => x.IsActive).IsModified = true;
            entry.Property(x => x.PhotoProfile).IsModified = true;

            entry.Property(x => x.UserNumber).IsModified = false;

            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string email)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null) return NotFound();

                var result = await userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    TempData["ToastrError"] = "Data gagal dihapus";
                    return RedirectToAction("Delete");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ToastrError"] = "Data gagal dihapus";
                return RedirectToAction("Index");
            }
        }
    }
}
