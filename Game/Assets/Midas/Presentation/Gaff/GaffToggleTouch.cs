using Midas.Core.General;
using Midas.Presentation.Data;
using UnityEngine;

namespace Midas.Presentation.Gaff
{
	public sealed class GaffToggleTouch : MonoBehaviour
	{
		private BoxCollider2D touch;

		private void Awake()
		{
			touch = GetComponent<BoxCollider2D>();
		}

		private void OnMouseDown()
		{
			GaffToggleController.IsTouched = true;
		}

		private void Update()
		{
			if (touch == null || StatusDatabase.GameStatus == null || StatusDatabase.BankStatus == null)
				return;

			touch.enabled = StatusDatabase.GameStatus.GameIsIdle && StatusDatabase.BankStatus.BankMeter == Money.Zero;
		}
	}
}