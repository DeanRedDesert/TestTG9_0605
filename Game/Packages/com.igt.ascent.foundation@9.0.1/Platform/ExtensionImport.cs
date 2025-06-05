// -----------------------------------------------------------------------
// <copyright file = "ExtensionImport.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System;
    using System.Globalization;
    using Interfaces;

    /// <summary>
    /// A class for storing information on the imported extension linked to the application.
    /// </summary>
    /// <devdoc>
    /// Copied (and optimized) from the same named class under Foundation.Standard namespace.
    /// </devdoc>
    [Serializable]
    internal class ExtensionImport : IExtensionImport
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="ExtensionImport"/>.
        /// </summary>
        /// <param name="extensionIdentifier">
        /// The extension identifier.
        /// </param>
        /// <param name="extensionVersion">
        /// The <see cref="ExtensionVersion"/> of the extension.
        /// </param>
        /// <param name="resourceDirectoryBase">
        /// The absolute directory path to be used by extension clients as the root directory for locating resource extensions.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="extensionIdentifier"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="extensionVersion"/> is null.
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown if the <paramref name="extensionIdentifier"/> is not a valid Guid type.
        /// </exception>
        public ExtensionImport(string extensionIdentifier, Version extensionVersion, string resourceDirectoryBase)
        {
            if(string.IsNullOrEmpty(extensionIdentifier))
            {
                throw new ArgumentException("The extension identifier cannot be null or empty.", extensionIdentifier);
            }

            try
            {
                ExtensionIdentifier = new Guid(extensionIdentifier);
            }
            catch(FormatException)
            {
                throw new FormatException(
                    $"Extension Identifier: {extensionIdentifier} is not a valid GUID.  " +
                    "Please check the format and ensure that the extension is using a valid GUID for its identifier.");
            }

            ExtensionVersion = extensionVersion ?? throw new ArgumentNullException(nameof(extensionVersion));
            ResourceDirectoryBase = resourceDirectoryBase;
        }

        #endregion Constructor

        #region IExtensionImport

        /// <inheritdoc />
        public Guid ExtensionIdentifier { get; private set; }

        /// <inheritdoc />
        public Version ExtensionVersion { get; private set; }

        /// <inheritdoc />
        public string ResourceDirectoryBase { get; private set; }

        #endregion IExtensionImport

        #region Object Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "ExtensionImport: Extension Identifier: {0}, " +
                                                               "Extension Version: {1}, Resource Directory Base: {2}",
                                                               ExtensionIdentifier, ExtensionVersion, ResourceDirectoryBase);
        }

        #endregion Object Overrides
    }
}
