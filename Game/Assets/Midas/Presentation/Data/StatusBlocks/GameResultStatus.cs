using System.Collections.Generic;

namespace Midas.Presentation.Data.StatusBlocks
{
	public abstract class GameResultStatus : StatusBlock
	{
		protected GameResultStatus(string name) : base(name)
		{
		}

		/// <summary>
		/// Get the win info for the current game cycle.
		/// </summary>
		/// <returns>The list of wins for the current game cycle, sorted into presentation order.</returns>
		public abstract IReadOnlyList<IWinInfo> GetWinInfo();

		/// <summary>
		/// Gets whether the current cycle is the base game cycle.
		/// </summary>
		public abstract bool IsBaseGameCycle();

		/// <summary>
		/// Gets whether the game is over.
		/// </summary>
		public abstract bool IsGameFinished();
	}
}