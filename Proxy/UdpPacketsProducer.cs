using System.Reactive.Linq;
using System.Threading.Channels;

using Microsoft.Extensions.Options;

using Websocket.Client;
using Websocket.Client.Models;

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
		using var webSocket = new WebsocketClient(new Uri(udpOptions.DomainToPull))
		{
			ErrorReconnectTimeout = TimeSpan.FromSeconds(10),
			IsReconnectionEnabled = true,
		};

		webSocket.MessageReceived
			.Where(msg => msg.Binary is not null && msg.Binary.Length > 0)
			.Subscribe(async msg => await channelWriter.WriteAsync(msg.Binary, stoppingToken));

		webSocket.ReconnectionHappened
			.Subscribe(OnReconection);

		await webSocket.Start();

		logger.LogInformation("Connected to server {ServerUrl}.", udpOptions.DomainToPull);

		await Task.Delay(Timeout.Infinite, stoppingToken);
	}

	void OnReconection(ReconnectionInfo reconnectionInfo)
	{
		if (reconnectionInfo.Type != ReconnectionType.Initial && reconnectionInfo.Type != ReconnectionType.NoMessageReceived)
			logger.LogInformation("Reconnected to server {ServerUrl}.", udpOptions.DomainToPull);
	}
}
