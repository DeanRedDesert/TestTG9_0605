using Midas.Core.General;

namespace Midas.Core
{
	public interface IGamble
	{
		void Init(IFoundationShim foundationShim, object historyState);
		void DeInit();
		IOutcome StartGamble(bool isFirstGambleCycle);
		object GetHistoryState();
		void ShowHistory(object historyData);
		bool IsPlayPossible(Money riskAmount);
		/// <summary>
		/// Gets the history data for the current game cycle.
		/// </summary>
		/// <returns>An object containing the logic engine specific history data.</returns>
		object GetGameCycleHistoryData();
	}
}