using System.Text.Json;
using System.Threading.Channels;

using Polly;
using Polly.Extensions.Http;

using Proxy;

using Refit;

var builder = Host.CreateApplicationBuilder(args);

var configuration = builder.Configuration
	.AddCommandLine(args)
	.AddEnvironmentVariables()
	.Build();

var host = configuration["UdpOptions:DomainToPull"]!;
var channel = Channel.CreateUnbounded<byte[]>();

builder.Services
	.Configure<UdpOptions>(configuration.GetSection(nameof(UdpOptions)))
	.AddRefitClient<IUdpPacketsClient>(new RefitSettings
	{
		ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
		{
			Converters = { new JsonToByteArrayConverter() }
		})
	})
	.ConfigureHttpClient(c =>
	{
		c.BaseAddress = new Uri(host);
	})
	.AddPolicyHandler(HttpPolicyExtensions
		.HandleTransientHttpError()
		.WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

builder.Services
	.AddSingleton(channel.Reader)
	.AddSingleton(channel.Writer)
	.AddHostedService<UdpPacketsProducer>()
	.AddHostedService<UdpHostedService>();

var app = builder.Build();

await app.RunAsync();
