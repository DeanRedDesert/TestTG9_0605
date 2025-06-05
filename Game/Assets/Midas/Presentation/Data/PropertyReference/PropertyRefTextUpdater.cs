using System.ComponentModel;
using TMPro;
using UnityEngine;

namespace Midas.Presentation.Data.PropertyReference
{
	[RequireComponent(typeof(TMP_Text))]
	public sealed class PropertyRefTextUpdater : MonoBehaviour
	{
		private TMP_Text textComponent;

		[SerializeField]
		private PropertyReferenceFormattedString propertyReference;

		private void Awake()
		{
			textComponent = GetComponent<TMP_Text>();
		}

		private void OnEnable()
		{
			if (propertyReference == null)
				return;

			propertyReference.FormattedPropertyChanged += OnFormattedPropertyChanged;
			textComponent.SetText(propertyReference.FormattedValue);
		}

		private void OnFormattedPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			textComponent.SetText(propertyReference.FormattedValue);
		}

		private void OnDisable()
		{
			propertyReference.FormattedPropertyChanged -= OnFormattedPropertyChanged;
			propertyReference.DeInit();
		}
	}
}