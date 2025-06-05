// -----------------------------------------------------------------------
// <copyright file = "LinkControlConverters.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using F2XLinkControl = F2X.Schemas.Internal.LinkControl;

    /// <summary>
    /// Collection of extension methods helping convert between
    /// interface types and F2X LinkControl schema types.
    /// </summary>
    internal static class LinkControlConverters
    {
        /// <summary>
        /// Extension method to convert a F2X <see cref="F2XLinkControl.ExtensionImport"/> to
        /// an <see cref="ExtensionImport"/>.
        /// </summary>
        /// <param name="extensionImport">
        /// The extension import to convert.
        /// </param>
        /// <returns>
        /// The conversion result or null if <paramref name="extensionImport"/> is null.
        /// </returns>
        /// <devdoc>
        /// This method has to be internal since <see cref="ExtensionImport"/> is an internal type.
        /// </devdoc>
        internal static ExtensionImport ToPublic(this F2XLinkControl.ExtensionImport extensionImport)
        {
            if(extensionImport == null)
            {
                return null;
            }

            return new ExtensionImport(extensionImport.Extension.ExtensionIdentifier,
                                       new System.Version((int)extensionImport.Extension.ExtensionVersion.MajorVersion,
                                                          (int)extensionImport.Extension.ExtensionVersion.MinorVersion,
                                                          (int)extensionImport.Extension.ExtensionVersion.PatchVersion),
                                       extensionImport.ResourceDirectoryBase);
        }
    }
}