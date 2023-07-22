namespace Proxy;

public record UdpOptions
{
	public string IpToForward { get; set; } = string.Empty;
	public int PortToForward { get; set; }
	public string DomainToPull { get; set; } = string.Empty;
}
