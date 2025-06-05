using Midas.Core.General;
using Midas.Presentation.Data;
using Midas.Presentation.Data.PropertyReference;
using Midas.Presentation.General;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.GameIdentity.Anz
{
	[RequireComponent(typeof(TMP_Text))]
	public sealed class TokenizationText : MonoBehaviour
	{
		private TMP_Text textComponent;

		[SerializeField]
		private PropertyReferenceValueType<(Money, Credit)> tokenizationPropRef;

		[SerializeField]
		private LocalizedString tokenizationText;

		private void Awake()
		{
			textComponent = GetComponent<TMP_Text>();
			SetTokenizationArguments(Money.Zero, Credit.Zero);
		}

		private void OnEnable()
		{
			tokenizationPropRef.ValueChanged += OnTokenizationValueChanged;
			tokenizationText.StringChanged += OnTokenizationTextChanged;
			OnTokenizationValueChanged(null, null);
		}

		private void OnDisable()
		{
			tokenizationPropRef.ValueChanged -= OnTokenizationValueChanged;
			tokenizationPropRef.DeInit();
			tokenizationText.StringChanged -= OnTokenizationTextChanged;
		}

		private void OnTokenizationValueChanged(PropertyReference sender, string propertyName)
		{
			var tokenizationData = tokenizationPropRef.Value;
			if (tokenizationData == null)
				return;

			SetTokenizationArguments(tokenizationData.Value.Item1, tokenizationData.Value.Item2);
			tokenizationText.RefreshString();
		}

		private void SetTokenizationArguments(Money tokenDenom, Credit creditsPerToken)
		{
			var formatter = StatusDatabase.ConfigurationStatus.CreditAndMoneyFormatter;
			tokenizationText.Arguments = new object[] { formatter?.GetFormatted(MoneyAndCreditDisplayMode.MoneyBase, tokenDenom, CreditDisplaySeparatorMode.Auto) ?? "", creditsPerToken.Credits };
		}

		private void OnTokenizationTextChanged(string value)
		{
			textComponent.text = value;
		}
	}
}