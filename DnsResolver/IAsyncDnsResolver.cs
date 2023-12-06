using DNS.Protocol.ResourceRecords;

namespace DNS_server;

public interface IAsyncDnsResolver
{
    Task<IEnumerable<IPAddressResourceRecord>> ResolveAsync(string hostName);
}