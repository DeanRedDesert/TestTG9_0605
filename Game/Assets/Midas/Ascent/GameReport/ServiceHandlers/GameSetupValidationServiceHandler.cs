using System.Collections.Generic;
using IGT.Ascent.Communication.Platform.ReportLib.Interfaces;
using IGT.Game.Core.GameReport;
using IGT.Game.Core.GameReport.Interfaces;

namespace Midas.Ascent.GameReport.ServiceHandlers
{
	/// <summary>
	/// Validates the theme and progressive setup.
	/// </summary>
	public sealed class GameSetupValidationServiceHandler : ISetupValidationServiceHandler
	{
		#region Fields

		private readonly IReportLib reportLib;
		private const long TopAwardCredits = 2500;
		private const long NumberOfSubBets = 30;

		#endregion

		/// <summary>
		/// Constructs a new instance given the report lib.
		/// </summary>
		/// <param name="iReportLib">The report lib interface.</param>
		public GameSetupValidationServiceHandler(IReportLib iReportLib)
		{
			reportLib = iReportLib;
		}

		#region Implementation of IReportResourceCleanUp

		/// <inheritdoc/>
		public void CleanUpResources(IReportLib iReportLib)
		{
		}

		/// <inheritdoc/>
		public IEnumerable<SetupValidationResult> ValidateThemeSetup(SetupValidationContext setupValidationContext)
		{
			var results = new List<SetupValidationResult>();

			//var paytableDenomInfo = reportLib.GameInformation.GetEnabledPaytableDenominationInfo(setupValidationContext.ThemeIdentifier);
			//foreach (var paytableDenom in paytableDenomInfo)
			//{
			//	var progressiveInfo = reportLib.GetLinkedProgressiveSettings(paytableDenom.PaytableIdentifier, paytableDenom.Denomination);
			//	var maxBet = reportLib.GameInformation.GetMaxBet(setupValidationContext.ThemeIdentifier, paytableDenom.PaytableIdentifier, paytableDenom.Denomination);
			//	var expectedStartingAmount = GetProgressiveStartingAmount(paytableDenom.Denomination, maxBet);
			//	foreach (var progressiveSettings in progressiveInfo.Values)
			//	{
			//		if (progressiveSettings.StartAmount >= expectedStartingAmount)
			//			continue;

			//		var desc = string.Format("The starting amount must be larger than {0} credits. (Actual: {1})", expectedStartingAmount, progressiveSettings.StartAmount);
			//		results.Add(new SetupValidationResult(SetupValidationFaultType.Error, SetupValidationFaultArea.Progressive, new List<ValidationFaultLocalization> { new ValidationFaultLocalization("en-US", "Progressive Setup Invalid", desc) }));
			//	}
			//}

			return results;
		}

		#endregion

		private static long GetProgressiveStartingAmount(long denomination, long maxBet)
		{
			return TopAwardCredits * denomination * (maxBet / NumberOfSubBets);
		}
	}
}