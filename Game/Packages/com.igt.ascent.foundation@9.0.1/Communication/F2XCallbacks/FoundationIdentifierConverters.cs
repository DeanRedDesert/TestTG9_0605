// -----------------------------------------------------------------------
// <copyright file = "FoundationIdentifierConverters.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using Ascent.Communication.Platform.Interfaces;
    using F2X.Schemas.Internal.Types;

    /// <summary>
    /// Collection of extension methods helping convert between
    /// the Foundation maintained identifiers and the identifier tokens.
    /// </summary>
    public static class FoundationIdentifierConverters
    {
        #region Converting between Identifier Tokens and Identifiers

        /// <summary>
        /// Extension method to convert an F2X <see cref="ThemeIdentifier"/>
        /// to a <see cref="IdentifierToken"/>.
        /// </summary>
        /// <param name="identifier">
        /// The theme identifier to convert.
        /// </param>
        /// <returns>
        /// The conversion result.
        /// </returns>
        public static IdentifierToken ToToken(this ThemeIdentifier identifier)
        {
            return new IdentifierToken(identifier.Value);
        }

        /// <summary>
        /// Extension method to convert a <see cref="IdentifierToken"/>
        /// to an F2X <see cref="ThemeIdentifier"/>.
        /// </summary>
        /// <param name="token">
        /// The identifier token to convert.
        /// </param>
        /// <returns>
        /// The conversion result.
        /// </returns>
        public static ThemeIdentifier ToThemeIdentifier(this IdentifierToken token)
        {
            return new ThemeIdentifier { Value = token.StringValue };
        }

        #endregion

        #region Converting between Strings and Identifiers

        // TODO: Remove the string versions when ready.  All public APIs should use Identifier Tokens as much as possible.
        // TODO: Possible exclusions are Extension Identifiers.

        /// <summary>
        /// Extension method to convert a string to
        /// a F2X <see cref="ThemeIdentifier"/>.
        /// </summary>
        /// <param name="identifier">The identifier to convert.</param>
        /// <returns>The conversion result.</returns>
        public static ThemeIdentifier ToThemeIdentifier(this string identifier)
        {
            return new ThemeIdentifier { Value = identifier };
        }

        /// <summary>
        /// Extension method to convert a string to
        /// a F2X <see cref="PayvarIdentifier"/>.
        /// </summary>
        /// <param name="identifier">The identifier to convert.</param>
        /// <returns>The conversion result.</returns>
        public static PayvarIdentifier ToPayvarIdentifier(this string identifier)
        {
            return new PayvarIdentifier { Value = identifier };
        }

        /// <summary>
        /// Extension method to convert a string to
        /// a F2X <see cref="ExtensionIdentifier"/>.
        /// </summary>
        /// <param name="identifier">The identifier to convert.</param>
        /// <returns>The conversion result.</returns>
        public static ExtensionIdentifier ToExtensionIdentifier(this string identifier)
        {
            return new ExtensionIdentifier { Value = identifier };
        }

        #endregion
    }
}