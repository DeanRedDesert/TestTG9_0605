namespace Midas.Presentation.Game
{
	public interface IHistoryPresentationNode : IPresentationNode
	{
		/// <summary>
		/// Is the node ready to show history?
		/// </summary>
		bool ReadyToShowHistory { get; }

		/// <summary>
		/// Show history.
		/// </summary>
		void ShowHistory();

		/// <summary>
		/// Clean up history presentation.
		/// </summary>
		void HideHistory();
	}
}