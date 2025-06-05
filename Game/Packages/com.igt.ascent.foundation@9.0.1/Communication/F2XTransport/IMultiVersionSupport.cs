//-----------------------------------------------------------------------
// <copyright file = "IMultiVersionSupport.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;

    /// <summary>
    /// This interface defines APIs for F2L categories
    /// which support multiple category versions in a
    /// single implementation.
    /// </summary>
    public interface IMultiVersionSupport
    {
        /// <summary>
        /// Update the version to the value accepted by Foundation
        /// during the theme level negotiation.
        /// </summary>
        /// <param name="major">The major value of the version.</param>
        /// <param name="minor">The minor value of the version.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the version to set is not supported by the category.
        /// </exception>
        void SetVersion(uint major, uint minor);
    }
}
