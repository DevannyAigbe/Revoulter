using Revoulter.Core.Interfaces;
using Revoulter.Core.Models;

namespace Revoulter.Core.Services
{
    public class MockStoryProtocolRegistrar : IStoryProtocolRegistrar
    {
        public Task<string> RegisterAsync(IpAsset ipAsset, string arweaveTxId)
        {
            // Simulate registration: Just generate a mock ID
            // In real, this would call Story Protocol API with arweaveTxId
            return Task.FromResult(Guid.NewGuid().ToString());
        }
    }
 }
