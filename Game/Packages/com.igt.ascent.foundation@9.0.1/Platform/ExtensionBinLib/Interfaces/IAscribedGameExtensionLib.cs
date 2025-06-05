// -----------------------------------------------------------------------
// <copyright file = "IAscribedGameExtensionLib.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    using System;

    /// <summary>
    /// This interface defines functionality available for an executable extension supporting Ascribed Games
    /// to communicate with the Foundation.
    /// </summary>
    /// <remarks>
    /// An Ascribed Game is an Ascent Game (either a Theme or a Shell) that is currently active and
    /// linked to the executable extension.
    /// </remarks>
    public interface IAscribedGameExtensionLib : IInnerLib
    {
        #region Events

        /// <summary>
        /// Occurs when an Ascribed Game Context is activated.
        /// </summary>
        event EventHandler<ActivateInnerContextEventArgs<IAscribedGameContext>> ActivateContextEvent;

        /// <summary>
        /// Occurs when an Ascribed Game Context is inactivated.
        /// </summary>
        event EventHandler<InactivateInnerContextEventArgs<IAscribedGameContext>> InactivateContextEvent;

        /// <summary>
        /// Occurs when the Ascribed Game Context changes without being inactivated first.
        /// </summary>
        /// <remarks>
        /// This event may occur only when current Ascribed Game type is Theme.
        /// </remarks>
        event EventHandler<SwitchInnerContextEventArgs<IAscribedGameContext>> SwitchContextEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current Ascribed Game Context.
        /// </summary>
        IAscribedGameContext AscribedGameContext { get; }

        #endregion
    }
}