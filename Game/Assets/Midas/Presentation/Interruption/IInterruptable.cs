namespace Midas.Presentation.Interruption
{
	public interface IInterruptable
	{
		/// <summary>
		/// false if currently not possible to interrupt e.g.: Win presentation with high win may be interrupted after a timeout.
		/// </summary>
		bool CanBeInterrupted { get; }

		/// <summary>
		/// Set to true to allow the feature auto start to trigger the interruption.
		/// </summary>
		bool CanBeAutoInterrupted { get; }

		/// <summary>
		/// Interruptables with higher priority values will be interrupted first.
		/// </summary>
		int InterruptPriority { get; }

		/// <summary>
		/// Interrupt the interruptable.
		/// </summary>
		void Interrupt();
	}
}