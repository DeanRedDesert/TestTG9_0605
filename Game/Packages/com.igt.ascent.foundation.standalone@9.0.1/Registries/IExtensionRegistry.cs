//-----------------------------------------------------------------------
// <copyright file = "IExtensionRegistry.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This interface represents an extension registry object and is used to retrieve the information from
    /// the extension registry file.
    /// </summary>
    public interface IExtensionRegistry
    {
        /// <summary>
        /// Gets an <see cref="ExtensionImport"/> requested by the game.
        /// </summary>
        /// <remarks>
        /// The extension will declare which versions it supports. The game will declare which versions of an extension
        /// it wants in its theme registry. The executable extension will declare which versions of an extension it wants in
        /// its executable extension registry. The Foundation will link the game to the extension at a compatible version
        /// based on Semantic Versioning approach. The Standalone mimics the Foundation behavior. Please note that only
        /// linking between a theme registry and an extension registry is supported in Standalone (i.e. linking between an
        /// extension registry and another extension registry is not supported).
        /// </remarks>
        /// <param name="themeRegistry">
        /// The <see cref="IThemeRegistry"/> that defines information on the theme.
        /// </param>
        /// <returns>
        /// An <see cref="ExtensionImport"/> requested by the game if it is supported by this extension registry.
        /// Otherwise return null.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="themeRegistry"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the extension required by the game cannot be linked.
        /// </exception>
        IExtensionImport GetExtensionImport(IThemeRegistry themeRegistry);
    }
}
