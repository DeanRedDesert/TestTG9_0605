using Midas.Core;
using Midas.Core.General;
using Midas.Presentation.Data;
using Midas.Presentation.General;
using TMPro;
using UnityEngine;

namespace Midas.Presentation.Dashboard
{
	public sealed class DenomPatchPresentation : MonoBehaviour
	{
		private IDenomBetData previousDenomBetData;
		private static readonly int denomHash = Animator.StringToHash("Denom");

		private Animator animator;

		[SerializeField]
		private TMP_Text denomText;

		[SerializeField]
		private float currencySymbolOffset;

		[SerializeField]
		private float minorCurrencySymbolOffset;

		private void Awake() => animator = GetComponent<Animator>();

		private void OnEnable()
		{
			previousDenomBetData = null;
			Update();
		}

		private void Update()
		{
			if (StatusDatabase.ConfigurationStatus == null || StatusDatabase.ConfigurationStatus.CreditAndMoneyFormatter == null)
				return;

			StatusDatabase.ConfigurationStatus.DenomBetData.TryGetValue(Money.Denomination, out var currentDenomBetData);

			if (currentDenomBetData == previousDenomBetData)
				return;

			previousDenomBetData = currentDenomBetData;
			animator.SetInteger(denomHash, (int)(currentDenomBetData?.DenomLevel ?? DenomLevel.Low));

			var cf = StatusDatabase.ConfigurationStatus.CreditAndMoneyFormatter;
			denomText.text = cf.GetFormatted(MoneyAndCreditDisplayMode.MoneyBase, Money.Denomination, CreditDisplaySeparatorMode.Auto);
		}
	}
}