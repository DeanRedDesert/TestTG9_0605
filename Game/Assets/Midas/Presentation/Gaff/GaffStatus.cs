using System.Collections.Generic;
using Midas.Core;
using Midas.Core.ExtensionMethods;
using Midas.Core.General;
using Midas.LogicToPresentation.Data;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.ExtensionMethods;
using UnityEngine;

namespace Midas.Presentation.Gaff
{
	public sealed class GaffStatus : StatusBlock
	{
		private static readonly Money oneHundredDollars = Money.FromMajorCurrency(100);
		private Money? denom;
		private StatusProperty<bool> isPopupActive;
		private StatusProperty<bool> isDevPanelVisible;
		private StatusProperty<Money> addCreditsAmount;
		private StatusProperty<IReadOnlyList<IGaffSequence>> gaffSequences;
		private StatusProperty<IReadOnlyList<IGaffSequence>> customGaffs;
		private StatusProperty<bool> repeatSelectedGaff;
		private StatusProperty<int?> selectedGaffIndex;
		private StatusProperty<int> selectedCustomGaffIndex;
		private StatusProperty<bool> isSelfPlayActive;
		private StatusProperty<bool> isSelfPlayAddCreditsActive;
		private StatusProperty<bool> isDialUpActive;
		private StatusProperty<float> gameTimeScale;
		private StatusProperty<bool> isFeatureFastForwarding;
		private StatusProperty<bool> isDebugEnabled;
		private StatusProperty<bool> areGaffCyclesPending;

		public bool IsPopupActive
		{
			get => isPopupActive.Value;
			set => isPopupActive.Value = value;
		}

		public Money AddCreditsAmount
		{
			get => addCreditsAmount.Value;
			set => addCreditsAmount.Value = value;
		}

		public bool RepeatSelectedGaff
		{
			get => repeatSelectedGaff.Value;
			set => repeatSelectedGaff.Value = value;
		}

		public int? SelectedGaffIndex
		{
			get => selectedGaffIndex.Value;
			set => selectedGaffIndex.Value = value;
		}

		public int SelectedCustomGaffIndex
		{
			get => selectedCustomGaffIndex.Value;
			set => selectedCustomGaffIndex.Value = value;
		}

		public bool IsSelfPlayActive
		{
			get => isSelfPlayActive.Value;
			set => isSelfPlayActive.Value = value;
		}

		public bool IsSelfPlayAddCreditsActive
		{
			get => isSelfPlayAddCreditsActive.Value;
			set => isSelfPlayAddCreditsActive.Value = value;
		}

		public bool IsDialUpActive
		{
			get => isDialUpActive.Value;
			set => isDialUpActive.Value = value;
		}

		public float GameTimeScale
		{
			get => gameTimeScale.Value;
			set => gameTimeScale.Value = value;
		}

		public bool IsFeatureFastForwarding
		{
			get => isFeatureFastForwarding.Value;
			set => isFeatureFastForwarding.Value = value;
		}

		public bool IsDebugEnabled
		{
			get => isDebugEnabled.Value;
			set => isDebugEnabled.Value = value;
		}

		public IGaffSequence SelectedGaff => SelectedGaffIndex == null ? null : GaffSequences[SelectedGaffIndex.Value];

		public IReadOnlyList<IGaffSequence> GaffSequences => gaffSequences.Value;
		public IReadOnlyList<IGaffSequence> CustomGaffs => customGaffs.Value;
		public bool AreGaffCyclesPending => areGaffCyclesPending.Value;

		public GaffStatus() : base(nameof(GaffStatus))
		{
		}

		public void ActivateCustomDialUp(IReadOnlyList<IGaffSequence> newCustomGaffs)
		{
			if (StatusDatabase.GameStatus.CurrentGameState == GameState.StartingCreditPlayoff || StatusDatabase.GameStatus.CurrentGameState == GameState.StartingGamble)
			{
				customGaffs.Value = newCustomGaffs;
				selectedCustomGaffIndex.Value = Random.Range(0, newCustomGaffs.Count);
				isPopupActive.Value = true;
			}
			else
			{
				Log.Instance.Warn("Attempt to activate custom gaff in the wrong state.");
			}
		}

		protected override void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
		{
			unregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.ConfigurationStatus, nameof(StatusDatabase.ConfigurationStatus.DenomConfig), OnDenomConfigChanged);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.GaffService.GaffSequences, OnGaffSequencesChanged);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.GaffService.AreGaffCyclesPending, v => areGaffCyclesPending.Value = v);
		}

		public override void ResetForNewGame()
		{
			base.ResetForNewGame();
			IsPopupActive = false;
			if (!RepeatSelectedGaff)
				SelectedGaffIndex = null;
		}

		private void OnGaffSequencesChanged(IReadOnlyList<IGaffSequence> v)
		{
			var selectedGaff = SelectedGaff;
			gaffSequences.Value = v;
			SelectedGaffIndex = v.FindIndex(selectedGaff);

			if (SelectedGaffIndex == -1)
			{
				SelectedGaffIndex = null;
				RepeatSelectedGaff = false;
			}
		}

		private void OnDenomConfigChanged(StatusBlock sender, string propertyname)
		{
			var newDenom = StatusDatabase.ConfigurationStatus.DenomConfig?.CurrentDenomination;
			if (!newDenom.HasValue)
				return;

			if (denom != newDenom)
			{
				denom = newDenom.Value;
				var newAddCreditsAmount = newDenom.Value * new RationalNumber(100, 1);
				if (newAddCreditsAmount < oneHundredDollars)
					newAddCreditsAmount = oneHundredDollars;

				AddCreditsAmount = newAddCreditsAmount;
			}
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			isPopupActive = AddProperty(nameof(IsPopupActive), false);
			addCreditsAmount = AddProperty(nameof(AddCreditsAmount), Money.Zero);
			gaffSequences = AddProperty(nameof(GaffSequences), default(IReadOnlyList<IGaffSequence>));
			customGaffs = AddProperty(nameof(CustomGaffs), default(IReadOnlyList<IGaffSequence>));
			repeatSelectedGaff = AddProperty(nameof(RepeatSelectedGaff), false);
			selectedGaffIndex = AddProperty<int?>(nameof(SelectedGaffIndex), null);
			selectedCustomGaffIndex = AddProperty(nameof(SelectedCustomGaffIndex), 0);
			isSelfPlayActive = AddProperty(nameof(IsSelfPlayActive), false);
			isSelfPlayAddCreditsActive = AddProperty(nameof(IsSelfPlayAddCreditsActive), false);
			isDialUpActive = AddProperty(nameof(IsDialUpActive), false);
			gameTimeScale = AddProperty(nameof(GameTimeScale), 1f);
			isFeatureFastForwarding = AddProperty(nameof(IsFeatureFastForwarding), false);
			areGaffCyclesPending = AddProperty(nameof(AreGaffCyclesPending), false);
			isDebugEnabled = AddProperty(nameof(IsDebugEnabled), false);

			denom = null;
		}
	}
}