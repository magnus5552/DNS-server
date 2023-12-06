using System.Net;
using System.Net.Sockets;
using DNS.Client;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Server;
using Microsoft.Extensions.Options;

namespace DNS_server;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private readonly IAsyncDnsResolver dnsResolver;
    private readonly DnsOptions dnsOptions;

    private IPEndPoint workerEndpoint;
    private UdpClient listener;

    public Worker(ILogger<Worker> logger,
                  IAsyncDnsResolver dnsResolver,
                  IOptions<DnsOptions> dnsOptions)
    {
        this.logger = logger;
        this.dnsResolver = dnsResolver;
        this.dnsOptions = dnsOptions.Value;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var address = IPAddress.Parse(dnsOptions.Address);
        workerEndpoint = new IPEndPoint(address, dnsOptions.Port);
        listener = new UdpClient(workerEndpoint);
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"Server started on {workerEndpoint}");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var request = await listener.ReceiveAsync(stoppingToken);
                logger.LogInformation($"Request from {request.RemoteEndPoint}");

                var dnsRequest = Request.FromArray(request.Buffer);
                var domain = dnsRequest.Questions.First(x => x.Type == RecordType.A).Name;
                
                var resolvedIPs = await dnsResolver.ResolveAsync(domain.ToString()!);
                
                var response = Response.FromRequest(dnsRequest);
                foreach (var address in resolvedIPs) 
                    response.AnswerRecords.Add(address);
                await listener.SendAsync(response.ToArray(), request.RemoteEndPoint, stoppingToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, null);
            }
        }
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Server is stopping");
        listener.Dispose();
        return base.StopAsync(stoppingToken);
    }
}