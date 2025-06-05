using Midas.Core.General;
using Newtonsoft.Json;

namespace Midas.Core.JsonConverters
{
	public sealed class RationalNumberConverter : JsonConverter<RationalNumber>
	{
		protected override void WriteJson(JsonWriter writer, RationalNumber value, JsonSerializer serializer)
		{
			if (writer.WriteState != WriteState.Object)
			{
				writer.WriteStartObject();
			}

			writer.WritePropertyName("Numerator");
			writer.WriteValue(value.Numerator);
			writer.WritePropertyName("Denominator");
			writer.WriteValue(value.Denominator);
			if (writer.WriteState != WriteState.Object)
			{
				writer.WriteEndObject();
			}
		}
	}
}