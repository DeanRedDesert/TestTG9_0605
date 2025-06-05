// -----------------------------------------------------------------------
// <copyright file = "DisplayControlState.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// This enumeration defines how an application should display its presentation.
    /// </summary>
    public enum DisplayControlState
    {
        /// <summary>
        /// Default value is invalid.
        /// </summary>
        Invalid,

        /// <summary>
        /// The application should hide its presentation.
        /// </summary>
        DisplayAsHidden,

        /// <summary>
        /// The application should display its presentation, and enable user input.
        /// </summary>
        DisplayAsNormal,

        /// <summary>
        /// The application should display its presentation, but disable user input.
        /// </summary>
        DisplayAsSuspended
    }
}