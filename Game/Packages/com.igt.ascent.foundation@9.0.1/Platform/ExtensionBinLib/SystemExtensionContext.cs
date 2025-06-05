// -----------------------------------------------------------------------
// <copyright file = "SystemExtensionContext.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib
{
    using System.Collections.Generic;
    using System.Text;
    using Interfaces;

    /// <summary>
    /// A simple implementation of <see cref="ISystemExtensionContext"/>.
    /// </summary>
    internal class SystemExtensionContext : ISystemExtensionContext
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="SystemExtensionContext"/>.
        /// </summary>
        /// <param name="extensionsInService">
        /// The list of system extensions that the Extension Bin claimed to support, and “won” the right to support.
        /// </param>
        public SystemExtensionContext(IReadOnlyList<IExtensionIdentity> extensionsInService = null)
        {
            ExtensionsInService = extensionsInService ?? new List<IExtensionIdentity>();
        }

        #endregion

        #region ISystemExtensionContext Implementation

        /// <inheritdoc />
        public IReadOnlyList<IExtensionIdentity> ExtensionsInService { get; }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder()
                   .AppendLine("SystemExtensionContext -")
                   .AppendLine($"\tTotal {ExtensionsInService.Count} system extensions in service:");

            foreach(var extensionIdentity in ExtensionsInService)
            {
                builder.AppendLine($"\t\t{extensionIdentity}");
            }

            return builder.ToString();
        }

        #endregion
    }
}
