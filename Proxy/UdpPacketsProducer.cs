using System.Buffers;
using System.Net.WebSockets;
using System.Threading.Channels;

using Microsoft.Extensions.Options;

namespace Proxy;

sealed class UdpPacketsProducer : BackgroundService
{
	const int BufferSize = 4096;
	readonly ChannelWriter<byte[]> channelWriter;
	readonly UdpOptions udpOptions;

	public UdpPacketsProducer(ChannelWriter<byte[]> channelWriter, IOptions<UdpOptions> udpOptions)
	{
		this.channelWriter = channelWriter;
		this.udpOptions = udpOptions.Value;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var webSocket = new ClientWebSocket();
		await webSocket.ConnectAsync(new Uri(udpOptions.DomainToPull), stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			using var buffer = MemoryPool<byte>.Shared.Rent(BufferSize);

			var webSocketResult = await webSocket.ReceiveAsync(buffer.Memory, stoppingToken);

			if (webSocketResult.MessageType == WebSocketMessageType.Close)
			{
				await webSocket.ConnectAsync(new Uri(udpOptions.DomainToPull), stoppingToken);
				continue;
			}

			if (webSocketResult.MessageType == WebSocketMessageType.Binary)
			{
				await channelWriter.WriteAsync(buffer.Memory[..webSocketResult.Count].ToArray(), stoppingToken);
			}
		}
	}
}
