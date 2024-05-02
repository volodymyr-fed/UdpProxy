using Microsoft.AspNetCore.SignalR;

namespace UdpServer;

public interface IUdpClient
{
	Task ReceiveBytes(byte[] bytes, CancellationToken cancellation);
}

sealed class UdpHub : Hub<IUdpClient>
{
}
