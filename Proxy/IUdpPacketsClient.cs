using Refit;

namespace Proxy;

public interface IUdpPacketsClient
{
	[Get("/udppackets")]
	public Task<byte[][]> GetPacketsAsync(CancellationToken cancellationToken);
}
