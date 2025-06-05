using Midas.Presentation.Symbols;
using TMPro;
using UnityEngine;

namespace Game.Stages.Common.Symbols
{
	[RequireComponent(typeof(TMP_Text))]
	public sealed class TextFallbackSymbolOverlay : FallbackSymbolOverlay
	{
		private TMP_Text text;

		private void Awake()
		{
			text = GetComponent<TMP_Text>();
		}

		public override void SetSymbolId(string symbolId)
		{
			if (text.text != symbolId)
				text.SetText(symbolId);
		}
	}
}