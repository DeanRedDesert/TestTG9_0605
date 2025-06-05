using IGT.Ascent.Communication.Platform.ReportLib.Interfaces;
using IGT.Game.Core.GameReport;
using Midas.Ascent.GameReport.ServiceHandlers;

namespace Midas.Ascent.GameReport
{
	/// <summary>
	/// This class implements a game report object.
	/// </summary>
	// ReSharper disable once UnusedType.Global
	public class GameReport : GameReportBase
	{
		/// <inheritdoc/>
		protected override IGameDataInspectionServiceHandler CreateGameDataInspectionServiceHandler(IReportLib reportLib)
		{
			return new GameDataInspectionServiceHandler(reportLib);
		}

		/// <inheritdoc/>
		protected override IGameLevelAwardServiceHandler CreateGameLevelAwardServiceHandler(IReportLib reportLib)
		{
			return new GameLevelAwardServiceHandler();
		}

		/// <inheritdoc/>
		protected override ISetupValidationServiceHandler CreateSetupValidationServiceHandler(IReportLib reportLib)
		{
			return new GameSetupValidationServiceHandler(reportLib);
		}
	}
}