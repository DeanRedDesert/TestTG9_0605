// -----------------------------------------------------------------------
// <copyright file = "AppExtensionContext.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib
{
    using System.Text;
    using Interfaces;

    /// <summary>
    /// A simple implementation of <see cref="IAppExtensionContext"/>.
    /// </summary>
    internal class AppExtensionContext : IAppExtensionContext
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="AppExtensionContext"/>.
        /// </summary>
        /// <param name="appExtensionIdentity">
        /// The identity of the app extension in the context.
        /// </param>
        public AppExtensionContext(IExtensionIdentity appExtensionIdentity = null)
        {
            AppExtensionIdentity = appExtensionIdentity;
        }

        #endregion

        #region IAppExtensionContext Implementation

        /// <inheritdoc />
        public IExtensionIdentity AppExtensionIdentity { get; }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            return new StringBuilder()
                   .AppendLine("AppExtensionContext -")
                   .AppendLine($"\tAppExtension: {AppExtensionIdentity}")
                   .ToString();
        }

        #endregion
    }
}
