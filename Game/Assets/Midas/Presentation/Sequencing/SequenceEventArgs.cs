namespace Midas.Presentation.Sequencing
{
	public sealed class SequenceEventArgs
	{
		public int EventId { get; }
		public int Intensity { get; }
		public object Data { get; }

		public SequenceEventArgs(int eventId, int intensity = 1, object data = null)
		{
			EventId = eventId;
			Data = data;
			Intensity = intensity;
		}
	}
}