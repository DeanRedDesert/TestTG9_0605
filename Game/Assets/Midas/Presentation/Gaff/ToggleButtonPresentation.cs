using Midas.Presentation.ButtonHandling;

namespace Midas.Presentation.Gaff
{
	public class ToggleButtonPresentation : DefaultButtonPresentation
	{
		private bool isHighlighted;

		protected override bool IsHighlighted => isHighlighted;

		public override bool OnSpecificButtonDataChanged(object oldSpecificData, object newSpecificData)
		{
			return true;
		}

		public override void RefreshVisualState(Button button, ButtonStateData buttonStateData)
		{
			isHighlighted = (bool?)buttonStateData?.SpecificData ?? false;
			base.RefreshVisualState(button, buttonStateData);
		}
	}
}