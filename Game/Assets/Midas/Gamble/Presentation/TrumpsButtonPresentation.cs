using Midas.Presentation.ButtonHandling;
using UnityEngine;

namespace Midas.Gamble.Presentation
{
	public sealed class TrumpsButtonPresentation : ButtonPresentation
	{
		private SpriteRenderer spriteRenderer;
		private bool isSelected;

		[SerializeField]
		private Sprite upSprite;

		[SerializeField]
		private Sprite downSprite;

		[SerializeField]
		private Color enabledColor;

		[SerializeField]
		private Color disabledColor;

		private void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}

		public override bool OnSpecificButtonDataChanged(object oldSpecificData, object newSpecificData)
		{
			var newIsSelected = false;
			if (newSpecificData != null)
			{
				newIsSelected = (bool)newSpecificData;
			}

			if (newIsSelected != isSelected)
			{
				isSelected = newIsSelected;
				return true;
			}

			return false;
		}

		public override void RefreshVisualState(Button button, ButtonStateData buttonStateData)
		{
			isSelected = buttonStateData.SpecificData != null && (bool)buttonStateData.SpecificData;
			spriteRenderer.sprite = isSelected ? downSprite : upSprite;
			spriteRenderer.color = isSelected || buttonStateData.ButtonState == ButtonState.Enabled ? enabledColor : disabledColor;
		}
	}
}