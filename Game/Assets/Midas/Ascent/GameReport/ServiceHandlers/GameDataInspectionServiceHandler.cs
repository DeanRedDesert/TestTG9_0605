using IGT.Ascent.Communication.Platform.Interfaces;
using IGT.Ascent.Communication.Platform.ReportLib.Interfaces;
using IGT.Game.Core.GameReport;
using IGT.Game.Core.Logic.PaytableLoader.Interfaces;
using Midas.Ascent.Ugp;
using Midas.Core.General;

namespace Midas.Ascent.GameReport.ServiceHandlers
{
	/// <summary>
	/// This class implements the Game Data Inspection reporting service.
	/// </summary>
	public sealed class GameDataInspectionServiceHandler : GameDataInspectionServiceHandlerBase
	{
		private sealed class DummyPaytableLoader : IPaytableLoader
		{
			#region Implementation of IPaytableLoader

			public PaytableLoadResult LoadPaytable(string paytablePath, string paytableId)
			{
				return null;
			}

			#endregion
		}

		/// <summary>
		/// Constructs an instance of <see cref="GameDataInspectionServiceHandler"/>.
		/// </summary>
		/// <param name="reportLib">Game report interface to the Foundation.</param>
		public GameDataInspectionServiceHandler(IReportLib reportLib)
			: base(reportLib, new DummyPaytableLoader())
		{
		}

		/// <inheritdoc/>
		protected override IStandardReportSection CreateStandardReportSection(ReportContext context)
		{
			var progressiveSetId = GetCustomConfiguration("ProgressiveSetId", context.PaytableTag.PaytableIdentifier);
			var percentage = GetCustomConfiguration("Percentage", context.PaytableTag.PaytableIdentifier);
			var creditDenom = $"{context.Denomination}u";
			var maxBet = $"{context.MaxBet}cr";

			var ausRegFile = AusRegReader.Load(ReportLib.MountPoint);
			var gameConfiguration = AusRegReader.GetGameConfiguration(ausRegFile, progressiveSetId, percentage, maxBet, creditDenom);
			var progressives = AusRegReader.GetAllProgressiveLevels(ReportLib.MountPoint, progressiveSetId, percentage, Credit.FromLong(context.MaxBet), Money.FromMinorCurrency(context.Denomination));

			return new GameStandardSection(context, gameConfiguration, new GameProgressiveReportSection(ReportLib, context, progressives));
		}

		/// <inheritdoc/>
		protected override IProgressiveReportSection CreateProgressiveReportSection(ReportContext context)
		{
			var progressiveSetId = GetCustomConfiguration("ProgressiveSetId", context.PaytableTag.PaytableIdentifier);
			var percentage = GetCustomConfiguration("Percentage", context.PaytableTag.PaytableIdentifier);
			var progressives = AusRegReader.GetAllProgressiveLevels(ReportLib.MountPoint, progressiveSetId, percentage, Credit.FromLong(context.MaxBet), Money.FromMinorCurrency(context.Denomination));
			return progressives.Count > 0 ? new GameProgressiveReportSection(ReportLib, context, progressives) : null;
		}

		/// <inheritdoc/>
		public override void CleanUpResources(IReportLib reportLib)
		{
		}

		private string GetCustomConfiguration(string configId, string paytableTagPaytableIdentifier)
		{
			if (ReportLib.ConfigurationRead.IsConfigurationDefined(new ConfigurationItemKey(ConfigurationScope.Payvar, paytableTagPaytableIdentifier, configId)))
				return ReportLib.ConfigurationRead.GetConfiguration<string>(new ConfigurationItemKey(ConfigurationScope.Payvar, paytableTagPaytableIdentifier, configId));

			return null;
		}
	}
}