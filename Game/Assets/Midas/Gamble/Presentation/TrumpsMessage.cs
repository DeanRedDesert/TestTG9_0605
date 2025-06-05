using UnityEngine;

namespace Midas.Gamble.Presentation
{
	/// <summary>
	/// Shows the trumps result text message.
	/// </summary>
	public sealed class TrumpsMessage : MonoBehaviour
	{
		#region Public Fields

		[SerializeField]
		private GameObject gameOver;

		[SerializeField]
		private GameObject gameWon;

		[SerializeField]
		private GameObject cycleLimitReached;

		[SerializeField]
		private GameObject winLimitReached;

		#endregion

		public void ShowGameOverMessage()
		{
			gameOver.SetActive(true);
			gameWon.SetActive(false);
			cycleLimitReached.SetActive(false);
			winLimitReached.SetActive(false);
		}

		public void ShowGameWinMessage()
		{
			gameOver.SetActive(false);
			gameWon.SetActive(true);
			cycleLimitReached.SetActive(false);
			winLimitReached.SetActive(false);
		}

		public void ShowCycleLimitReached()
		{
			gameOver.SetActive(false);
			gameWon.SetActive(false);
			cycleLimitReached.SetActive(true);
			winLimitReached.SetActive(false);
		}

		public void ShowWinLimitReached()
		{
			gameOver.SetActive(false);
			gameWon.SetActive(false);
			cycleLimitReached.SetActive(false);
			winLimitReached.SetActive(true);
		}

		public void Clear()
		{
			gameOver.SetActive(false);
			gameWon.SetActive(false);
			cycleLimitReached.SetActive(false);
			winLimitReached.SetActive(false);
		}
	}
}