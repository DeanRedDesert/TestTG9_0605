//-----------------------------------------------------------------------
// <copyright file = "ConfigurationScope.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// The enumeration defines the scope of a custom configuration item.
    /// </summary>
    [Serializable]
    public enum ConfigurationScope
    {
        /// <summary>
        /// The custom configuration is defined for a paytable.
        /// </summary>
        Payvar,

        /// <summary>
        /// The custom configuration is defined for a theme.
        /// </summary>
        Theme,

        /// <summary>
        /// The custom configuration is defined for a configuration extension.
        /// </summary>
        Extension
    }
}
