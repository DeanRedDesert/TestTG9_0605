using Midas.Presentation.General;
using UnityEngine;

namespace Midas.Presentation.Data.PropertyReference
{
	[RequireComponent(typeof(TouchRegion))]
	public sealed class PropertyRefFloatSlider : MonoBehaviour
	{
		private TouchRegion touchRegion;

		private enum SliderOrientation
		{
			Horizontal,
			Vertical
		}

		[SerializeField]
		private float minValue;

		[SerializeField]
		private float maxValue = 1f;

		[SerializeField]
		private PropertyReferenceValueType<float> sliderPosition;

		[SerializeField]
		private SliderOrientation orientation;

		[SerializeField]
		private Transform sliderPill;

		private void Awake()
		{
			touchRegion = GetComponent<TouchRegion>();

			if (minValue > maxValue)
			{
				Log.Instance.Warn("Slider min is greater than max. Reversing.");
				(minValue, maxValue) = (maxValue, minValue);
			}
		}

		private void OnEnable()
		{
			touchRegion.OnTouch += OnTouch;
			sliderPosition.ValueChanged += OnSliderPositionChange;
			UpdateSlider();
		}

		private void OnDisable()
		{
			touchRegion.OnTouch -= OnTouch;
			sliderPosition.ValueChanged -= OnSliderPositionChange;
			sliderPosition.DeInit();
		}

		private void OnSliderPositionChange(PropertyReference propertyReference, string path)
		{
			UpdateSlider();
		}

		private void UpdateSlider()
		{
			if (!sliderPosition.Value.HasValue)
				return;

			var normalisedValue = (sliderPosition.Value.Value - minValue) / (maxValue - minValue);

			switch (orientation)
			{
				case SliderOrientation.Horizontal:
					sliderPill.localPosition = touchRegion.ValueToSliderPosHorizontal(normalisedValue);
					break;
				case SliderOrientation.Vertical:
					sliderPill.localPosition = touchRegion.ValueToSliderPosVertical(normalisedValue);
					break;
			}
		}

		private void OnTouch(Vector3 touchPoint)
		{
			var normalisedValue = orientation == SliderOrientation.Horizontal ? touchPoint.x : touchPoint.y;
			sliderPosition.Value = minValue + normalisedValue * (maxValue - minValue);
		}
	}
}