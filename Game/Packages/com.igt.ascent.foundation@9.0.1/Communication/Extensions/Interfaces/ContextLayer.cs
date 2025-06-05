// -----------------------------------------------------------------------
// <copyright file = "ContextLayer.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    /// <summary>
    /// This enumeration defines the layer whose context activation events
    /// are listened to by the interface extensions.
    /// </summary>
    public enum ContextLayer
    {
        /// <summary>
        /// The context of the application executable.
        /// </summary>
        Application,

        /// <summary>
        /// The context of a legacy F2L Theme.
        /// </summary>
        LegacyTheme,

        /// <summary>
        /// The context of an extension application that provides system level functionality.
        /// </summary>
        SystemExtension,

        /// <summary>
        /// The context of an extension application that provides app level functionality.
        /// </summary>
        AppExtension,

        /// <summary>
        /// The context of the Shell in a concurrent game application.
        /// </summary>
        Shell,
    }
}