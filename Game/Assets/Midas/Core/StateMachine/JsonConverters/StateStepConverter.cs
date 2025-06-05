using System;
using Newtonsoft.Json;

namespace Midas.Core.StateMachine.JsonConverters
{
	internal sealed class StateStepConverter : JsonConverter<StateStep>
	{
		public override void WriteJson(JsonWriter writer, StateStep value, JsonSerializer serializer)
		{
			writer.WriteValue(value.State.Name);
			writer.WriteValue(value.Time.ToString());
			writer.WriteValue(value.FrameNumber.ToString());
		}

		public override StateStep ReadJson(JsonReader reader, Type objectType, StateStep existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			throw new NotSupportedException();
		}
	}
}