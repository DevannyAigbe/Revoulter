using Revoulter.Core.Interfaces;
using System.Security.Cryptography;

namespace Revoulter.Core.Services
{
    public class MockArweaveUploader : IArweaveUploader
    {
        public MockArweaveUploader()
        {
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is required.");

            // Use temp folder instead of wwwroot (writable in Docker)
            var uploadsFolder = Path.Combine(Path.GetTempPath(), "uploads");
            Directory.CreateDirectory(uploadsFolder);

            // Safe file name
            var safeFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, safeFileName);

            // Save file to temp
            await using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(stream);
            }

            // Optional: compute hash if needed
            using var sha256 = SHA256.Create();
            await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var hash = BitConverter.ToString(sha256.ComputeHash(fs)).Replace("-", "").ToLower();

            // Return mock Arweave TxId
            return Guid.NewGuid().ToString();
        }
    }
}
