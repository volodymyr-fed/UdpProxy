using System.Threading.Channels;

using Proxy;

var builder = Host.CreateApplicationBuilder(args);

var configuration = builder.Configuration
	.AddCommandLine(args)
	.AddEnvironmentVariables()
	.Build();

var host = configuration["UdpOptions:DomainToPull"]!;
var bytesChannel = Channel.CreateUnbounded<byte[]>();

builder.Services
	.Configure<UdpOptions>(configuration.GetSection(nameof(UdpOptions)))
	.AddSingleton(bytesChannel.Reader)
	.AddSingleton(bytesChannel.Writer)
	.AddHostedService<UdpPacketsProducer>()
	.AddHostedService<UdpHostedService>();

var app = builder.Build();

await app.RunAsync();
