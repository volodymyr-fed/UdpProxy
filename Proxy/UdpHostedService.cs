using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

using Microsoft.Extensions.Options;

namespace Proxy;

sealed class UdpHostedService : BackgroundService
{
	readonly UdpOptions options;
	readonly ChannelReader<byte[]> channelReader;

	public UdpHostedService(ChannelReader<byte[]> channelReader, IOptions<UdpOptions> options)
	{
		this.channelReader = channelReader;
		this.options = options.Value;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var endpointToSend = new IPEndPoint(IPAddress.Parse(options.IpToForward), options.PortToForward);
		using var udpClient = new UdpClient();

		while (!stoppingToken.IsCancellationRequested)
		{
			var receivedPacket = await channelReader.ReadAsync(stoppingToken);

			await udpClient.SendAsync(receivedPacket, endpointToSend, stoppingToken);
		}
	}
}
