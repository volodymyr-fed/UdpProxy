using System.Threading.Channels;
using Proxy.Consumers;

namespace Proxy;

sealed class UdpHostedService : BackgroundService
{
	readonly ChannelReader<byte[]> channelReader;
	readonly IEnumerable<IPacketConsumer> packetConsumers;

	public UdpHostedService(ChannelReader<byte[]> channelReader, IEnumerable<IPacketConsumer> packetConsumers)
	{
		this.channelReader = channelReader;
		this.packetConsumers = packetConsumers;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{

		while (!stoppingToken.IsCancellationRequested)
		{
			var receivedPacket = await channelReader.ReadAsync(stoppingToken);

			foreach (var consumer in packetConsumers)
				await consumer.Consume(receivedPacket, stoppingToken);
		}
	}
}
