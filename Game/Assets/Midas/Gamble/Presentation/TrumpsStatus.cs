using System.Collections.Generic;
using Midas.Core.General;
using Midas.Gamble.LogicToPresentation;
using Midas.Presentation.Data;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.Gamble;

namespace Midas.Gamble.Presentation
{
	public sealed class TrumpsStatus : GambleStatus
	{
		private StatusProperty<IReadOnlyList<TrumpsSuit>> history;
		private StatusProperty<IReadOnlyList<TrumpsCycleData>> results;
		private StatusProperty<int> currentResultIndex;
		private StatusProperty<bool> isIdleActive;
		private StatusProperty<TrumpsSelection?> selection;

		public IReadOnlyList<TrumpsSuit> History => history.Value;
		public IReadOnlyList<TrumpsCycleData> Results => results.Value;
		public int CurrentResultIndex => currentResultIndex.Value;

		public bool IsIdleActive
		{
			get => isIdleActive.Value;
			set
			{
				isIdleActive.Value = value;
				AwaitingSelection = value && Selection == null;
			}
		}

		public TrumpsSelection? Selection
		{
			get => selection.Value;
			set
			{
				selection.Value = value;
				AwaitingSelection = value == null && IsIdleActive;
			}
		}

		public TrumpsCycleData CurrentResult
		{
			get
			{
				if (Results == null || CurrentResultIndex < 0 || CurrentResultIndex >= Results.Count)
					return null;

				return Results[CurrentResultIndex];
			}
		}

		public TrumpsStatus() : base(nameof(TrumpsStatus))
		{
		}

		public override void TakeWin()
		{
			if (AwaitingSelection)
				Selection = TrumpsSelection.Decline;
		}

		protected override void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
		{
			base.RegisterForEvents(unregisterHelper);

			var trumpsService = TrumpsService.Instance;
			unregisterHelper.RegisterGameServiceChangedHandler(trumpsService.History, v => history.SetValue(v));
			unregisterHelper.RegisterGameServiceChangedHandler(trumpsService.Results, v => results.SetValue(v));
			unregisterHelper.RegisterGameServiceChangedHandler(trumpsService.CurrentResultIndex, v => currentResultIndex.SetValue(v));
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			history = AddProperty(nameof(History), default(IReadOnlyList<TrumpsSuit>));
			results = AddProperty<IReadOnlyList<TrumpsCycleData>>(nameof(Results), default);
			currentResultIndex = AddProperty(nameof(CurrentResultIndex), 0);
			isIdleActive = AddProperty(nameof(IsIdleActive), false);
			selection = AddProperty<TrumpsSelection?>(nameof(TrumpsSelection), null);
		}
	}
}