// -----------------------------------------------------------------------
// <copyright file = "NewAscribedShellContextEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using System;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event indicating the extension should change to a new AscribedShell context.
    /// </summary>
    public class NewAscribedShellContextEventArgs : AscribedShellContextEventArgs
    {
        /// <summary>
        /// Event indicating the extension should change to a new AscribedShell context.
        /// </summary>
        /// <param name="gameMode">Game mode of the AscribedShell context activation.</param>
        /// <param name="shellEntity">
        /// The <see cref="AscribedGameEntity"/> for the ascribed shell. 
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="shellEntity"/> is not an ascribed shell entity.
        /// </exception>
        public NewAscribedShellContextEventArgs(GameMode gameMode, AscribedGameEntity shellEntity = null) : base(shellEntity)
        {
            GameMode = gameMode;
        }

        /// <summary>
        /// Gets the game mode of the AscribedShell context activation.
        /// </summary>
        public GameMode GameMode { get; }
    }
}
