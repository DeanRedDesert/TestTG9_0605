using System.Collections.Generic;
using Midas.Presentation.Data;

namespace Game
{
	public sealed class GameSpecificStatus : StatusBlock
	{
		private StatusProperty<IReadOnlyList<(int Column, int Row)>> preShowWinHighlight;
		private StatusProperty<IReadOnlyDictionary<string, long>> cashOnReelsTable;

		public IReadOnlyList<(int Column, int Row)> PreShowWinHighlight
		{
			get => preShowWinHighlight.Value;
			set => preShowWinHighlight.Value = value;
		}

		public IReadOnlyDictionary<string, long> CashOnReelsTable
		{
			get => cashOnReelsTable.Value;
			set => cashOnReelsTable.Value = value;
		}

		public GameSpecificStatus() : base(nameof(GameSpecificStatus))
		{
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			preShowWinHighlight = AddProperty(nameof(PreShowWinHighlight), default(IReadOnlyList<(int Column, int Row)>));
			cashOnReelsTable = AddProperty(nameof(CashOnReelsTable), default(IReadOnlyDictionary<string, long>));
		}
	}
}