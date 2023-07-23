namespace Proxy.Consumers;

interface IPacketConsumer
{
	public Task Consume(byte[] data, CancellationToken cancellationToken);
}
