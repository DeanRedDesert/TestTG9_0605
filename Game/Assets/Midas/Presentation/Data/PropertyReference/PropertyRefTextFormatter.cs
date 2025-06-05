using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Midas.Presentation.Data.PropertyReference
{
	[RequireComponent(typeof(TMP_Text))]
	public sealed class PropertyRefTextFormatter : MonoBehaviour
	{
		private TMP_Text text;
		private bool refreshRequired;
		private string lastFormatString = null;

		[Multiline]
		[SerializeField]
		private string formatString;

		[SerializeField]
		private List<PropertyReferenceFormattedString> arguments;

		private void Awake()
		{
			text = GetComponent<TMP_Text>();
		}

		private void OnEnable()
		{
			refreshRequired = true;

			foreach (var prop in arguments)
				prop.ValueChanged += OnValueChanged;
		}

		private void OnValueChanged(PropertyReference reference, string path)
		{
			refreshRequired = true;
		}

		private void OnDisable()
		{
			foreach (var prop in arguments)
			{
				prop.ValueChanged -= OnValueChanged;
				prop.DeInit();
			}
		}

		private void Update()
		{
			if (refreshRequired || lastFormatString != formatString)
			{
				lastFormatString = formatString;
				refreshRequired = false;

				var args = arguments
					.Select(s => (object)s.FormattedValue)
					.ToArray();

				var t = string.Format(formatString, args);
				text.SetText(t);
			}
		}

#if UNITY_EDITOR
		public void ConfigureForMakeGame(IReadOnlyList<string> paths)
		{
			foreach (var path in paths)
				arguments.Add(new PropertyReferenceFormattedString(path, Array.Empty<string>()));
		}
#endif
	}
}