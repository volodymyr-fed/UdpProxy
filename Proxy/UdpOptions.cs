namespace Proxy
{
	public record UdpOptions
	{
		public string IpToForward { get; set; } = string.Empty;
		public int PortToForward { get; set; }
		public int PortToListen { get; set; }
		public string MyDomain { get; set; } = string.Empty;
	}
}
