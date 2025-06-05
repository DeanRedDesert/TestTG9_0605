using System;
using Newtonsoft.Json;

namespace Midas.Core.StateMachine.JsonConverters
{
	internal sealed class StateMachineConverter : JsonConverter<StateMachine>
	{
		public override void WriteJson(JsonWriter writer, StateMachine value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("Name");
			writer.WriteValue(value.Name);
			if (value.Parent != null)
			{
				writer.WritePropertyName("Parent");
				writer.WriteValue(value.Parent.Name);
			}

			writer.WritePropertyName("CurrentState");
			serializer.Serialize(writer, value.CurrentState);
			writer.WritePropertyName("States");
			serializer.Serialize(writer, value.States);
			writer.WritePropertyName("History");
			serializer.Serialize(writer, value.StateHistory);
			writer.WriteEndObject();
		}

		public override StateMachine ReadJson(JsonReader reader, Type objectType, StateMachine existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			throw new NotSupportedException();
		}
	}
}