using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Midas.Presentation.Data.PropertyReference
{
	[Serializable]
	public class EnumUnityEvent<T> where T : Enum
	{
		[SerializeField]
		private T value;

		[SerializeField]
		private UnityEvent unityEvent;

		public T Value => value;
		public UnityEvent Event => unityEvent;

#if UNITY_EDITOR
		public void ConfigureForMakeGame(T valueToConfig, UnityEvent eventToConfig)
		{
			unityEvent = eventToConfig;
			value = valueToConfig;
		}
#endif
	}

	public class PropertyRefEnumUnityEvent<T> : MonoBehaviour where T : struct, Enum
	{
		[SerializeField]
		private PropertyReferenceValueType<T> propertyReference;

		[SerializeField]
		private EnumUnityEvent<T>[] events;

		[SerializeField]
		private UnityEvent defaultEvent;

		private void OnEnable()
		{
			if (propertyReference == null)
				return;

			propertyReference.ValueChanged += OnValueChanged;
			InvokeEvent();
		}

		private void OnValueChanged(PropertyReference propertyRef, string path)
		{
			InvokeEvent();
		}

		private void InvokeEvent()
		{
			var caseFound = false;
			foreach (var e in events)
			{
				if (e.Value.Equals(propertyReference.Value))
				{
					e.Event.Invoke();
					caseFound = true;
				}
			}

			if (!caseFound)
				defaultEvent.Invoke();
		}

		private void OnDisable()
		{
			propertyReference.ValueChanged -= OnValueChanged;
			propertyReference.DeInit();
		}

#if UNITY_EDITOR
		public void ConfigureForMakeGame(string pathToConfigure, IReadOnlyList<(object key, IReadOnlyList<(UnityAction<bool> Action, bool Value)> Actions)> mappings, IReadOnlyList<(UnityAction<bool> Action, bool Value)> defaultMappings)
		{
			propertyReference = new PropertyReferenceValueType<T>(pathToConfigure);

			var l = new List<EnumUnityEvent<T>>();
			foreach (var action in mappings)
			{
				var e = new EnumUnityEvent<T>();
				e.ConfigureForMakeGame((T)action.key, new UnityEvent());
				foreach (var ac in action.Actions)
					UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(e.Event, ac.Action, ac.Value);

				l.Add(e);
			}

			events = l.ToArray();

			defaultEvent = new UnityEvent();
			foreach (var dm in defaultMappings)
				UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(defaultEvent, dm.Action, dm.Value);
		}
#endif
	}
}