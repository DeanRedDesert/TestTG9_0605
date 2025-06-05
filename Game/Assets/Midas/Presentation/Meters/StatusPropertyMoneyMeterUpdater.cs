using Midas.Core.General;
using Midas.Presentation.Data.PropertyReference;
using UnityEngine;

namespace Midas.Presentation.Meters
{
	[RequireComponent(typeof(MoneyMeter))]
	public sealed class StatusPropertyMoneyMeterUpdater : MonoBehaviour
	{
		private MoneyMeter meter;

		[SerializeField]
		private PropertyReferenceValueType<Money> valueRef;

		private void Awake()
		{
			meter = GetComponent<MoneyMeter>();
		}

		private void OnEnable()
		{
			if (valueRef == null)
				return;

			valueRef.ValueChanged += OnValueChanged;
			UpdateValue();
		}

		private void OnDisable()
		{
			valueRef.DeInit();
		}

		private void OnValueChanged(PropertyReference arg1, string arg2)
		{
			UpdateValue();
		}

		private void UpdateValue()
		{
			if (valueRef.Value == null)
				return;

			meter.SetValue(valueRef.Value.Value);
		}
	}
}