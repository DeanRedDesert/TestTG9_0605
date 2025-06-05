using System.Collections.Generic;
using Midas.Presentation.Game;
using Midas.Presentation.Sequencing;
using Midas.Presentation.WinPresentation;

namespace Game.GameIdentity.Common
{
	public sealed class CommonSequences : IPresentationController, ISequenceOwner
	{
		private readonly List<Sequence> sequences = new List<Sequence>();

		public WinPresSequence MainWinPres { get; private set; }
		public IReadOnlyList<Sequence> Sequences => sequences;

		public CommonSequences()
		{
			sequences.Add(MainWinPres = new DefaultWinPresSequence("GameWin/WinPres"));
		}

		public void Init()
		{
			foreach (var sequence in sequences)
				sequence.Init();
		}

		public void DeInit()
		{
			foreach (var sequence in Sequences)
				sequence.DeInit();
		}

		public void Destroy()
		{
			sequences.Clear();
			MainWinPres = null;
		}
	}
}