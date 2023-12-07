using System.Diagnostics;
using DNS.Protocol.ResourceRecords;

namespace DNS_server;

public class TimingAsyncDnsResolver : IAsyncDnsResolver
{
    private readonly IAsyncDnsResolver resolver;
    private readonly ILogger<IAsyncDnsResolver> logger;
    
    public TimingAsyncDnsResolver(IAsyncDnsResolver resolver,
                                  ILogger<IAsyncDnsResolver> logger)
    {
        this.resolver = resolver;
        this.logger = logger;
    }
    
    public Task<IEnumerable<IPAddressResourceRecord>> ResolveAsync(string hostName)
    {
        var timer = Stopwatch.StartNew();
        var result = resolver.ResolveAsync(hostName);
        timer.Stop();
        
        logger.LogInformation($"Resolved {hostName} in {timer.ElapsedMilliseconds} ms");

        return result;
    }
}