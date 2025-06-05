using Midas.Presentation.Data.PropertyReference;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.WinPresentation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Midas.Presentation.Paylines
{
	[RequireComponent(typeof(PaylineContainer))]
	public sealed class PaylineHighlighter : MonoBehaviour
	{
		private PaylineContainer paylineContainer;

		[SerializeField]
		private PropertyReference<IWinInfo> currentWin;

		[FormerlySerializedAs("currentWinVisbilityType")]
		[SerializeField]
		private PropertyReferenceValueType<VisibilityType> currentWinVisibilityType;

		private void Awake()
		{
			paylineContainer = GetComponent<PaylineContainer>();
		}

		private void OnEnable()
		{
			if (currentWin == null)
				return;

			currentWin.ValueChanged += OnCurrentWinValueChanged;
			currentWinVisibilityType.ValueChanged += OnCurrentWinValueChanged;
			UpdateCurrentWin();
		}

		private void OnCurrentWinValueChanged(PropertyReference arg1, string arg2)
		{
			UpdateCurrentWin();
		}

		private void UpdateCurrentWin()
		{
			if (currentWin.Value != null && currentWinVisibilityType.Value == VisibilityType.Visible)
				paylineContainer.HighlightWin(currentWin.Value);
			else
				paylineContainer.HideWins();
		}

		private void OnDisable()
		{
			if (currentWin == null)
				return;

			currentWin.ValueChanged -= OnCurrentWinValueChanged;
			currentWin.DeInit();

			currentWinVisibilityType.ValueChanged -= OnCurrentWinValueChanged;
			currentWinVisibilityType.DeInit();
		}

		#region Editor

#if UNITY_EDITOR

		public void ConfigureForMakeGame(string statusItemPath)
		{
			currentWin = new PropertyReference<IWinInfo>(statusItemPath);
		}

#endif

		#endregion
	}
}