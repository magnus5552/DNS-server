using System.Net;
using System.Net.Sockets;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using Microsoft.Extensions.Options;

namespace DNS_server;

public class AsyncDnsResolver : IAsyncDnsResolver
{
    private readonly UdpClient udpClient;
    private readonly Func<IRequestBuilder> requestBuilderFactory;
    private readonly IResponseParser responseParser;
    private readonly DnsOptions dnsOptions;
    private readonly ILogger<IAsyncDnsResolver> logger;
    
    public AsyncDnsResolver(UdpClient udpClient,
                            Func<IRequestBuilder> requestBuilderFactory,
                            IResponseParser responseParser,
                            IOptions<DnsOptions> dnsOptions,
                            ILogger<IAsyncDnsResolver> logger)
    {
        this.udpClient = udpClient;
        this.requestBuilderFactory = requestBuilderFactory;
        this.responseParser = responseParser;
        this.logger = logger;
        this.dnsOptions = dnsOptions.Value;
    }
    
    public async Task<IEnumerable<IPAddressResourceRecord>> ResolveAsync(string hostName)
    {
        logger.LogInformation($"Resolving hostname: {hostName}");
        var uri = new Uri(hostName);
        var domain = Domain.FromString(uri.DnsSafeHost);

        var endpointAddresses = new HashSet<IPAddress> { IPAddress.Parse(dnsOptions.RootServerAddress) };
        var resolvedIPs = new HashSet<IPAddressResourceRecord>();
        while (resolvedIPs.Count == 0)
        {
            var nextEndpoints = new HashSet<IPAddress>();
            foreach (var endpointAddress in endpointAddresses)
            {
                var response = await GetRecord(domain, endpointAddress, RecordType.A);
                resolvedIPs.UnionWith(response.AnswerRecords
                                              .OfType<IPAddressResourceRecord>());
                
                var additionalRecords = response.AdditionalRecords
                                                .OfType<IPAddressResourceRecord>()
                                                .Where(record => record.Type == RecordType.A);
                var authorityRecords = response.AuthorityRecords
                                               .OfType<NameServerResourceRecord>();
                nextEndpoints.UnionWith(additionalRecords.Join(authorityRecords,
                                                               record => record.Name,
                                                               record => record.NSDomainName,
                                                               (a, _) => a.IPAddress));
            }
            if (nextEndpoints.Count == 0)
                break;
            endpointAddresses = nextEndpoints;
        } 

        return resolvedIPs.DistinctBy(x => x.IPAddress);
    }
    
    private async Task<Response> GetRecord(Domain domain, IPAddress endpointAddress, RecordType recordType)
    {
        var request = requestBuilderFactory().AddQuestion(domain, recordType)
                                             .WithOperationCode(OperationCode.Query)
                                             .RecursionDesired(false)
                                             .Build();

        return await MakeRequest(request, endpointAddress);
    }
    
    private async Task<Response> MakeRequest(byte[] request, IPAddress endpointAddress)
    {
        var endpoint = new IPEndPoint(endpointAddress, dnsOptions.RemotePort);
        await udpClient.SendAsync(request, request.Length, endpoint);
        var result = await udpClient.ReceiveAsync(); 
        return responseParser.Parse(result.Buffer);
    }

}