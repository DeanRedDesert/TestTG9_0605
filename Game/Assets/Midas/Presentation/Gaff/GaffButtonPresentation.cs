using Midas.Core;
using Midas.Presentation.ButtonHandling;
using TMPro;
using UnityEngine;

namespace Midas.Presentation.Gaff
{
	public sealed class GaffButtonPresentation : DefaultButtonPresentation
	{
		private bool isHighlighted;

		[SerializeField]
		private TMP_Text textToUpdate;

		protected override bool IsHighlighted => isHighlighted;

		public override bool OnSpecificButtonDataChanged(object oldSpecificData, object newSpecificData)
		{
			return true;
		}

		public override void RefreshVisualState(Button button, ButtonStateData buttonStateData)
		{
			if (buttonStateData == null)
			{
				isHighlighted = false;
			}
			else
			{
				var gaff = ((IGaffSequence gaff, bool isSelected))buttonStateData.SpecificData;
				isHighlighted = gaff.isSelected;
				textToUpdate.text = gaff.gaff == null ? "" : gaff.gaff.GaffType == GaffType.Development ? $"{gaff.gaff.Name} (dev)" : gaff.gaff.Name;
			}

			base.RefreshVisualState(button, buttonStateData);
		}
	}
}