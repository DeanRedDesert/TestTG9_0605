using UnityEngine;

namespace Midas.Presentation.General
{
	[CreateAssetMenu(menuName = "Midas/Color Selector Table", order = 100)]
	public sealed class ColorSelectorTable : ColorSelector
	{
		[SerializeField]
		private Color defaultColor = Color.white;

		[SerializeField]
		private Color[] colors;

		public override Color GetColor(int index)
		{
			if (colors == null || index >= colors.Length)
				return defaultColor;

			return colors[index];
		}
	}
}