namespace Midas.LogicToPresentation.Messages
{
	/// <summary>
	///     Message holds current active paytable instances and stake combination
	///     <remarks>
	///         Pres  -> Logic: Whenever the player changes the active paytable instance or stake combination
	///     </remarks>
	/// </summary>
	public sealed class GameStakeCombinationMessage : IMessage
	{
		#region Public

		public GameStakeCombinationMessage(int stakeCombinationIndex)
		{
			SelectedStakeCombinationIndex = stakeCombinationIndex;
		}

		public int SelectedStakeCombinationIndex { get; }

		#endregion
	}
}