using System;
using Midas.Presentation.Data.PropertyReference;
using UnityEngine;
using UnityEngine.Serialization;

namespace Midas.Presentation.Dashboard
{
	public sealed class GameMessageLeftUpdater : MonoBehaviour
	{
		private GameMessageLeft lastState = GameMessageLeft.Nothing;

		[SerializeField]
		private PropertyReferenceValueType<GameMessageLeft> statePropertyReference;

		[FormerlySerializedAs("Mapping")]
		[SerializeField]
		private GameInfoLeftMapping[] mapping;

		private void OnEnable()
		{
			if (statePropertyReference == null)
				return;

			statePropertyReference.ValueChanged += OnValueChanged;
			statePropertyReference.Init();

			if (statePropertyReference.Value != null)
				DoUpdate(statePropertyReference.Value.Value);
		}

		private void OnDisable()
		{
			statePropertyReference.ValueChanged -= OnValueChanged;
			statePropertyReference.DeInit();
		}

		private void OnValueChanged(PropertyReference arg1, string arg2)
		{
			var newState = statePropertyReference.Value;

			if (newState == null || newState == lastState)
				return;

			DoUpdate(newState.Value);
		}

		private void DoUpdate(GameMessageLeft state)
		{
			lastState = state;
			foreach (var item in mapping)
			{
				if (item.Object != null)
					item.Object.SetActive(item.Value == state);
			}
		}
	}

	[Serializable]
	public sealed class GameInfoLeftMapping
	{
		public GameMessageLeft Value;
		public GameObject Object;
	}
}