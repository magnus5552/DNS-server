using DNS.Protocol.ResourceRecords;
using Microsoft.Extensions.Caching.Memory;

namespace DNS_server;

public class CachingAsyncDnsResolver : IAsyncDnsResolver
{
    private readonly IAsyncDnsResolver resolver;
    private readonly IMemoryCache memoryCache;
    
    public CachingAsyncDnsResolver(IAsyncDnsResolver resolver,
                                   IMemoryCache memoryCache)
    {
        this.resolver = resolver;
        this.memoryCache = memoryCache;
    }
    
    public Task<IEnumerable<IPAddressResourceRecord>> ResolveAsync(string hostName)
    {
        return memoryCache.GetOrCreateAsync(hostName, async entry =>
                                                      {
                                                          var addresses = await resolver.ResolveAsync(hostName);
                                                          entry.AbsoluteExpirationRelativeToNow =
                                                              addresses.Min(x => x.TimeToLive);
                                                          return addresses;
                                                      });
    }
}