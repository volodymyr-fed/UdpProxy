using System.Threading.Channels;

using UdpServer;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration
	.AddCommandLine(args)
	.AddEnvironmentVariables()
	.Build();

builder.WebHost.UseUrls($"http://*:{configuration[$"{nameof(UdpOptions)}:{nameof(UdpOptions.ServerPort)}"]}");

var channel = Channel.CreateUnbounded<byte[]>();

builder.Services.AddSignalR();
builder.Services
	.Configure<UdpOptions>(configuration.GetSection(nameof(UdpOptions)))
	.AddSingleton(channel.Writer)
	.AddSingleton(channel.Reader)
	.AddHostedService<UdpReceiverService>()
	.AddHostedService<SignalRServerService>();

var app = builder.Build();

app.MapHub<UdpHub>("/udp");

await app.RunAsync();

