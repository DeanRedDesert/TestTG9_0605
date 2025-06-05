using System.Collections.Generic;
using IGT.Ascent.Communication.Platform.ReportLib.Interfaces;
using IGT.Game.Core.GameReport;

namespace Midas.Ascent.GameReport.ServiceHandlers
{
	/// <summary>
	/// This class implements the Game Level Award reporting service.
	/// </summary>
	public sealed class GameLevelAwardServiceHandler : GameLevelAwardServiceHandlerBase
	{
		/// <summary>
		/// Constructs an instance of <see cref="GameLevelAwardServiceHandler"/>.
		/// </summary>
		public GameLevelAwardServiceHandler()
		{
		}

		/// <inheritdoc/>
		protected override void InitializeGameLevelData(GameLevelDataContext gameLevelDataContext)
		{
		}

		/// <inheritdoc/>
		protected override IList<GameLevelLinkedData> AdjustGameLevelValues(GameLevelDataContext gameLevelDataContext, IList<GameLevelLinkedData> rawProgressiveData)
		{
			return rawProgressiveData;
		}

		/// <inheritdoc/>
		public override void CleanUpResources(IReportLib reportLib)
		{
		}
	}
}