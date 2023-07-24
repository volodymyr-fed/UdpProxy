using System.Threading.Channels;

using Microsoft.AspNetCore.SignalR;

namespace UdpServer;

sealed class SignalRServerService : BackgroundService
{
	readonly ChannelReader<byte[]> channelReader;
	readonly IHubContext<UdpHub, IUdpClient> hubContext;

	public SignalRServerService(ChannelReader<byte[]> channelReader, IHubContext<UdpHub, IUdpClient> hubContext)
	{
		this.channelReader = channelReader;
		this.hubContext = hubContext;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var data = await channelReader.ReadAsync(stoppingToken);

			await hubContext.Clients.All.ReceiveBytes(data, stoppingToken);
		}
	}
}
