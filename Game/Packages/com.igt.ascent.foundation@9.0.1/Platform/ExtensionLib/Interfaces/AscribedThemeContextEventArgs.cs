// -----------------------------------------------------------------------
// <copyright file = "AscribedThemeContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using System;
    using Platform.Interfaces;

    /// <summary>
    /// Event arguments for ascribed theme events.
    /// </summary>
    public class AscribedThemeContextEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets the identifier of the ascribed theme.
        /// </summary>
        public string AscribedThemeIdentifier { get; }

        /// <summary>
        /// Initialize a new instance of the <see cref="AscribedThemeContextEventArgs"/> class.
        /// </summary>
        /// <param name="themeEntity">
        /// The <see cref="AscribedGameEntity"/> for the ascribed theme. 
        /// Must have <see cref="AscribedGameEntity.AscribedGameType"/> of <see cref="AscribedGameType.Theme"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown is <paramref name="themeEntity"/> has the wrong ascribed game type.
        /// </exception>
        public AscribedThemeContextEventArgs(AscribedGameEntity themeEntity)
        {
            if(themeEntity.AscribedGameType != AscribedGameType.Theme)
            {
                throw new ArgumentException("Must be a theme entity.");
            }
            AscribedThemeIdentifier = themeEntity.AscribedGameIdentifier;
        }
    }
}
