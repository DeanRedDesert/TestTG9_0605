//-----------------------------------------------------------------------
// <copyright file = "StandaloneUgpProgressive.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Progressive
{
    using System.Collections.Generic;

    /// <summary>
    /// Standalone helper interface for UGP progressive.
    /// </summary>
    public interface IStandaloneHelperUgpProgressive
    {
        /// <summary>
        /// Set the progressive levels
        /// </summary>
        /// <param name="progressiveLevels">The collection of progressive levels.</param>
        void SetProgressiveLevels(IList<ProgressiveLevel> progressiveLevels);
    }
}
