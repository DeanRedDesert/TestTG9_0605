using System.Collections.Generic;
using Midas.Core;
using Midas.Core.ExtensionMethods;
using Midas.Core.General;
using Midas.LogicToPresentation.Data;
using Midas.LogicToPresentation.Data.Services;
using Midas.Presentation.ExtensionMethods;

namespace Midas.Presentation.Data.StatusBlocks
{
	public sealed class ProgressiveStatus : StatusBlock
	{
		private StatusProperty<IReadOnlyList<(string LevelId, Money Value)>> broadcastData;
		private StatusProperty<IReadOnlyList<ProgressiveAwardServiceData>> progressiveAwards;
		private StatusProperty<ProgressiveAwardServiceData> currentProgressiveAward;
		private StatusProperty<IReadOnlyList<ProgressiveLevel>> progressiveLevels;

		public IReadOnlyList<(string LevelId, Money Value)> BroadcastData => broadcastData.Value;

		public ProgressiveAwardServiceData CurrentProgressiveAward
		{
			get => currentProgressiveAward.Value;
			set => currentProgressiveAward.Value = value;
		}

		public IReadOnlyList<ProgressiveLevel> ProgressiveLevels => progressiveLevels.Value;
		public IReadOnlyList<ProgressiveAwardServiceData> ProgressiveAwards => progressiveAwards.Value;

		public ProgressiveStatus() : base(nameof(ProgressiveStatus))
		{
		}

		protected override void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
		{
			base.RegisterForEvents(unregisterHelper);

			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.ProgressiveService.BroadcastData, v => broadcastData.Value = v);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.ProgressiveService.ProgressiveAwards, UpdateProgressiveAwards);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.ProgressiveService.ProgressiveLevels, v => progressiveLevels.Value = v);

			void UpdateProgressiveAwards(IReadOnlyList<ProgressiveAwardServiceData> awards)
			{
				var index = progressiveAwards.Value?.FindIndex(currentProgressiveAward.Value) ?? -1;

				progressiveAwards.Value = awards;
				currentProgressiveAward.Value = index == -1 ? null : ProgressiveAwards[index];
			}
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			broadcastData = AddProperty(nameof(BroadcastData), default(IReadOnlyList<(string LevelId, Money Value)>));
			currentProgressiveAward = AddProperty(nameof(CurrentProgressiveAward), default(ProgressiveAwardServiceData));
			progressiveLevels = AddProperty(nameof(ProgressiveLevels), default(IReadOnlyList<ProgressiveLevel>));
			progressiveAwards = AddProperty(nameof(ProgressiveAwards), default(IReadOnlyList<ProgressiveAwardServiceData>));
		}
	}
}