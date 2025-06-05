using System.Collections.Generic;

namespace Midas.Core
{
	/// <summary>
	/// Provides important paytable functionality.
	/// </summary>
	public interface IGame
	{
		/// <summary>
		/// Gets a key that uniquely identifies the configuration.
		/// </summary>
		string ConfigurationKey { get; }

		/// <summary>
		/// Gets all the unique stake combinations for the game.
		/// </summary>
		IReadOnlyList<IStakeCombination> StakeCombinations { get; }

		/// <summary>
		/// Gets the list of progressive levels defined in the game.
		/// </summary>
		/// <returns></returns>
		IReadOnlyList<(string LevelId, uint GameLevel)> ProgressiveLevels { get; }

		/// <summary>
		/// Initialise the game.
		/// </summary>
		/// <param name="foundationShim">The foundation.</param>
		/// <param name="gameReset">True if the game should be reset (denom change or exit to game menu for example).</param>
		/// <param name="historyGameState">The history state data if in history mode.</param>
		void Init(IFoundationShim foundationShim, bool gameReset, object historyGameState);

		/// <summary>
		/// Perform any cleanup the game requires.
		/// </summary>
		void DeInit();

		/// <summary>
		/// Allows the game to prepare for game start.
		/// </summary>
		/// <param name="stakeCombinationIndex">The stake combination that this game will use.</param>
		IOutcome StartGame(int stakeCombinationIndex);

		/// <summary>
		/// Allows the game to prepare for game continuation.
		/// </summary>
		IOutcome ContinueGame();

		/// <summary>
		/// Apply win capping to the game.
		/// </summary>
		void ApplyWinCapping();

		/// <summary>
		/// Gets the history data for the current game cycle.
		/// </summary>
		/// <returns>An array containing the logic engine specific history data.</returns>
		object GetGameCycleHistoryData();

		/// <summary>
		/// Save the history state for the game.
		/// </summary>
		object GetHistoryState();

		/// <summary>
		/// Show the history game result.
		/// </summary>
		/// <param name="historyData">The logic engine specific history data.</param>
		void ShowHistory(object historyData);

		/// <summary>
		/// Enables gaff for the next game cycle.
		/// </summary>
		void SetGaffActive(int gaffType);

		/// <summary>
		/// Provides gaff results to use for the next cycles.
		/// </summary>
		void SetGaffActive(IDialUpResults gaffResults);

		/// <summary>
		/// Provides the game data required to perform gaffing.
		/// </summary>
		/// <returns>The logic specific data required for gaffing.</returns>
		IDialUpData GetGaffData();

		/// <summary>
		/// Returns the new stake combination index if a gaff requires one.
		/// </summary>
		/// <returns>The required stake combination or null if no requirement exists.</returns>
		int? CheckGaffStakeCombinationOverride();
	}
}