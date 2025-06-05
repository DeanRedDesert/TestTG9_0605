using System;
using System.Collections.Generic;
using IGT.Ascent.Communication.Platform.ReportLib.Interfaces;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Progressive;
using IGT.Game.Core.GameReport;
using IGT.Game.Core.GameReport.Interfaces;

namespace Midas.Ascent.GameReport
{
	/// <inheritdoc />
	public sealed class GameProgressiveLevel : IProgressiveLevelData
	{
		public const string StandaloneLinkStatus = "Standalone";
		public const string SasLinkedLinkStatus = "SAS Linked";

		#region Fields

		private readonly ProgressiveLevel progressiveLevel;
		private readonly decimal contributionPercentage;
		private readonly long maxAmount;
		private readonly long startAmount;
		private readonly decimal startPercent;
		private readonly bool isSap;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="reportLib">The report lib to get data from.</param>
		/// <param name="context">The context for the report.</param>
		/// <param name="progressiveLevel">The progressive level to represent.</param>
		/// <param name="gameLevel">The associated game level.</param>
		public GameProgressiveLevel(IReportLib reportLib, ReportContext context, ProgressiveLevel progressiveLevel, int gameLevel)
		{
			this.progressiveLevel = progressiveLevel;
			ProgressiveLevel = gameLevel;

			if (progressiveLevel == null)
				throw new ArgumentNullException(nameof(progressiveLevel), "Argument may not be null, progressiveLevel ");

			var progressiveDictionary = reportLib.GetLinkedProgressiveSettings(context.PaytableTag.PaytableIdentifier, context.Denomination);
			isSap = progressiveLevel.IsStandalone;
			if (progressiveDictionary != null)
			{
				if (progressiveDictionary.TryGetValue(gameLevel, out var settings))
				{
					maxAmount = settings.MaxAmount;
					startAmount = settings.StartAmount;
					contributionPercentage = settings.ContributionPercentage;
					// ReSharper disable once PossibleLossOfFraction # The startup is a multiple of the denom.
					startPercent = (decimal)progressiveLevel.TriggerProbability * (startAmount / context.Denomination) * 100;
				}
			}
		}

		#endregion

		#region IProgressiveLevelData Members

		/// <inheritdoc />
		public int ProgressiveLevel { get; }

		/// <inheritdoc />
		public string GetGameLevelDescription()
		{
			return progressiveLevel.Name;
		}

		/// <inheritdoc />
		public string GetLinkStatus()
		{
			return isSap ? StandaloneLinkStatus : SasLinkedLinkStatus;
		}

		/// <inheritdoc />
		public decimal GetContributionPercentage()
		{
			return isSap ? contributionPercentage : GameDataInspectionServiceHandlerBase.CheckLinkController;
		}

		/// <inheritdoc />
		public long GetMaxAmount()
		{
			return isSap ? maxAmount : GameDataInspectionServiceHandlerBase.CheckLinkController;
		}

		/// <inheritdoc />
		public long GetStartAmount()
		{
			return isSap ? startAmount : GameDataInspectionServiceHandlerBase.CheckLinkController;
		}

		/// <inheritdoc />
		public decimal GetStartPercent()
		{
			return isSap ? startPercent : GameDataInspectionServiceHandlerBase.CheckLinkController;
		}

		/// <inheritdoc />
		public ICustomReportSection CustomReportSection
		{
			get { return new CustomReportSection(new List<CustomReportItem>()); }
		}

		#endregion
	}
}