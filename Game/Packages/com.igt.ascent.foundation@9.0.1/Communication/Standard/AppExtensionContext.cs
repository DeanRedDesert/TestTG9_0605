// -----------------------------------------------------------------------
// <copyright file = "AppExtensionContext.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces;

    /// <inheritdoc cref="IAppExtensionContext"/>
    /// <summary>
    /// A simple implementation of <see cref="IAppExtensionContext"/>.
    /// </summary>
    internal class AppExtensionContext : IAppExtensionContext
    {
        #region Constructor

        /// <summary>
        /// Constructs an instance of <see cref="AppExtensionContext"/>.
        /// </summary>
        /// <param name="appExtensionGuid">
        /// The Guid of the app extension in the context.
        /// </param>
        public AppExtensionContext(Guid appExtensionGuid)
        {
            AppExtensionGuid = appExtensionGuid;
        }

        #endregion

        #region IAppExtensionContext Implementation

        /// <inheritdoc/>
        public Guid AppExtensionGuid { get; private set; }

        #endregion
    }
}
