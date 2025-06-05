using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Midas.Core.StateMachine.JsonConverters
{
	internal sealed class StateMachineListConverter : JsonConverter<IEnumerable<WeakReference<StateMachine>>>
	{
		public override void WriteJson(JsonWriter writer, IEnumerable<WeakReference<StateMachine>> value, JsonSerializer serializer)
		{
			writer.WriteStartArray();
			foreach (WeakReference<StateMachine> smRef in value)
			{
				if (smRef.TryGetTarget(out StateMachine sm))
				{
					serializer.Serialize(writer, sm);
				}
			}

			writer.WriteEndArray();
		}

		public override IEnumerable<WeakReference<StateMachine>> ReadJson(JsonReader reader, Type objectType,
			IEnumerable<WeakReference<StateMachine>>
				existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			throw new NotSupportedException();
		}
	}
}