using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Midas.Presentation.DevHelpers.JsonConverters
{
	public sealed class UnityColorConverter : Midas.Core.JsonConverters.JsonConverter<Color>
	{
		protected override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer) => writer.WriteValue(value.ToString());
	}
}