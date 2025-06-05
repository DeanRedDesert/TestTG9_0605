using System.Collections.Generic;
using Midas.Core;
using Midas.Core.General;
using Midas.LogicToPresentation.Data;
using Midas.Presentation.ExtensionMethods;

namespace Midas.Presentation.Data.StatusBlocks
{
	public sealed class PidStatus : StatusBlock
	{
		private StatusProperty<PidConfiguration> config;
		private StatusProperty<PidSession> session;
		private StatusProperty<bool> isServiceRequested;
		private StatusProperty<double> gamesPerWin;
		private StatusProperty<double> minGameRtp;
		private StatusProperty<double> maxGameRtp;
		private StatusProperty<IReadOnlyList<(string Prize, int Odds)>> largestPrizes;
		private StatusProperty<IReadOnlyList<(string Prize, int Odds)>> smallestPrizes;

		public PidConfiguration Config => config.Value;
		public PidSession Session => session.Value;
		public bool IsServiceRequested => isServiceRequested.Value;
		public double GamesPerWin => gamesPerWin.Value;
		public double MinGameRtp => minGameRtp.Value;
		public double MaxGameRtp => maxGameRtp.Value;
		public IReadOnlyList<(string Prize, int Odds)> LargestPrizes => largestPrizes.Value;
		public IReadOnlyList<(string Prize, int Odds)> SmallestPrizes => smallestPrizes.Value;

		public PidStatus() : base(nameof(PidStatus))
		{
		}

		protected override void RegisterForEvents(AutoUnregisterHelper unregisterHelper)
		{
			base.RegisterForEvents(unregisterHelper);

			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.PidService.PidConfiguration, v => config.Value = v);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.PidService.PidSession, v => session.Value = v);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.PidService.IsServiceRequested, v => isServiceRequested.Value = v);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.PidService.GamesPerWin, v => gamesPerWin.Value = v);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.PidService.MinGameRtp, v => minGameRtp.Value = v);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.PidService.MaxGameRtp, v => maxGameRtp.Value = v);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.PidService.LargestPrizes, v => largestPrizes.Value = v);
			unregisterHelper.RegisterGameServiceChangedHandler(GameServices.PidService.SmallestPrizes, v => smallestPrizes.Value = v);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			config = AddProperty(nameof(Config), default(PidConfiguration));
			session = AddProperty(nameof(Session), default(PidSession));
			isServiceRequested = AddProperty(nameof(IsServiceRequested), default(bool));
			gamesPerWin = AddProperty(nameof(GamesPerWin), default(double));
			minGameRtp = AddProperty(nameof(MinGameRtp), default(double));
			maxGameRtp = AddProperty(nameof(MaxGameRtp), default(double));
			largestPrizes = AddProperty(nameof(LargestPrizes), default(IReadOnlyList<(string Prize, int Odds)>));
			smallestPrizes = AddProperty(nameof(SmallestPrizes), default(IReadOnlyList<(string Prize, int Odds)>));
		}
	}
}