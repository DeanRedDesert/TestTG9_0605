using Midas.Presentation.ButtonHandling;
using TMPro;
using UnityEngine;

namespace Midas.Presentation.Info
{
	public sealed class InfoButtonPresentation : DefaultButtonPresentation
	{
		[SerializeField]
		private TMP_Text textToUpdate;

		public override bool OnSpecificButtonDataChanged(object oldSpecificData, object newSpecificData) => true;

		public override void RefreshVisualState(Button button, ButtonStateData buttonStateData)
		{
			textToUpdate.text = (string)buttonStateData.SpecificData;
			base.RefreshVisualState(button, buttonStateData);
		}
	}
}