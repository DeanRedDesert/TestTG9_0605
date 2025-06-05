using Midas.Presentation.Data.StatusBlocks;

namespace Midas.Presentation.Dashboard
{
	/// <summary>
	/// Used to allow customisation of the dashboard game messages used in the glolbal GI.
	/// </summary>
	public interface IGameMessages
	{
		/// <summary>
		/// Called to override the game message displayed on the left of the dashboard.
		/// </summary>
		/// <param name="gameMessageLeft"></param>
		/// <returns>Returns true if the game message is valid.</returns>
		bool GetGameSpecificGameMessage(out GameMessageLeft gameMessageLeft);

		/// <summary>
		/// Called to check if a given win info is.
		/// </summary>
		/// <param name="winInfo">The win info to check. ></param>
		/// <param name="isRetrigger">Is the prize a retrigger prize?</param>
		/// <returns>Returns true if the win info is a feature trigger.</returns>
		bool IsFeatureTriggerPrize(IWinInfo winInfo, out bool isRetrigger);

		/// <summary>
		/// Called to check if the current game play message should only display the bonus message.
		/// </summary>
		bool UseBonusPlay();
	}
}