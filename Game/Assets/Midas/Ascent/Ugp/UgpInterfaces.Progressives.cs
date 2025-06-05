using System;
using System.Collections.Generic;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Progressive;
using static Midas.Ascent.AscentFoundation;

namespace Midas.Ascent.Ugp
{
	public sealed partial class UgpInterfaces
	{
		private IUgpProgressive progressives;
		private readonly ProgressiveLevel noProgressiveLevel = new ProgressiveLevel();

		/// <summary>
		/// Get the <see cref="IStandaloneHelperUgpProgressive"/> interface, if it exists.
		/// </summary>
		public IStandaloneHelperUgpProgressive StandaloneProgressive => progressives as IStandaloneHelperUgpProgressive;

		private void InitProgressives()
		{
			progressives = GameLib.GetInterface<IUgpProgressive>();
		}

		private void DeInitProgressives()
		{
			progressives = null;
		}

		/// <summary>
		/// Retrieves the progressive level of specified progressive ID.
		/// </summary>
		/// <param name="progressiveId">Specify the progressive ID to get.</param>
		/// <returns>The retrieved progressive level.</returns>
		public ProgressiveLevel GetProgressiveLevel(string progressiveId)
		{
			return progressives == null ? noProgressiveLevel : progressives.GetProgressiveLevel(progressiveId);
		}

		/// <summary>
		/// Retrieves all the progressive levels.
		/// </summary>
		/// <returns>A list of all the progressive levels retrieved.</returns>
		public IEnumerable<ProgressiveLevel> GetAllProgressives()
		{
			return progressives == null ? Array.Empty<ProgressiveLevel>() : progressives.GetAllProgressives();
		}
	}
}