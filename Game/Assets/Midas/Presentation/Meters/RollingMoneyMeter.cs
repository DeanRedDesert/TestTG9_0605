using System.Collections.Generic;
using Midas.Core.General;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.General;
using UnityEngine;

namespace Midas.Presentation.Meters
{
	public interface IGlyph
	{
		Vector2 Size { get; }
		GameObject Obj { get; }
		void SetCharacter(char c);
	}

	public abstract class GlyphFactory : ScriptableObject
	{
		public abstract IGlyph GetGlyph();
		public abstract Vector2 GetSize(string text);
	}

	public class RollingMoneyMeter : MoneyMeter
	{
		private readonly Stack<IGlyph> glyphPool = new Stack<IGlyph>();
		private readonly List<IGlyph> activeGlyphs = new List<IGlyph>();
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();

		private Money? currentValue;

		[SerializeField]
		[Tooltip("The size of the progressive display")]
		private Vector2 size;

		[SerializeField]
		[Tooltip("True to scale horizontally only when the text is too large for the progressive region")]
		private bool scaleHorizontalOnly;

		[SerializeField]
		[Tooltip("The factory to build glyph objects with")]
		private GlyphFactory glyphFactory;

		private void OnEnable()
		{
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.CreditAndMoneyFormatter), OnFormatterChanged);
			SetValueInternal();
		}

		private void OnDisable()
		{
			autoUnregisterHelper.UnRegisterAll();
		}

		public override void SetValue(Money value)
		{
			if (currentValue.Equals(value))
				return;

			currentValue = value;
			SetValueInternal();
		}

		private void OnFormatterChanged(StatusBlock sender, string propertyname)
		{
			SetValueInternal();
		}

		private void SetValueInternal()
		{
			if (!currentValue.HasValue || StatusDatabase.ConfigurationStatus.CreditAndMoneyFormatter == null)
				return;

			ClearRenderers();

			var fraction = currentValue.Value.SubMinorCurrency.ToDouble();
			var stringValue = StatusDatabase.ConfigurationStatus.CreditAndMoneyFormatter.GetFormatted(MoneyAndCreditDisplayMode.MoneyWholePlusBase, currentValue.Value, CreditDisplaySeparatorMode.Auto);

			var totalSize = glyphFactory.GetSize(stringValue);
			var scale = CalculateScale(ref totalSize);

			// We lay out the characters least to most significant digit.

			var currentYLoc = (float)fraction * totalSize.y;
			var xLoc = 0.5f * totalSize.x;
			for (var i = stringValue.Length - 1; i >= 0; i--)
			{
				var c = stringValue[i];
				var glyph = GetGlyph(c);

				xLoc -= 0.5f * glyph.Size.x * scale.x;

				if (!char.IsDigit(c))
				{
					// Add special characters that don't scroll.

					AddDigitRenderer(xLoc, 0, scale, glyph);
				}
				else
				{
					// Add scrolling digits.

					AddDigitRenderer(xLoc, currentYLoc, scale, glyph);

					if (currentYLoc > 0)
					{
						var nextChar = c == '9' ? '0' : c + 1;

						glyph = GetGlyph((char)nextChar);
						AddDigitRenderer(xLoc, currentYLoc - totalSize.y, scale, glyph);

						if (nextChar != '0')
							currentYLoc = 0;
					}
				}

				xLoc -= 0.5f * glyph.Size.x * scale.x;
			}
		}

		private IGlyph GetGlyph(char c)
		{
			var glyph = glyphPool.Count == 0 ? glyphFactory.GetGlyph() : glyphPool.Pop();
			glyph.Obj.SetActive(true);
			glyph.SetCharacter(c);
			return glyph;
		}

		private void ClearRenderers()
		{
			foreach (var glyph in activeGlyphs)
			{
				glyph.Obj.SetActive(false);
				glyphPool.Push(glyph);
			}

			activeGlyphs.Clear();
		}

		/// <summary>
		/// Adds a digit renderer to the scene.
		/// </summary>
		private void AddDigitRenderer(float xLoc, float yLoc, Vector3 scale, IGlyph glyph)
		{
			var t = glyph.Obj.transform;
			t.SetParent(transform, false);
			t.localPosition = new Vector3(xLoc, yLoc, 0);
			t.localScale = scale;
			activeGlyphs.Add(glyph);
		}

		/// <summary>
		/// Calculate the scale required for the meter.
		/// </summary>
		private Vector3 CalculateScale(ref Vector2 totalSize)
		{
			var scale = new Vector3(1, 1, 1);

			if (totalSize.x / totalSize.y > size.x / size.y)
			{
				scale.x = size.x / totalSize.x;
				totalSize.x = size.x;

				if (scaleHorizontalOnly)
					scale.y = size.y / totalSize.y;
				else
					scale.y = scale.x;

				totalSize.y *= scale.y;
			}
			else
			{
				scale.y = size.y / totalSize.y;
				scale.x = scale.y;
				totalSize.x *= scale.x;
				totalSize.y = size.y;
			}

			return scale;
		}

		#region Editor

		private void OnDrawGizmos()
		{
			DrawGizmos(new Color(0.1f, 0.1f, 0.1f, 0.2f));
		}

		private void OnDrawGizmosSelected()
		{
			DrawGizmos(new Color(0, 0, 1, .2f));
		}

		private void DrawGizmos(Color col)
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = col;
			Gizmos.DrawCube(Vector3.zero, size);
		}

#if UNITY_EDITOR

		public void ConfigureForMakeGame(Vector2 size, bool scaleHorizontalOnly, GlyphFactory glyphFactory)
		{
			this.size = size;
			this.scaleHorizontalOnly = scaleHorizontalOnly;
			this.glyphFactory = glyphFactory;
		}

#endif

		#endregion
	}
}