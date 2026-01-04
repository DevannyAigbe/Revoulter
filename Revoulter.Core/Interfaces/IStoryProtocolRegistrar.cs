using Revoulter.Core.Models;

namespace Revoulter.Core.Interfaces
{
    public interface IStoryProtocolRegistrar
    {
        Task<string> RegisterAsync(IpAsset ipAsset, string arweaveTxId); // Returns mock registration ID
    }
}
