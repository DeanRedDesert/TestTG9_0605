using Game.GameIdentity.Common;
using Midas.Presentation.Game;
using Midas.Presentation.Game.WinPresentation;
using Midas.Presentation.Sequencing;
using Midas.Presentation.StageHandling;

namespace Game.WinPresentation
{
	public sealed class MainWinPresNode : WinPresentationNode
	{
		private Sequence sequence;

		protected override Sequence Sequence => sequence;

		public MainWinPresNode(string nodeId, Stage stage, bool needsToSummarizeProgressiveWins = false)
			: base(nodeId, stage, needsToSummarizeProgressiveWins)
		{
		}

		public override void Init()
		{
			sequence = GameBase.GameInstance.GetPresentationController<CommonSequences>().MainWinPres;

			base.Init();
		}
	}
}