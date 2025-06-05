using System;
using System.Globalization;

namespace Midas.Presentation.Sequencing
{
	/// <summary>
	/// Used to insert sequence events between other events in more complicated sequences.
	/// </summary>
	public readonly struct CustomEvent
	{
		public const int MidasEventStartId = 900;
		public const int CustomEventStartId = 1000;

		/// <summary>
		/// The event ID equal to the enum value given in the constructor.
		/// </summary>
		public int Id { get; }

		/// <summary>
		/// The name of the event, taken from the enum given in the constructor.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <remarks>
		/// The enum value in <see cref="eventId"/> should be based from <see cref="CustomEventStartId"/>
		/// <code>
		///	public enum MySequenceEvents
		/// {
		///		SeqEventA = CustomEvent.CustomEventStartId,
		///		SeqEventB,
		///		...
		/// }
		/// </code>
		/// </remarks>
		/// <param name="eventId">This can come from any enum.</param>
		public CustomEvent(Enum eventId)
		{
			var convertible = (IConvertible)eventId;
			Id = convertible.ToInt32(null);
			Name = convertible.ToString(CultureInfo.InvariantCulture);
		}
	}
}