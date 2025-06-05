// -----------------------------------------------------------------------
// <copyright file = "IExtensionImportCollection.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An interface for accessing information on the imported extensions linked to the application.
    /// </summary>
    public interface IExtensionImportCollection
    {
        /// <summary>
        /// Gets a list of extension identifiers linked to the application.
        /// </summary>
        /// <remarks>
        /// An empty list is returned if no extensions are linked to the application.
        /// </remarks>
        IList<Guid> GetLinkedExtensionIdentifiers { get; }

        /// <summary>
        /// Gets an <see cref="IExtensionImport"/> given its <paramref name="extensionIdentifier"/>.
        /// </summary>
        /// <param name="extensionIdentifier">
        /// The extension identifier that uniquely identifies the extension.
        /// </param>
        /// <returns>
        /// An <see cref="IExtensionImport"/> for the <paramref name="extensionIdentifier"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="extensionIdentifier"/> is empty, or if the extension is not linked to
        /// the application.
        /// </exception>
        IExtensionImport GetExtensionImport(Guid extensionIdentifier);

        /// <summary>
        /// Checks if the extension corresponding to the <paramref name="extensionIdentifier"/> is linked to the application.
        /// </summary>
        /// <param name="extensionIdentifier">
        /// The extension identifier that uniquely identifies the extension.
        /// </param>
        /// <returns>
        /// True if the extension corresponding to the <paramref name="extensionIdentifier"/> is linked to the application, and
        /// false otherwise.
        /// </returns>
        bool IsExtensionLinked(Guid extensionIdentifier);

        /// <summary>
        /// Gets a <see cref="Version"/> for the extension given its <paramref name="extensionIdentifier"/>.
        /// </summary>
        /// <remarks>
        /// Only one version of the extension is imported by the Foundation.
        /// </remarks>
        /// <param name="extensionIdentifier">
        /// The extension identifier that uniquely identifies the extension.
        /// </param>
        /// <returns>
        /// A <see cref="Version"/> for the extension given its <paramref name="extensionIdentifier"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="extensionIdentifier"/> is empty, or if the extension is not linked to
        /// the application.
        /// </exception>
        Version GetExtensionVersion(Guid extensionIdentifier);
    }
}
