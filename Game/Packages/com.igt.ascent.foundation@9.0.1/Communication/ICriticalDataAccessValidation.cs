// -----------------------------------------------------------------------
// <copyright file = "ICriticalDataAccessValidation.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This interface provides validation for accessing to the critical data.
    /// </summary>
    internal interface ICriticalDataAccessValidation
    {
        /// <summary>
        /// Checks if all the accessing to the critical data are allowed.
        /// </summary>
        /// <param name="criticalDataSelectors">The list of the critical data selectors to validate.</param>
        /// <param name="dataAccessing">The data accessing type to validate.</param>
        /// <exception cref="CriticalDataAccessDeniedException">
        /// Thrown if any of the accessing to the critical data is denied.
        /// </exception>
        void ValidateCriticalDataAccess(IList<CriticalDataSelector> criticalDataSelectors, DataAccessing dataAccessing);
    }
}
