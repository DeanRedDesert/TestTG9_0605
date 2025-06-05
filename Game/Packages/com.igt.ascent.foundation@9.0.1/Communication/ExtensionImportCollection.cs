// -----------------------------------------------------------------------
// <copyright file = "ExtensionImportCollection.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// A class for storing information on the extensions to linked to the application.
    /// </summary>
    internal class ExtensionImportCollection : IExtensionImportCollection
    {
        #region Private Members

        /// <summary>
        /// A collection of <see cref="IExtensionImport"/>.
        /// </summary>
        private readonly IList<IExtensionImport> extensionImports;

        #endregion Private Members

        #region Constructors

        /// <summary>
        /// Constructs an instance of <see cref="ExtensionImportCollection"/>.
        /// </summary>
        /// <param name="extensionImports">
        /// A collection of <see cref="IExtensionImport"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="extensionImports"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="extensionImports"/> contains extension imports that have the same extension import
        /// identifier.
        /// </exception>
        public ExtensionImportCollection(IEnumerable<IExtensionImport> extensionImports)
        {
            this.extensionImports = extensionImports?.ToList() ?? throw new ArgumentNullException(nameof(extensionImports));

            if(this.extensionImports.GroupBy(extensionImport => extensionImport.ExtensionIdentifier)
                                    .Any(extensionImport => extensionImport.Count() > 1))
            {
                throw new ArgumentException("The extension imports collection contains extension imports that have the same " +
                                            "extension import identifier.");
            }
        }

        #endregion Constructors

        #region IExtensionImportCollection

        /// <inheritdoc />
        public IList<Guid> GetLinkedExtensionIdentifiers
        {
            get
            {
                return new List<Guid>(
                    extensionImports.Select(extensionImport => extensionImport.ExtensionIdentifier));
            }
        }

        /// <inheritdoc />
        public IExtensionImport GetExtensionImport(Guid extensionIdentifier)
        {
            if(extensionIdentifier == Guid.Empty)
            {
                throw new ArgumentException("The extension identifier is empty.", extensionIdentifier.ToString());
            }

            if(!IsExtensionLinked(extensionIdentifier))
            {
                throw new ArgumentException($"The extension {extensionIdentifier} is not linked.");
            }

            return extensionImports.First(extensionImport => extensionImport.ExtensionIdentifier == extensionIdentifier);
        }

        /// <inheritdoc />
        public bool IsExtensionLinked(Guid extensionIdentifier)
        {

            return extensionImports?.Any(
                       extensionImport => extensionImport.ExtensionIdentifier == extensionIdentifier) == true;
        }

        /// <inheritdoc />
        public Version GetExtensionVersion(Guid extensionIdentifier)
        {

            if(!IsExtensionLinked(extensionIdentifier))
            {
                throw new ArgumentException($"The extension {extensionIdentifier} is not linked.");
            }

            return extensionImports.Where(extensionImport => extensionImport.ExtensionIdentifier == extensionIdentifier).
                Select(extensionImport => new Version(extensionImport.ExtensionVersion.Major,
                                                      extensionImport.ExtensionVersion.Minor,
                                                      extensionImport.ExtensionVersion.Build)).First();
        }

        #endregion IExtensionImportCollection
    }
}
