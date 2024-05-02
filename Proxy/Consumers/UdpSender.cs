using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Options;

namespace Proxy.Consumers;

sealed class UdpSender : IPacketConsumer, IDisposable
{
	readonly UdpClient udpClient;
	readonly IPEndPoint endpointToSend;

	public UdpSender(IOptions<UdpOptions> options, ILogger<UdpSender> logger)
	{
		udpClient = new UdpClient();
		endpointToSend = new IPEndPoint(IPAddress.Parse(options.Value.IpToForward), options.Value.PortToForward);
		logger.LogInformation("Sending bytes to {Endpoint}", endpointToSend);
	}

	public async Task Consume(byte[] data, CancellationToken cancellationToken)
	{
		await udpClient.SendAsync(data, endpointToSend, cancellationToken);
	}

	public void Dispose()
	{
		udpClient?.Dispose();
	}
}
