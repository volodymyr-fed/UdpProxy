using System.Diagnostics;
using System.Threading.Channels;

namespace Proxy;

sealed class UdpPacketsProducer : BackgroundService
{
	readonly IUdpPacketsClient udpPacketsClient;
	readonly ChannelWriter<byte[]> channelWriter;

	public UdpPacketsProducer(IUdpPacketsClient udpPacketsClient, ChannelWriter<byte[]> channelWriter)
	{
		this.udpPacketsClient = udpPacketsClient;
		this.channelWriter = channelWriter;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var timespan = Stopwatch.GetTimestamp();

		while (!stoppingToken.IsCancellationRequested)
		{
			var elapsed = Stopwatch.GetElapsedTime(timespan);
			var timeToWait = TimeSpan.FromMilliseconds(100).Subtract(elapsed);

			if(timeToWait > TimeSpan.Zero)
				await Task.Delay(timeToWait, stoppingToken);

			var packets = await udpPacketsClient.GetPacketsAsync(stoppingToken);
			timespan = Stopwatch.GetTimestamp();

			foreach (var packet in packets)
				await channelWriter.WriteAsync(packet, stoppingToken);
		}
	}
}
