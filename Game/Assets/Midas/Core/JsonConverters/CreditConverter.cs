using Midas.Core.General;
using Newtonsoft.Json;

namespace Midas.Core.JsonConverters
{
	public sealed class CreditConverter : JsonConverter<Credit>
	{
		protected override void WriteJson(JsonWriter writer, Credit value, JsonSerializer serializer)
		{
			writer.WriteValue(value.IsValid ? $"{(double)value.Value.Numerator / value.Value.Denominator:F3}cr" : "NaN");
		}
	}
}