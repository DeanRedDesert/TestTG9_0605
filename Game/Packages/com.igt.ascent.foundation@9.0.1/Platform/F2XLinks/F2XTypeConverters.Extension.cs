// -----------------------------------------------------------------------
// <copyright file = "F2XTypeConverters.Extension.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System;
    using ExtensionBinLib;
    using F2XInternalTypes = Game.Core.Communication.Foundation.F2X.Schemas.Internal.Types;

    /// <summary>
    /// Collection of extension methods to help converting to and from types under <see cref="F2XInternalTypes"/> namespace.
    /// </summary>
    /// <devdoc>
    /// This can be converted to partial class if new type converters are needed.
    /// This way we can see what type is being converted by the file name.
    /// </devdoc>
    internal static class F2XTypeConverters
    {
        /// <summary>
        /// Extension method to convert <see cref="F2XInternalTypes.Extension"/> to <see cref="ExtensionIdentity"/>.
        /// </summary>
        /// <param name="extension">The type to convert.</param>
        /// <returns>The conversion result.</returns>
        internal static ExtensionIdentity ToPublic(this F2XInternalTypes.Extension extension)
        {
            return extension == null
                       ? null
                       : new ExtensionIdentity(new Guid(extension.ExtensionIdentifier),
                                               new Version((int)extension.ExtensionVersion.MajorVersion,
                                                           (int)extension.ExtensionVersion.MinorVersion,
                                                           (int)extension.ExtensionVersion.PatchVersion));
        }
    }
}