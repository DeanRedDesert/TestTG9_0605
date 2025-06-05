// -----------------------------------------------------------------------
// <copyright file = "IAppExtensionLink.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using ExtensionBinLib.Interfaces;
    using Game.Core.Communication.Foundation.F2X;

    /// <summary>
    /// This interface defines the functionality of the Inner Link on App level.
    /// </summary>
    internal interface IAppExtensionLink : IInnerLink
    {
        /// <summary>
        /// Gets the App extension that will be activated later.
        /// There can be only one active app extension at a time.
        /// </summary>
        IExtensionIdentity ActiveAppExtension { get; }

        /// <summary>
        /// Gets the category for chooser services.
        /// </summary>
        IChooserServicesCategory ChooserServicesCategory { get; }
    }
}