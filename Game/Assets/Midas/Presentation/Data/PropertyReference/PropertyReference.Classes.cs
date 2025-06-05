using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Midas.Core.General;
using Midas.Presentation.General;
using UnityEngine;
using UnityEngine.Serialization;

namespace Midas.Presentation.Data.PropertyReference
{
	[Serializable]
	public sealed class PropertyReferenceFormattedString : PropertyReference
	{
		public const char FormatOptionsSeparator = ':';

		private (MoneyAndCreditDisplayMode DisplayMode, CreditDisplaySeparatorMode SeperatorMode) formattingOptionsMoneyAndCredits;
		private string cachedFormattedValue;
		private bool formattingOptionsResolved;
		private MonoSpacingMode monoSpacingMode;
		private float monoSpacingSize;

		[FormerlySerializedAs("_formattingOptions")]
		[SerializeField]
		private string[] formattingOptions;

		/// <summary>
		/// If the value representation is changed. Example: Currency sign changed.
		/// </summary>
		public event PropertyChangedEventHandler FormattedPropertyChanged;

		protected override Type RequiredType => typeof(object);

		public string FormattedValue
		{
			get
			{
				if (cachedFormattedValue == null)
				{
					var v = ObjectValue;

					if (!formattingOptionsResolved)
					{
						ResolveFormattingOptions();
					}

					switch (v)
					{
						case Credit credit:
							cachedFormattedValue = StatusDatabase.ConfigurationStatus?.CreditAndMoneyFormatter?.GetFormatted(
									formattingOptionsMoneyAndCredits.DisplayMode, credit, formattingOptionsMoneyAndCredits.SeperatorMode) ??
								credit.ToString();
							break;
						case Money money:
							cachedFormattedValue = StatusDatabase.ConfigurationStatus?.CreditAndMoneyFormatter?.GetFormatted(
									formattingOptionsMoneyAndCredits.DisplayMode, money, formattingOptionsMoneyAndCredits.SeperatorMode) ??
								money.ToString();
							break;
						default:
							cachedFormattedValue = v != null ? v.ToString() : "NULL";
							break;
					}
				}

				cachedFormattedValue = TextHelper.MonoSpace(cachedFormattedValue, monoSpacingSize, monoSpacingMode);

				return cachedFormattedValue;
			}
		}

		public string[] FormattingFormattingOptions
		{
			get => formattingOptions;
			set
			{
				if (formattingOptions == null || !formattingOptions.Equals(value))
				{
					formattingOptions = value;
					Reset();
				}
			}
		}

		public PropertyReferenceFormattedString()
		{
		}

		public PropertyReferenceFormattedString(string pathAndFormattingOptions)
			: base(pathAndFormattingOptions.Substring(0, pathAndFormattingOptions.IndexOf(FormatOptionsSeparator)))
		{
			var formattingOptionsString = pathAndFormattingOptions.Substring(pathAndFormattingOptions.IndexOf(FormatOptionsSeparator) + 1);
			if (formattingOptionsString.Length > 0)
			{
				formattingOptions = formattingOptionsString.Split(FormatOptionsSeparator).ToArray();
			}
		}

		public PropertyReferenceFormattedString(string path, string[] formattingFormattingOptions)
			: base(path)
		{
			formattingOptions = formattingFormattingOptions;
		}

		public override void DeInit()
		{
			base.DeInit();
			UnregisterForMoneyAndCreditFormatChanged();
		}

		protected override void OnPropertyChanged()
		{
			base.OnPropertyChanged();

			cachedFormattedValue = null;
			FormattedPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Path));
		}

		protected override void Reset()
		{
			base.Reset();

			if (formattingOptionsResolved)
			{
				UnregisterForMoneyAndCreditFormatChanged();
				formattingOptionsResolved = false;
			}

			cachedFormattedValue = null;
		}

		private void OnMoneyAndCreditFormatChangedEventHandler()
		{
			cachedFormattedValue = null;
			FormattedPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Path));
		}

		private void RegisterForMoneyAndCreditFormatChanged()
		{
			StatusDatabase.ConfigurationStatus.MoneyAndCreditFormatChanged += OnMoneyAndCreditFormatChangedEventHandler;
		}

		private void UnregisterForMoneyAndCreditFormatChanged()
		{
			StatusDatabase.ConfigurationStatus.MoneyAndCreditFormatChanged -= OnMoneyAndCreditFormatChangedEventHandler;
		}

		private void ResolveFormattingOptions()
		{
			cachedFormattedValue = null;

			if (Type == typeof(Money) || Type == typeof(Credit))
			{
				//we need to parse _formattingOptions as MoneyAndCreditDisplayMode, CreditDisplaySeparatorMode
				var displayMode = Type == typeof(Credit) ? MoneyAndCreditDisplayMode.Credit : MoneyAndCreditDisplayMode.MoneyWhole;
				var creditSeparatorMode = CreditDisplaySeparatorMode.Auto;
				if (formattingOptions.Length >= 5)
				{
					throw new ArgumentException($"Money and credit only support two formatting options (MoneyAndCreditDisplayMode:CreditDisplaySeparatorMode). Formatting options is: {string.Join(",", formattingOptions)}");
				}

				if (formattingOptions.Length >= 1 &&
					!Enum.TryParse(formattingOptions[0], out displayMode))
				{
					throw new ArgumentException($"Unsupported format option for MoneyAndCreditDisplayMode='{formattingOptions[0]}'");
				}

				if (formattingOptions.Length >= 2 &&
					!Enum.TryParse(formattingOptions[1], out creditSeparatorMode))
				{
					throw new ArgumentException($"Unsupported format option for CreditDisplaySeparatorMode='{formattingOptions[1]}'");
				}

				if (formattingOptions.Length >= 4)
				{
					if (!float.TryParse(formattingOptions[2], out monoSpacingSize))
						throw new ArgumentException($"Unsupported format option for MonoSpacingSize='{formattingOptions[2]}'");
					if (!Enum.TryParse(formattingOptions[3], out monoSpacingMode))
						throw new ArgumentException($"Unsupported format option for MonoSpacingMode='{formattingOptions[3]}'");
				}

				formattingOptionsMoneyAndCredits = (displayMode, creditSeparatorMode);
				RegisterForMoneyAndCreditFormatChanged();
			}

			formattingOptionsResolved = true;
		}

#if UNITY_EDITOR
		public override void ConfigureForMakeGame(string pathAndFormattingOptions)
		{
			base.ConfigureForMakeGame(pathAndFormattingOptions.Substring(0, pathAndFormattingOptions.IndexOf(FormatOptionsSeparator)));
			var formattingOptionsString = pathAndFormattingOptions.Substring(pathAndFormattingOptions.IndexOf(FormatOptionsSeparator) + 1);
			if (formattingOptionsString.Length > 0)
			{
				formattingOptions = formattingOptionsString.Split(FormatOptionsSeparator).ToArray();
			}
		}
#endif
	}

	[Serializable]
	public class PropertyReference<T> : PropertyReference where T : class
	{
		protected override Type RequiredType => typeof(T);

		public T Value
		{
			get => (T)ObjectValue;
			set => ObjectValue = value;
		}

		public PropertyReference(string path = "") : base(path)
		{
		}
	}

	[Serializable]
	public class PropertyReferenceValueType<T> : PropertyReference where T : struct
	{
		protected override Type RequiredType => typeof(T);

		public T? Value
		{
			get
			{
				return ObjectValue == null ? (T?)null : (T)ObjectValue;
			}

			set
			{
				ObjectValue = value;
			}
		}

		public PropertyReferenceValueType(string path = "") : base(path)
		{
		}
	}
}