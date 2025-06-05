using System.Collections.Generic;
using IGT.Ascent.Communication.Platform.ReportLib.Interfaces;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Progressive;
using IGT.Game.Core.GameReport;

namespace Midas.Ascent.GameReport
{
	/// <inheritdoc />
	public sealed class GameProgressiveReportSection : IProgressiveReportSection
	{
		#region Fields

		private readonly List<IProgressiveLevelData> progressiveLevelData;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="reportLib">The report lib to get data from.</param>
		/// <param name="reportContext">The context for the report.</param>
		/// <param name="progressives">The progressives attached for this configuration.</param>
		public GameProgressiveReportSection(IReportLib reportLib, ReportContext reportContext, IList<ProgressiveLevel> progressives)
		{
			progressiveLevelData = new List<IProgressiveLevelData>();
			for (var i = 0; i < progressives.Count; i++)
				progressiveLevelData.Add(new GameProgressiveLevel(reportLib, reportContext, progressives[i], i));
		}

		#endregion

		#region IProgressiveReportSection Members

		/// <inheritdoc />
		public IList<IProgressiveLevelData> ProgressiveLevels
		{
			get { return progressiveLevelData; }
		}

		#endregion
	}
}