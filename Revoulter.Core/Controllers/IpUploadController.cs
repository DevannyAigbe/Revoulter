using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Revoulter.Core.Data;
using Revoulter.Core.Interfaces;
using Revoulter.Core.Models;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Revoulter.Core.Controllers
{
    [Authorize]
    public class IpUploadController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IArweaveUploader _arweaveUploader;
        private readonly IStoryProtocolRegistrar _storyRegistrar;
        private readonly IWebHostEnvironment _env;

        public IpUploadController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            IArweaveUploader arweaveUploader, IStoryProtocolRegistrar storyRegistrar, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _arweaveUploader = arweaveUploader;
            _storyRegistrar = storyRegistrar;
            _env = env;
        }

        public IActionResult Index()
        {
            ViewBag.Categories = new SelectList(Enum.GetValues(typeof(Category)));
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MyAssets()
        {
            // ✅ FIX: Use fallback methods to get user
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var userAssets = await _context.IpAssets
                .Where(ip => ip.OwnerId == user.Id)
                .OrderByDescending(ip => ip.CreatedAt)
                .ToListAsync();

            return View(userAssets);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var asset = await _context.IpAssets
                .Include(ip => ip.Owner)
                .FirstOrDefaultAsync(ip => ip.Id == id);

            if (asset == null)
            {
                return NotFound();
            }

            // ✅ FIX: Use fallback methods to get user
            var user = await GetCurrentUserAsync();
            if (user == null || asset.OwnerId != user.Id)
            {
                return Forbid();
            }

            return View(asset);
        }

       
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IpAsset model, IFormFile file)
        {
            //if (!ModelState.IsValid || file == null)
            //{
            //    ViewBag.Categories = new SelectList(Enum.GetValues(typeof(Category)));
            //    return View("Index", model);
            //}

            // 1️⃣ Try your custom/session-based method first
            var user = await GetCurrentUserAsync();

            // 2️⃣ Fallback to ASP.NET Core Identity if custom fails
            if (user == null && User?.Identity?.IsAuthenticated == true)
            {
                user = await _userManager.GetUserAsync(User);
            }

            // 3️⃣ Final guard
            if (user == null)
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account"); // or Identity default route
            }


            Console.WriteLine($"✅ Upload - User found: {user.Id}");

            model.OwnerId = user.Id;
            model.FileName = file.FileName;

            // Create uploads folder if it doesn't exist
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            // Save file physically
            var filePath = Path.Combine(uploadsFolder, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            model.FilePath = Path.Combine("/uploads", file.FileName);

            // Mock upload to Arweave
            var arweaveTxId = await _arweaveUploader.UploadAsync(file);
            model.ArweaveTxId = arweaveTxId;

            // Compute hash
            using (var sha256 = SHA256.Create())
            {
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    model.Hash = BitConverter.ToString(sha256.ComputeHash(fs)).Replace("-", "").ToLower();
                }
            }

            // Mock register on Story Protocol
            var storyId = await _storyRegistrar.RegisterAsync(model, arweaveTxId);
            model.StoryProtocolId = storyId;

            // Save to DB
            _context.IpAssets.Add(model);
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ Asset uploaded successfully: {model.Id}");

            return RedirectToAction("Success", new { id = model.Id });
        }

        public IActionResult Success(int id)
        {
            var asset = _context.IpAssets.Find(id);
            return View(asset);
        }

        // ✅ HELPER METHOD: Get user with multiple fallback methods
        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            // Method 1: GetUserAsync
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                return user;
            }

            // Method 2: Try by NameIdentifier claim
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                user = await _userManager.FindByIdAsync(userId);
                if (user != null) return user;
            }

            // Method 3: Try by Name claim
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (!string.IsNullOrEmpty(userName))
            {
                user = await _userManager.FindByNameAsync(userName);
                if (user != null) return user;
            }

            // Method 4: Try by Email claim
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                user = await _userManager.FindByEmailAsync(email);
                if (user != null) return user;
            }

            return null;
        }
    }
}