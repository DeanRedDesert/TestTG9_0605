using Midas.Core.General;

namespace Midas.Core
{
	public interface ICreditPlayoff
	{
		void Init(IFoundationShim foundationShim, object historyState);
		void DeInit();
		void Reset();
		void Offer(Money cash, Money bet);
		void Decline();
		public bool TryCommit();
		IOutcome StartCreditPlayoff();
		object GetHistoryState();
		void ShowHistory(object historyData);

		/// <summary>
		/// Gets the history data for the current game cycle.
		/// </summary>
		/// <returns>An array containing the logic engine specific history data.</returns>
		object GetGameCycleHistoryData();
	}
}