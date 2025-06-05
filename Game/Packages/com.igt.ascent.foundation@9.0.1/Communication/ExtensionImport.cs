// -----------------------------------------------------------------------
// <copyright file = "ExtensionImport.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;
    using System.Globalization;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// A class for storing information on the imported extension linked to the application.
    /// </summary>
    [Serializable]
    internal class ExtensionImport : IExtensionImport
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="ExtensionImport"/>.
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

            if(extensionVersion == null)
            {
                throw new ArgumentNullException(nameof(extensionVersion));
            }
            try
            {
                ExtensionIdentifier = new Guid(extensionIdentifier);
            }
            catch(FormatException)
            {
                throw new FormatException(
                    $"Extension Identifier: {extensionIdentifier} is not a valid GUID. Please check the format and ensure that the extension is using a valid GUID for its identifier.");
            }

            ExtensionVersion = extensionVersion;
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
            return
                $"ExtensionImport: Extension Identifier: {ExtensionIdentifier}, " + 
                $"Extension Version: {ExtensionVersion}, Resource Directory Base: {ResourceDirectoryBase}";

        }

        #endregion Object Overrides
    }
}
