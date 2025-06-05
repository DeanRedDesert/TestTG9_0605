using Midas.Core.Debug;

namespace Midas.Ascent
{
	internal static class GameLogicTimings
	{
		public static void Reset()
		{
			BeginGame.Reset();
			CommitGame.Reset();
			CommitBet.Reset();
			EnrollGame.Reset();

			EvaluateCycle.Reset();
			GameLogicStart.Reset();
			AdjustOutcome.Reset();

			EndGame.Reset();
			FinalizeOutcome.Reset();
			GameLogicEndGame.Reset();
			EndGameCycle.Reset();
		}

		public static string TimingsString => $@"
                                       Current     Min     Max     Avg
BeginGame                            : {BeginGame.ToString(false)}
    CommitGame                       : {CommitGame.ToString(false)}
    CommitBet                        : {CommitBet.ToString(false)}
    EnrollGame                       : {EnrollGame.ToString(false)}
 
EvaluateCycle                        : {EvaluateCycle.ToString(false)}
    GameLogicStart                   : {GameLogicStart.ToString(false)}
    AdjustOutcome                    : {AdjustOutcome.ToString(false)}
 
EndGame                              : {EndGame.ToString(false)}
    FinalizeOutcome                  : {FinalizeOutcome.ToString(false)}
    GameLogicEndGame                 : {GameLogicEndGame.ToString(false)}
    EndGameCycle                     : {EndGameCycle.ToString(false)}
";

		public static readonly TimeSpanCollector BeginGame = new TimeSpanCollector();
		public static readonly TimeSpanCollector CommitGame = new TimeSpanCollector();
		public static readonly TimeSpanCollector CommitBet = new TimeSpanCollector();
		public static readonly TimeSpanCollector EnrollGame = new TimeSpanCollector();

		public static readonly TimeSpanCollector EvaluateCycle = new TimeSpanCollector();
		public static readonly TimeSpanCollector GameLogicStart = new TimeSpanCollector();
		public static readonly TimeSpanCollector AdjustOutcome = new TimeSpanCollector();

		public static readonly TimeSpanCollector EndGame = new TimeSpanCollector();
		public static readonly TimeSpanCollector FinalizeOutcome = new TimeSpanCollector();
		public static readonly TimeSpanCollector GameLogicEndGame = new TimeSpanCollector();
		public static readonly TimeSpanCollector EndGameCycle = new TimeSpanCollector();
	}
}