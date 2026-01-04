using Revoulter.Core.Interfaces;
using System.Security.Cryptography;

namespace Revoulter.Core.Services
{
    public class MockArweaveUploader : IArweaveUploader
    {
        private readonly IWebHostEnvironment _env;

        public MockArweaveUploader(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("File is required.");

            // Simulate upload: Save file to wwwroot/uploads
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Generate mock TxId (GUID) and hash
            using var sha256 = SHA256.Create();
            using var fileStream = file.OpenReadStream();
            var hash = BitConverter.ToString(sha256.ComputeHash(fileStream)).Replace("-", "").ToLower();

            return Guid.NewGuid().ToString(); // Mock Arweave TxId
        }
    }
    }
