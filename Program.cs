using System.Net.Sockets;
using DNS_server;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<HostOptions>(
    options => options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

builder.Services.AddOptions<DnsOptions>().BindConfiguration(DnsOptions.Dns);

builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<UdpClient>();
builder.Services.AddSingleton<IResponseParser, ResponseParser>();
builder.Services.AddTransient<IRequestBuilder, RequestBuilder>();
builder.Services.AddSingleton<Func<IRequestBuilder>>(provider => () => provider.GetService<IRequestBuilder>()!);
builder.Services.AddSingleton<IAsyncDnsResolver, AsyncDnsResolver>();

var host = builder.Build();
host.Run();