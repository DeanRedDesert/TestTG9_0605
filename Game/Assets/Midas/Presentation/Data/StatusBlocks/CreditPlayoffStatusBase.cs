using Midas.Core.General;

namespace Midas.Presentation.Data.StatusBlocks
{
	public abstract class CreditPlayoffStatusBase : StatusBlock
	{
		protected CreditPlayoffStatusBase(string name) : base(name)
		{
		}

		public abstract bool IsAvailable { get; }
		public abstract bool IsPlaying { get; }
		public abstract bool IsReadyToPlay { get; }
		public abstract bool IsWin { get; }
		public abstract Money Bet { get; }
	}
}