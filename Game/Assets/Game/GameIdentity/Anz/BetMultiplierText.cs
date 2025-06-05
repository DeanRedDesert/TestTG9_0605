using System;
using Midas.Presentation.Data.PropertyReference;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.GameIdentity.Anz
{
	[RequireComponent(typeof(TMP_Text))]
	public sealed class BetMultiplierText : MonoBehaviour
	{
		private LocalizedString currentLocalizedString;
		private TMP_Text textComponent;

		[SerializeField]
		private LocalizedString linesAndMultiwayString;

		[SerializeField]
		private LocalizedString linesString;

		[SerializeField]
		private LocalizedString multiwayString;

		[SerializeField]
		private PropertyReferenceValueType<long> linesBetPropRef;

		[SerializeField]
		private PropertyReferenceValueType<long> multiwayBetPropRef;

		[SerializeField]
		private PropertyReferenceValueType<long> betMultiplierPropRef;

		private void Awake()
		{
			textComponent = GetComponent<TMP_Text>();
			linesAndMultiwayString.Arguments = new object[] { 0L };
			linesString.Arguments = new object[] { 0L };
			multiwayString.Arguments = new object[] { 0L };
		}

		private void OnEnable()
		{
			linesBetPropRef.ValueChanged += OnTextTypeChanged;
			multiwayBetPropRef.ValueChanged += OnTextTypeChanged;
			betMultiplierPropRef.ValueChanged += OnBetMultChanged;

			if (linesBetPropRef.Value != null && multiwayBetPropRef.Value != null)
				OnTextTypeChanged(null, null);

			if (betMultiplierPropRef.Value != null)
				OnBetMultChanged(null, null);
		}

		private void OnDisable()
		{
			linesBetPropRef.ValueChanged -= OnTextTypeChanged;
			linesBetPropRef.DeInit();
			multiwayBetPropRef.ValueChanged -= OnTextTypeChanged;
			multiwayBetPropRef.DeInit();
			betMultiplierPropRef.ValueChanged -= OnBetMultChanged;
			betMultiplierPropRef.DeInit();

			if (currentLocalizedString != null)
			{
				currentLocalizedString.StringChanged -= OnStringChanged;
				currentLocalizedString = null;
			}
		}

		private void OnTextTypeChanged(PropertyReference sender, string propName)
		{
			LocalizedString newLocalizedString;
			var linesBet = linesBetPropRef.Value ?? 0;
			var multiwayBet = multiwayBetPropRef.Value ?? 0;

			if (linesBet == 0 && multiwayBet == 0)
				return;

			// ReSharper disable once ConvertIfStatementToSwitchStatement This is more readable

			if (linesBet > 0 && multiwayBet > 0)
				newLocalizedString = linesAndMultiwayString;
			else if (linesBet > 0)
				newLocalizedString = linesString;
			else if (multiwayBet > 0)
				newLocalizedString = multiwayString;
			else
				throw new ArgumentOutOfRangeException();

			if (!ReferenceEquals(newLocalizedString, currentLocalizedString))
			{
				if (currentLocalizedString != null)
					currentLocalizedString.StringChanged -= OnStringChanged;

				newLocalizedString.StringChanged += OnStringChanged;
				currentLocalizedString = newLocalizedString;
			}

			OnBetMultChanged(null, null);
		}

		private void OnBetMultChanged(PropertyReference sender, string propName)
		{
			if (currentLocalizedString != null)
			{
				var l = betMultiplierPropRef.Value ?? 0;
				currentLocalizedString.Arguments = new object[] { l };
				currentLocalizedString.RefreshString();
			}
		}

		private void OnStringChanged(string value)
		{
			textComponent.text = value;
		}
	}
}