using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.Coroutine;
using TMPro;
using UnityEngine;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.ButtonHandling
{
	public abstract class DefaultButtonPresentationBase : ButtonPresentation
	{
		private static readonly TimeSpan scaleTime = TimeSpan.FromSeconds(0.1);

		private SpriteRenderer[] sprites;
		private TMP_Text[] text;
		private Color[] textColors;
		private Color[] spriteColors;
		private bool? enabledState;
		private Coroutine scaleCoroutine;

		protected abstract Color HighlightSpriteColor { get; }

		protected abstract Color HighlightTextColor { get; }

		protected abstract bool IsHighlighted { get; }

		private void Awake()
		{
			sprites = GetComponentsInChildren<SpriteRenderer>();
			text = GetComponentsInChildren<TMP_Text>();

			spriteColors = sprites.Select(s => s.color).ToArray();
			textColors = text.Select(t => t.color).ToArray();
		}

		private void OnDisable()
		{
			scaleCoroutine?.Stop();
			scaleCoroutine = null;
			transform.localScale = Vector3.one;
		}

		public override bool OnSpecificButtonDataChanged(object oldSpecificData, object newSpecificData)
		{
			return false;
		}

		public override void RefreshVisualState(Button button, ButtonStateData buttonStateData)
		{
			var isEnabled = button.ButtonState == Button.State.Hidden ? null : (bool?)(button.ButtonState == Button.State.EnabledDown || button.ButtonState == Button.State.EnabledUp);

			if (button.ButtonState == Button.State.EnabledDown || button.ButtonState == Button.State.DisabledDown)
			{
				scaleCoroutine?.Stop();
				scaleCoroutine = FrameUpdateService.Update.StartCoroutine(Scale());
			}

			enabledState = isEnabled;
			if (enabledState == null)
			{
				foreach (var s in sprites)
					s.enabled = false;
				foreach (var t in text)
					t.gameObject.SetActive(false);
			}
			else
			{
				UpdateText();
				UpdateSprites();
			}
		}

		protected virtual Color OverrideColor(SpriteRenderer spriteRenderer, Color originalColor)
		{
			return originalColor;
		}

		protected virtual Color OverrideColor(TMP_Text text, Color originalColor)
		{
			return originalColor;
		}

		private void UpdateText()
		{
			for (var i = 0; i < text.Length; i++)
			{
				var t = text[i];
				var col = IsHighlighted ? HighlightTextColor : OverrideColor(t, textColors[i]);
				if (enabledState == false)
					col *= Color.gray;
				t.gameObject.SetActive(true);
				t.color = col;
			}
		}

		private void UpdateSprites()
		{
			for (var i = 0; i < sprites.Length; i++)
			{
				var sprite = sprites[i];
				sprite.enabled = true;
				var col = IsHighlighted ? HighlightSpriteColor : OverrideColor(sprite, spriteColors[i]);
				if (enabledState == false)
					col *= Color.gray;
				sprite.color = col;
			}
		}

		private IEnumerator<CoroutineInstruction> Scale()
		{
			var t = TimeSpan.Zero;
			while (t < scaleTime)
			{
				var v = Mathf.SmoothStep(1f, 0.9f, (float)(t.TotalSeconds / scaleTime.TotalSeconds));
				transform.localScale = new Vector3(v, v, v);
				yield return null;
				t = t.Add(FrameTime.DeltaTime);
			}

			t = TimeSpan.Zero;
			while (t < scaleTime)
			{
				var v = Mathf.SmoothStep(0.9f, 1f, (float)(t.TotalSeconds / scaleTime.TotalSeconds));
				transform.localScale = new Vector3(v, v, v);
				yield return null;
				t = t.Add(FrameTime.DeltaTime);
			}

			scaleCoroutine = null;
			transform.localScale = Vector3.one;
		}
	}
}