// -----------------------------------------------------------------------
// <copyright file = "IAppExtensionLib.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    using System;
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines functionality available for an App Extension to communicate with the Foundation.
    /// </summary>
    /// <remarks>
    /// An App Extension is an executable extension that acts as a presentation that the player may interact with.
    /// It can be thought of as a more flexible/generic Ascent game that does not necessarily meet the strict slot
    /// game requirements such as paytables, game cycle states, utility and history modes etc..
    /// </remarks>
    public interface IAppExtensionLib : IInnerLib
    {
        #region Events

        /// <summary>
        /// Occurs when an App Extension Context is activated.
        /// </summary>
        event EventHandler<ActivateInnerContextEventArgs<IAppExtensionContext>> ActivateContextEvent;

        /// <summary>
        /// Occurs when an App Extension Context is inactivated.
        /// </summary>
        event EventHandler<InactivateInnerContextEventArgs<IAppExtensionContext>> InactivateContextEvent;

        /// <summary>
        /// Occurs when the display control state is changed.
        /// </summary>
        event EventHandler<DisplayControlEventArgs> DisplayControlEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current App Extension Context.
        /// </summary>
        IAppExtensionContext Context { get; }

        /// <summary>
        /// Gets the current display control state of the App Extension.
        /// </summary>
        DisplayControlState DisplayControlState { get; }

        /// <summary>
        /// Gets the interface for the App Extension to check if the Chooser is currently available,
        /// and request the Chooser if needed.
        /// </summary>
        IChooserServices ChooserServices { get; }

        #endregion
    }
}