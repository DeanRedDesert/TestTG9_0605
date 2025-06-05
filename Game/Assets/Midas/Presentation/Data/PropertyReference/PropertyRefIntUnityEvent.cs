using System;
using UnityEngine;
using UnityEngine.Events;

namespace Midas.Presentation.Data.PropertyReference
{
	public sealed class PropertyRefIntUnityEvent : MonoBehaviour
	{
		private enum Condition
		{
			Equal,
			NotEqual,
			Less,
			LessOrEqual,
			Greater,
			GreaterOrEqual
		}

		[Serializable]
		private class EventCondition
		{
			public Condition condition;
			public int value;
			public UnityEvent unityEvent;
		}

		[SerializeField]
		private PropertyReferenceValueType<int> propertyReference;

		[Tooltip("The action to take if there is no value in the property reference (ie, the parent is null).")]
		[SerializeField]
		private UnityEvent noValueEvent;

		[SerializeField]
		private EventCondition[] eventConditions;

		private void OnEnable()
		{
			if (propertyReference == null)
				return;

			propertyReference.ValueChanged += OnValueChanged;
			Refresh();
		}

		private void OnValueChanged(PropertyReference propertyRef, string path)
		{
			Refresh();
		}

		private void OnDisable()
		{
			propertyReference.ValueChanged -= OnValueChanged;
			propertyReference.DeInit();
		}

		private void Refresh()
		{
			var val = propertyReference.Value;

			if (!val.HasValue)
			{
				noValueEvent?.Invoke();
				return;
			}

			foreach (var ec in eventConditions)
			{
				var isTriggered = ec switch
				{
					{ condition: Condition.Equal } => val.Value == ec.value,
					{ condition: Condition.NotEqual } => val.Value != ec.value,
					{ condition: Condition.Less } => val.Value < ec.value,
					{ condition: Condition.LessOrEqual } => val.Value <= ec.value,
					{ condition: Condition.Greater } => val.Value > ec.value,
					{ condition: Condition.GreaterOrEqual } => val.Value >= ec.value,
					_ => false
				};

				if (isTriggered)
					ec.unityEvent?.Invoke();
			}
		}
	}
}