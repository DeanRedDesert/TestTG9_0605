using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.General;
using Midas.LogicToPresentation;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;

namespace Midas.Presentation.Game
{
	public sealed class GameFlowStatus : StatusBlock
	{
		private readonly string presDataName;

		private StatusProperty<List<(string, TimeSpan)>> presentationNodeHistory;
		private StatusProperty<bool> autoPlayCanStart;
		private StatusProperty<IReadOnlyList<IPresentationNode>> runningNodes;

		public IReadOnlyList<(string, TimeSpan)> PresentationNodeHistory
		{
			get => presentationNodeHistory.Value;
		}

		public bool AutoPlayCanStart
		{
			get => autoPlayCanStart.Value;
			set => autoPlayCanStart.Value = value;
		}

		public IReadOnlyList<IPresentationNode> RunningNodes
		{
			get => runningNodes.Value;
			set => runningNodes.Value = value;
		}

		public GameFlowStatus() : base(nameof(GameFlowStatus))
		{
			presDataName = $"{Name}.{nameof(PresentationNodeHistory)}";
		}

		public IMessage AddToPresentationNodeHistory(IReadOnlyList<IPresentationNode> nodes)
		{
			foreach (var node in nodes)
				presentationNodeHistory.Value.Add((node.NodeId, FrameTime.CurrentTime));

			return StatusDatabase.PresentationDataStatus.UpdatePresentationData(presDataName, false, PresentationNodeHistory.ToArray());
		}

		public override void ResetForNewGame()
		{
			base.ResetForNewGame();
			presentationNodeHistory.Value.Clear();
		}

		protected override void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
		{
			base.RegisterForEvents(unregisterHelper);

			unregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.PresentationDataStatus, nameof(PresentationDataStatus.PresentationData), OnPresDataChange);
		}

		private void OnPresDataChange(StatusBlock sender, string propertyname)
		{
			if (StatusDatabase.PresentationDataStatus.PresentationData == null)
				return;

			if (StatusDatabase.PresentationDataStatus.PresentationData.TryGetValue(presDataName, out var obj))
				presentationNodeHistory.Value = ((IReadOnlyList<(string, TimeSpan)>)obj.Data).ToList();
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			presentationNodeHistory = AddProperty(nameof(PresentationNodeHistory), new List<(string, TimeSpan)>());
			autoPlayCanStart = AddProperty(nameof(AutoPlayCanStart), false);
			runningNodes = AddProperty(nameof(RunningNodes), (IReadOnlyList<IPresentationNode>)Array.Empty<IPresentationNode>());
		}
	}
}