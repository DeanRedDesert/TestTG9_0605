using System.Collections.Generic;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Gamble;
using Midas.Presentation.Game;
using Midas.Presentation.Sequencing;

namespace Midas.Gamble.Presentation
{
	public sealed class TrumpsController : IPresentationController, IPresentationNodeOwner, ISequenceOwner, IButtonControllerOwner
	{
		public static readonly SimpleSequence ShowSequence = new SimpleSequence("Trumps/ShowTrumps", TrumpsSequenceEvent.Show);
		public static readonly SimpleSequence HideSequence = new SimpleSequence("Trumps/HideTrumps", TrumpsSequenceEvent.Hide);

		private TrumpsStatus trumpsStatus;

		public IReadOnlyList<IPresentationNode> PresentationNodes { get; }
		public IReadOnlyList<Sequence> Sequences { get; }
		public IReadOnlyList<IButtonController> ButtonControllers { get; }

		public TrumpsController()
		{
			trumpsStatus = StatusDatabase.AddStatusBlock(new TrumpsStatus());

			// OfferGambleNode is a core node, but only needed if there is a gamble feature.

			PresentationNodes = new IPresentationNode[]
			{
				new OfferGambleNode(),
				new TrumpsIdleNode(),
				new TrumpsShowResultNode()
			};

			Sequences = new[]
			{
				ShowSequence,
				HideSequence
			};

			// GambleEntryButtonController is a core controller, but only needed if there is a gamble feature.

			ButtonControllers = new IButtonController[]
			{
				new GambleEntryButtonController(),
				new TrumpsButtonController()
			};
		}

		public void Init()
		{
			foreach (var sequence in Sequences)
				sequence.Init();
		}

		public void DeInit()
		{
			foreach (var sequence in Sequences)
				sequence.DeInit();
		}

		public void Destroy()
		{
			if (trumpsStatus == null)
				return;

			StatusDatabase.RemoveStatusBlock(trumpsStatus);
			trumpsStatus = null;
		}
	}
}