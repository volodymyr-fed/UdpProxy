using System.Net.Sockets;
using System.Threading.Channels;

using Microsoft.Extensions.Options;

namespace UdpServer;

sealed class UdpReceiverService : BackgroundService
{
	readonly UdpOptions udpOptions;
	readonly ChannelWriter<byte[]> channelWriter;
	readonly ILogger<UdpReceiverService> _logger;

	public UdpReceiverService(IOptions<UdpOptions> udpOptions, ChannelWriter<byte[]> channelWriter, ILogger<UdpReceiverService> logger)
	{
		this.udpOptions = udpOptions.Value;
		this.channelWriter = channelWriter;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var udpClient = new UdpClient(udpOptions.UdpPort);
		_logger.LogInformation("Listening UDP port {port}", udpOptions.UdpPort);

		while (!stoppingToken.IsCancellationRequested)
		{
			var udpResult = await udpClient.ReceiveAsync(stoppingToken);

			await channelWriter.WriteAsync(udpResult.Buffer, stoppingToken);
		}
	}
}
