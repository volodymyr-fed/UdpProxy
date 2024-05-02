using System.Threading.Channels;

using Microsoft.Extensions.Options;

using Proxy;
using Proxy.Consumers;

var builder = Host.CreateApplicationBuilder(args);

var configuration = builder.Configuration
	.AddCommandLine(args)
	.AddEnvironmentVariables()
	.Build();

var host = configuration["UdpOptions:DomainToPull"]!;
var bytesChannel = Channel.CreateUnbounded<byte[]>();

var statusLineUpdater = new StatusLineUpdater();

builder.Services
	.Configure<UdpOptions>(configuration.GetSection(nameof(UdpOptions)))
	.AddSingleton(bytesChannel.Reader)
	.AddSingleton(bytesChannel.Writer)
	.AddSingleton<IPacketConsumer, UdpSender>()
	.AddSingleton<IPacketConsumer, BytesForwardedConsumer>()
	.AddSingleton(statusLineUpdater)
	.AddHostedService<UdpPacketsProducer>()
	.AddHostedService<UdpHostedService>();

var app = builder.Build();

statusLineUpdater.InitStatus(app.Services.GetRequiredService<IOptions<UdpOptions>>().Value);

await app.RunAsync();
