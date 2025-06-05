namespace Midas.Core.LogicServices
{
	/// <summary>
	/// Game service interface for use by the logic side.
	/// </summary>
	public interface IGameService
	{
		/// <summary>
		/// Gets the name of this service.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Init the game service.
		/// </summary>
		void Init(string name);

		/// <summary>
		/// DeInit the game service.
		/// </summary>
		void DeInit();

		/// <summary>
		/// Gets whether the service needs to be saved in history for a particular snapshot type.
		/// </summary>
		bool IsHistoryRequired(HistorySnapshotType snapshotType);

		/// <summary>
		/// Notify any listeners that a change has occurred.
		/// </summary>
		void DeliverChange(object value);

		/// <summary>
		/// Sends the service to the presentation regardless of whether it has changed.
		/// </summary>
		void Refresh();

		/// <summary>
		/// Get the data to save into history.
		/// </summary>
		object GetHistoryData(HistorySnapshotType snapshotType);

		/// <summary>
		/// Restore the game service from snapshot data.
		/// </summary>
		void RestoreHistoryData(HistorySnapshotType snapshotType, object o);
	}
}