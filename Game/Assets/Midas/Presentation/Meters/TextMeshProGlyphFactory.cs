using System.Linq;
using TMPro;
using UnityEngine;

namespace Midas.Presentation.Meters
{
	[CreateAssetMenu(menuName = "Midas/Glyphs/TMP_Text Glyph Factory")]
	public sealed class TextMeshProGlyphFactory : GlyphFactory
	{
		private sealed class TmpTextGlyph : IGlyph
		{
			private readonly TextMeshProGlyphFactory factory;
			private readonly TMP_Text text;

			public Vector2 Size { get; private set; }
			public GameObject Obj { get; }

			public TmpTextGlyph(TextMeshProGlyphFactory factory, TMP_Text textPrefab)
			{
				this.factory = factory;
				text = Instantiate(textPrefab);
				Obj = text.gameObject;
				Obj.name = "Digit";
			}

			public void SetCharacter(char c)
			{
				Size = factory.GetGlyphSize(c);;
				text.text = c.ToString();
			}
		}

		[SerializeField]
		private TMP_Text prefab;

		[SerializeField]
		private Vector2 digitSize;

		[SerializeField]
		private Vector2 punctuationSize;

		public override IGlyph GetGlyph()
		{
			return new TmpTextGlyph(this, prefab);
		}

		public override Vector2 GetSize(string text)
		{
			var totalWidth = text.Sum(g => GetGlyphSize(g).x);
			var totalHeight = Mathf.Max(digitSize.y, punctuationSize.y);
			return new Vector2(totalWidth, totalHeight);
		}

		private Vector2 GetGlyphSize(char character)
		{
			return char.IsPunctuation(character) ? punctuationSize : digitSize;
		}
	}
}