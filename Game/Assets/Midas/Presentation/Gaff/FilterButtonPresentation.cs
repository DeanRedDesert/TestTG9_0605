using Midas.Core;
using Midas.Presentation.ButtonHandling;
using TMPro;
using UnityEngine;

namespace Midas.Presentation.Gaff
{
	public class FilterButtonPresentation : DefaultButtonPresentation
	{
		[SerializeField]
		private string formatString;

		[SerializeField]
		private TMP_Text[] textToUpdate;

		public override bool OnSpecificButtonDataChanged(object oldSpecificData, object newSpecificData)
		{
			return true;
		}

		public override void RefreshVisualState(Button button, ButtonStateData buttonStateData)
		{
			base.RefreshVisualState(button, buttonStateData);

			var text = "";
			if (buttonStateData != null)
			{
				var selectedSet = (GaffType?)buttonStateData.SpecificData;

				text = selectedSet.HasValue ? selectedSet.Value.ToString() : "All";
				text = string.Format(formatString, text);
			}

			foreach (var t in textToUpdate)
				t.text = text;
		}
	}
}