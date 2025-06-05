// -----------------------------------------------------------------------
// <copyright file = "NewAscribedThemeContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event indicating the extension should change to a new ascribed theme context.
    /// </summary>
    public class NewAscribedThemeContextEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Event indicating the extension should change to a new ascribed theme context.
        /// </summary>
        /// <param name="payvarIdentifier">Payvar Identifier of the theme context being activated.</param>
        /// <param name="denomination">Denomination for the theme context being activated.</param>
        /// <param name="gameMode">Game mode of the theme context activation.</param>
        public NewAscribedThemeContextEventArgs(string payvarIdentifier, long denomination, GameMode gameMode)
        {
            PayvarIdentifier = payvarIdentifier;
            Denomination = denomination;
            GameMode = gameMode;
        }

        /// <summary>
        /// Gets the Payvar Identifier of the theme context being activated.
        /// </summary>
        public string PayvarIdentifier { get; }

        /// <summary>
        /// Gets the denomination for the theme context being activated.
        /// </summary>
        public long Denomination { get; }

        /// <summary>
        /// Gets the game mode of the theme context activation.
        /// </summary>
        public GameMode GameMode { get; }
    }
}
