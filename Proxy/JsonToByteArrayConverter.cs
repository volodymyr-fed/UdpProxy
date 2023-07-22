using System.Text.Json.Serialization;
using System.Text.Json;
using System.Buffers;

sealed class JsonToByteArrayConverter : JsonConverter<byte[]?>
{
	const int BufferLength = 2500;

	public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null || reader.TokenType == JsonTokenType.None)
			return null;

		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException("The JSON value could not be converted to System.Byte[]");

		using var dest = MemoryPool<byte>.Shared.Rent(BufferLength);
		var span = dest.Memory.Span;
		var currentLength = 0;

		while (reader.Read() && reader.TokenType != JsonTokenType.EndArray && currentLength < BufferLength)
		{
			span[currentLength++] = reader.GetByte();
		};

		if (currentLength >= BufferLength && reader.TokenType != JsonTokenType.EndArray)
			throw new JsonException("Expected byte array with less than 2500 elements.");

		return span[..currentLength].ToArray();
	}

	public override void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
	{
		if (value is null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStartArray();

		foreach (var byteValue in value)
			writer.WriteNumberValue(byteValue);

		writer.WriteEndArray();
	}
}