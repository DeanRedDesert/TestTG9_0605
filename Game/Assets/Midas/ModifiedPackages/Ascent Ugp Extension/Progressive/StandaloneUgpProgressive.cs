//-----------------------------------------------------------------------
// <copyright file = "StandaloneUgpProgressive.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Progressive
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;

    /// <summary>
    /// Standalone implementation of the UgpProgressive extended interface.
    /// </summary>
    internal sealed class StandaloneUgpProgressive : IUgpProgressive, IInterfaceExtension,
        IStandaloneHelperUgpProgressive
    {
        #region Fields

        private List<ProgressiveLevel> cachedProgressiveLevels = new List<ProgressiveLevel>();

        #endregion

        #region IUgpProgressive Implementation

        /// <inheritdoc/>
        public ProgressiveLevel GetProgressiveLevel(string progressiveId)
        {
            return cachedProgressiveLevels.FirstOrDefault(pl => pl.Id == progressiveId);
        }

        /// <inheritdoc/>
        public IEnumerable<ProgressiveLevel> GetAllProgressives()
        {
            return cachedProgressiveLevels;
        }

        #endregion

        #region IStandaloneHelperUgpProgressive Implementation

        /// <summary>
        /// Set and cache current progressive values. 
        /// </summary>
        /// <param name="progressiveLevels">The progressive levels.</param>
        public void SetProgressiveLevels(IList<ProgressiveLevel> progressiveLevels)
        {
            cachedProgressiveLevels = progressiveLevels.ToList();
        }

        #endregion
    }
}