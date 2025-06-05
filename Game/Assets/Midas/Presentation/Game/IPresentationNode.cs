namespace Midas.Presentation.Game
{
	public interface IPresentationNode
	{
		/// <summary>
		/// Identifies the node so it can be discovered easily.
		/// </summary>
		string NodeId { get; }

		/// <summary>
		/// Gets whether the node is ready to start.
		/// </summary>
		bool ReadyToStart { get; }

		/// <summary>
		/// Gets whether the main part of the node is complete.
		/// </summary>
		bool IsMainActionComplete { get; }

		/// <summary>
		/// Initialise the node.
		/// </summary>
		void Init();

		/// <summary>
		/// Deinit the node.
		/// </summary>
		void DeInit();

		/// <summary>
		/// Destroy the node.
		/// </summary>
		void Destroy();

		/// <summary>
		/// Perform the node action.
		/// </summary>
		void Show();
	}
}