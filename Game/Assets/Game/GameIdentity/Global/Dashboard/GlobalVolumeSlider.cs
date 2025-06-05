using System;
using Midas.Presentation.Data.PropertyReference;
using TMPro;
using UnityEngine;

namespace Game.GameIdentity.Global.Dashboard
{
	public sealed class GlobalVolumeSlider : MonoBehaviour
	{
		private float backgroundFillSize;

		[SerializeField]
		private PropertyReferenceValueType<float> volumePercentReference;

		[SerializeField]
		private SpriteRenderer backgroundFill;

		[SerializeField]
		private TMP_Text volumePercent;

		private void Awake()
		{
			backgroundFillSize = backgroundFill.size.y;
		}

		private void OnEnable()
		{
			if (volumePercentReference == null)
				return;

			volumePercentReference.ValueChanged += OnValueChanged;
			UpdateVolume();
		}

		private void OnDisable()
		{
			volumePercentReference.ValueChanged -= OnValueChanged;
			volumePercentReference.DeInit();
		}

		private void OnValueChanged(PropertyReference propertyRef, string path)
		{
			UpdateVolume();
		}

		private void UpdateVolume()
		{
			if (volumePercentReference.Value == null)
				return;

			var volume = volumePercentReference.Value.Value;
			backgroundFill.size = new Vector2(backgroundFill.size.x, volume * backgroundFillSize);
			volumePercent.SetText($"{Math.Round(volume * 100f, MidpointRounding.ToEven)}%");
		}
	}
}