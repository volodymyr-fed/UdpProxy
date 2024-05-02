namespace UdpServer;

sealed record UdpOptions
{
	public int UdpPort { get; set; }
	public int ServerPort { get; set; }
}