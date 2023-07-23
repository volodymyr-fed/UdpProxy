namespace Proxy.Consumers;

sealed class BytesForwardedConsumer : IPacketConsumer
{
	readonly StatusLineUpdater statusLineUpdater;
	ulong bytesCount = 0;

	public BytesForwardedConsumer(StatusLineUpdater statusLineUpdater)
	{
		this.statusLineUpdater = statusLineUpdater;
	}

	public Task Consume(byte[] data, CancellationToken cancellationToken)
	{
		bytesCount += (uint) data.Length;

		statusLineUpdater.UpdateConsumedBytes(bytesCount);

		return Task.CompletedTask;
	}
}
