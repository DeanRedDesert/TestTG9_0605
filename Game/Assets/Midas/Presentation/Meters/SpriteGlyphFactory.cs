using System;
using System.Linq;
using Midas.Presentation.General;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Meters
{
	[CreateAssetMenu(menuName = "Midas/Glyphs/Sprite Glyph Factory")]
	public sealed class SpriteGlyphFactory : GlyphFactory
	{
		#region Nested Types

		/// <summary>
		/// Details about an individual glyph.
		/// </summary>
		[Serializable]
		public class GlyphDetails
		{
			/// <summary>
			/// The character that the glyph represents.
			/// </summary>
			public char Character;

			/// <summary>
			/// The sprite for the glyph.
			/// </summary>
			public Sprite Sprite;
		}

		private sealed class SpriteGlyph : IGlyph
		{
			private readonly SpriteRenderer renderer;
			private readonly SpriteGlyphFactory factory;
			private GlyphDetails glyph;

			public Vector2 Size => glyph.Sprite.bounds.size;
			public GameObject Obj { get; }

			public SpriteGlyph(SpriteGlyphFactory factory, SpriteMaskInteraction maskInteraction)
			{
				this.factory = factory;
				Obj = new GameObject("Digit");
				renderer = Obj.AddComponent<SpriteRenderer>();
				renderer.maskInteraction = maskInteraction;
				factory.spriteSortingLayer.Apply(renderer);
			}

			public void SetCharacter(char c)
			{
				if (glyph?.Character == c)
					return;

				glyph = factory.GetGlyphDetails(c);
				renderer.sprite = glyph.Sprite;
			}
		}

		#endregion

		[SerializeField]
		private GlyphDetails[] glyphs;

		[SerializeField]
		private SpriteMaskInteraction maskInteraction;

		[SerializeField]
		private CustomSortingLayer spriteSortingLayer = new CustomSortingLayer(0, 0);

		private GlyphDetails GetGlyphDetails(char c)
		{
			return glyphs.First(g => g.Character == c);
		}

		public override IGlyph GetGlyph()
		{
			return new SpriteGlyph(this, maskInteraction);
		}

		public override Vector2 GetSize(string text)
		{
			var selectedGlyphs = text.Select(c => glyphs.First(d => d.Character == c)).ToList();
			var totalWidth = selectedGlyphs.Sum(g => g.Sprite.bounds.size.x);
			var totalHeight = selectedGlyphs.Max(g => g.Sprite.bounds.size.y);
			return new Vector2(totalWidth, totalHeight);
		}

#if UNITY_EDITOR
		[CustomPropertyDrawer(typeof(GlyphDetails))]
		public class GlyphDetailsEditor : PropertyDrawer
		{
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				EditorGUI.BeginProperty(position, label, property);

				var charRect = new Rect(position.x, position.y, Math.Min(position.width * 0.3f, 35.0f), position.height);
				var objectRect = new Rect(charRect.xMax, position.y, position.width - charRect.width, position.height);

				EditorGUI.PropertyField(charRect, property.FindPropertyRelative(nameof(GlyphDetails.Character)), GUIContent.none);
				EditorGUI.PropertyField(objectRect, property.FindPropertyRelative(nameof(GlyphDetails.Sprite)), GUIContent.none);

				EditorGUI.EndProperty();
			}
		}
#endif
	}
}