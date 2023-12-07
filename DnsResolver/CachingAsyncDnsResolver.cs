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
        => memoryCache.GetOrCreateAsync(hostName, async entry => await GetItem(entry, hostName))!;

    private async Task<IEnumerable<IPAddressResourceRecord>> GetItem(ICacheEntry entry, string hostName)
    {
        var addresses = (await resolver.ResolveAsync(hostName)).ToArray();
        if (addresses.Length != 0)
            entry.AbsoluteExpirationRelativeToNow = addresses.Min(x => x.TimeToLive);
        else
            entry.Dispose();
        
        return addresses;
    }
}