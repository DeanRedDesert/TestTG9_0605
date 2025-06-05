using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Midas.Presentation.Data.PropertyReference
{
	public abstract class PropertyRefBool : MonoBehaviour
	{
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		private enum NoValueAction
		{
			DoNothing,
			SetTrue,
			SetFalse
		}

		[SerializeField]
		private PropertyReferenceValueType<bool> propertyReference;

		[Tooltip("The action to take if there is no value in the property reference (ie, the parent is null).")]
		[SerializeField]
		private NoValueAction noValueAction;

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
				if (noValueAction == NoValueAction.DoNothing)
					return;

				val = noValueAction == NoValueAction.SetTrue;
			}

			Refresh(val.Value);
		}

		protected abstract void Refresh(bool value);

		#region Editor

#if UNITY_EDITOR
		public void ConfigureForMakeGame(string path)
		{
			propertyReference = new PropertyReferenceValueType<bool>(path);
		}
#endif

		#endregion
	}
}