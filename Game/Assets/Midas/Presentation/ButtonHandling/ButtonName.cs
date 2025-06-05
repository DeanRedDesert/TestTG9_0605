using System;
using Midas.Presentation.Cabinet;
using UnityEngine;

namespace Midas.Presentation.ButtonHandling
{
	[Serializable]
	public sealed class ButtonName
	{
		#region Public

		public ButtonName() { }

		public ButtonName(string buttonName)
		{
			idName = buttonName;
		}

		public string Name => idName;

		public PhysicalButton? Button => CabinetManager.Cabinet.PhysicalButtons?.GetButtonFromName(idName);

		#endregion

		#region Private

		[SerializeField]
		private string idName = "Undefined";

		#endregion
	}
}