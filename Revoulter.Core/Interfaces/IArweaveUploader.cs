namespace Revoulter.Core.Interfaces
{
    public interface IArweaveUploader
    {
        Task<string> UploadAsync(IFormFile file); // Returns mock transaction ID
    }
}
