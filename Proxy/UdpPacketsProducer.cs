using System.Threading.Channels;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace Proxy;

sealed class UdpPacketsProducer : BackgroundService
{
	readonly ChannelWriter<byte[]> channelWriter;
	readonly ILogger<UdpPacketsProducer> logger;
	readonly UdpOptions udpOptions;

	public UdpPacketsProducer(ChannelWriter<byte[]> channelWriter, IOptions<UdpOptions> udpOptions,
		ILogger<UdpPacketsProducer> logger)
	{
		this.channelWriter = channelWriter;
		this.logger = logger;
		this.udpOptions = udpOptions.Value;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var connection = new HubConnectionBuilder()
			.WithUrl(new Uri(udpOptions.DomainToPull))
			.WithAutomaticReconnect(new RetryPolicy())
			.Build();

		connection.Reconnected += OnReconection;
		await ConnectWithRetryAsync(connection, stoppingToken);

		if (stoppingToken.IsCancellationRequested)
			return;

		connection.On<byte[]>("ReceiveBytes", async bytes => await channelWriter.WriteAsync(bytes));

		logger.LogInformation("Connected to server {ServerUrl}.", udpOptions.DomainToPull);
	}

	Task OnReconection(string? _)
	{
		logger.LogInformation("Reconnected to server {ServerUrl}.", udpOptions.DomainToPull);

		return Task.CompletedTask;
	}

	static async Task<bool> ConnectWithRetryAsync(HubConnection connection, CancellationToken token)
	{
		while (true)
		{
			try
			{
				await connection.StartAsync(token);
				return true;
			}
			catch when (token.IsCancellationRequested)
			{
				return false;
			}
			catch
			{
				await Task.Delay(5000, token);
			}
		}
	}
}

sealed class RetryPolicy : IRetryPolicy
{
	public TimeSpan? NextRetryDelay(RetryContext retryContext)
	{
		return TimeSpan.FromSeconds(5);
	}
}
