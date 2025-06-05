using Newtonsoft.Json;

namespace Midas.Core.StateMachine.JsonConverters
{
	internal sealed class StateConverter : Core.JsonConverters.JsonConverter<State>
	{
		protected override void WriteJson(JsonWriter writer, State value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value.Name);
		}
	}
}