using Microsoft.Extensions.Options;

using System.Net;
using System.Net.Sockets;

namespace Proxy
{
	public class UdpHostedService : IHostedService
	{
		readonly CancellationTokenSource cancellationTokenSource = new();
		readonly UdpOptions options;
		readonly ILogger<UdpHostedService> logger;

		public UdpHostedService(IOptions<UdpOptions> options, ILogger<UdpHostedService> logger)
		{
			this.options = options.Value;
			this.logger = logger;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation(options.ToString());

			Task.Run(Forward, cancellationToken);

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			cancellationTokenSource.Cancel();
			return Task.CompletedTask;
		}

		public async Task Forward()
		{
			var endpointToSend = new IPEndPoint(IPAddress.Parse(options.IpToForward), options.PortToForward);
			var udpClient = new UdpClient(options.PortToListen);

			while (!cancellationTokenSource.IsCancellationRequested)
			{
				var receive = await udpClient.ReceiveAsync(cancellationTokenSource.Token);

				await udpClient.SendAsync(receive.Buffer, endpointToSend, cancellationTokenSource.Token);
			}
		}
	}
}
