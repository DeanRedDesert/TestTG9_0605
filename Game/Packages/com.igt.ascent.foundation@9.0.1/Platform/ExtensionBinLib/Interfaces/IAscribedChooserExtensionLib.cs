// -----------------------------------------------------------------------
// <copyright file = "IAscribedChooserExtensionLib.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    using System;

    /// <summary>
    /// This interface defines functionality available for an executable extension supporting Ascribed Choosers
    /// to communicate with the Foundation.
    /// </summary>
    /// <remarks>
    /// An Ascribed Chooser is an Ascent Chooser that is currently active and linked to the executable extension.
    /// </remarks>
    public interface IAscribedChooserExtensionLib : IInnerLib
    {
        #region Events

        /// <summary>
        /// Occurs when an Ascribed Chooser Context is activated.
        /// </summary>
        event EventHandler<ActivateInnerContextEventArgs<IAscribedChooserContext>> ActivateContextEvent;

        /// <summary>
        /// Occurs when an Ascribed Chooser Context is inactivated.
        /// </summary>
        event EventHandler<InactivateInnerContextEventArgs<IAscribedChooserContext>> InactivateContextEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current Ascribed Chooser Context.
        /// </summary>
        IAscribedChooserContext AscribedChooserContext { get; }

        #endregion
    }
}