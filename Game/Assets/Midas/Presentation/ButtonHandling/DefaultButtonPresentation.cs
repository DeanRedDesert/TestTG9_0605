using UnityEngine;

namespace Midas.Presentation.ButtonHandling
{
	public class DefaultButtonPresentation : DefaultButtonPresentationBase
	{
		[SerializeField]
		private Color highlightSpriteColor;

		[SerializeField]
		private Color highlightTextColor;

		protected override Color HighlightSpriteColor => highlightSpriteColor;
		protected override Color HighlightTextColor => highlightTextColor;
		protected override bool IsHighlighted => false;
	}
}