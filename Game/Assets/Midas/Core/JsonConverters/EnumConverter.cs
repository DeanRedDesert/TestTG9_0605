using System;
using Newtonsoft.Json;

namespace Midas.Core.JsonConverters
{
	public sealed class EnumConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => writer.WriteValue(value.ToString());
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotSupportedException();
		public override bool CanConvert(Type objectType) => objectType.IsEnum;
	}
}