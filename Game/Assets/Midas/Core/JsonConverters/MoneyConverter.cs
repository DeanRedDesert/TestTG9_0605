using Midas.Core.General;
using Newtonsoft.Json;

namespace Midas.Core.JsonConverters
{
	public sealed class MoneyConverter : JsonConverter<Money>
	{
		protected override void WriteJson(JsonWriter writer, Money value, JsonSerializer serializer)
		{
			if (value.IsValid)
				writer.WriteValue(value.AsMinorCurrency);
			else
				writer.WriteValue("NaN");
		}
	}
}