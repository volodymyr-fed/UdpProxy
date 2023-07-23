using System.Numerics;

namespace Proxy;

sealed class StatusLineUpdater
{
	int bytesInfoPosition = 0;
	int bytesStringLength = 0;

	public void InitStatus(UdpOptions options)
	{
		Console.CursorVisible = false;
		Console.Clear();
		var statusString = $"Forwarding bytes from {options.DomainToPull} to {options.IpToForward}:{options.PortToForward}. Forwarded: {GetByteString(0)}";
		bytesInfoPosition = statusString.LastIndexOf(":") + 2;
		bytesStringLength = statusString.Length - bytesInfoPosition;
		Console.Write(statusString);
	}

	public void UpdateConsumedBytes(ulong bytesCount)
	{
		var bytesString = GetByteString(bytesCount);

		Console.SetCursorPosition(bytesInfoPosition, 0);
		Console.Write(new string(' ', bytesStringLength));
		Console.SetCursorPosition(bytesInfoPosition, 0);
		Console.Write(bytesString);

		bytesStringLength = bytesString.Length;
	}

	static string GetByteString(ulong bytesCount)
	{
		var log2 = BitOperations.Log2(bytesCount);
		var (unit, unitString) = GetUnit(log2);

		var (quot, rem) = Math.DivRem(bytesCount, unit);

		return $"{quot}.{rem * 100 / unit} {unitString}";
	}

	static (ulong unit, string unitString) GetUnit(int powerOf2)
	{
		return powerOf2 switch
		{
			< 10 => (1, "B"),
			>= 10 and < 20 => (1ul << 10, "KB"),
			>= 20 and < 30 => (1ul << 20, "MB"),
			>= 30 and < 40 => (1ul << 30, "GB"),
			>= 40  => (1ul << 40, "TB"),
		};
	}
}
