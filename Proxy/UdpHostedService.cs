using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

using Microsoft.Extensions.Options;

namespace Proxy;

sealed class UdpHostedService : BackgroundService
{
	readonly UdpOptions options;
	readonly ChannelReader<byte[]> channelReader;
	readonly ILogger<UdpHostedService> logger;

	public UdpHostedService(ChannelReader<byte[]> channelReader, IOptions<UdpOptions> options, ILogger<UdpHostedService> logger)
	{
		this.channelReader = channelReader;
		this.options = options.Value;
		this.logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Sending bytes to {IpToForward}:{PortToForward}",
			options.IpToForward, options.PortToForward);

		var endpointToSend = new IPEndPoint(IPAddress.Parse(options.IpToForward), options.PortToForward);
		using var udpClient = new UdpClient();

		while (!stoppingToken.IsCancellationRequested)
		{
			var receivedPacket = await channelReader.ReadAsync(stoppingToken);

			await udpClient.SendAsync(receivedPacket, endpointToSend, stoppingToken);
		}
	}
}
