using System;
using Midas.Presentation.Data.PropertyReference;
using UnityEngine;

namespace Game.GameIdentity.Global.Dashboard
{
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class StringSpriteUpdater : MonoBehaviour
	{
		[Serializable]
		private sealed class TextToSpriteMap
		{
			public string text;
			public Sprite sprite;
		}

		private SpriteRenderer spriteRenderer;

		[SerializeField]
		private PropertyReference<string> stringRef;

		[SerializeField]
		private Sprite fallbackSprite;

		[SerializeField]
		private TextToSpriteMap[] sprites;

		private void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}

		private void OnEnable()
		{
			if (stringRef == null)
				return;

			stringRef.ValueChanged += OnValueChanged;
			Refresh();
		}

		private void OnDisable()
		{
			stringRef.ValueChanged -= OnValueChanged;
			stringRef.DeInit();
		}

		private void OnValueChanged(PropertyReference propertyRef, string path)
		{
			Refresh();
		}

		private void Refresh()
		{
			var val = stringRef.Value;

			foreach (var entry in sprites)
			{
				if (entry.text == val)
				{
					spriteRenderer.sprite = entry.sprite;
					return;
				}
			}

			spriteRenderer.sprite = fallbackSprite;
		}
	}
}