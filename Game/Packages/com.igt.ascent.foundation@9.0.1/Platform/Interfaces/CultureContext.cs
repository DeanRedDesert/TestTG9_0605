// -----------------------------------------------------------------------
// <copyright file = "CultureContext.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// Indicates the context in which a given culture is intended for.
    /// </summary>
    [Serializable]
    public enum CultureContext
    {
        /// <summary>
        /// Indicates a given culture is associated with a game application.
        /// </summary>
        Game,

        /// <summary>
        /// Indicates a given culture is associated with the Chooser.
        /// </summary>
        Chooser
    }
}
