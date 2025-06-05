using Midas.Core;
using Midas.Presentation.Data;
using UnityEngine;

namespace Game.GameIdentity.Anz
{
	public sealed class MetersToggle : MonoBehaviour
	{
		private bool cashMode = true;

		[SerializeField]
		private GameObject[] cashObjects;

		[SerializeField]
		private GameObject[] creditsObjects;

		private void Start()
		{
			Refresh();
		}

		private void Refresh()
		{
			foreach (var o in cashObjects)
				o.SetActive(cashMode);

			var creditsMode = !cashMode;

			foreach (var o in creditsObjects)
				o.SetActive(creditsMode);
		}

		private void OnMouseDown()
		{
			if (StatusDatabase.GameStatus.GameIsIdle || StatusDatabase.GameStatus.GameMode == FoundationGameMode.History)
				cashMode = !cashMode;

			Refresh();
		}
	}
}