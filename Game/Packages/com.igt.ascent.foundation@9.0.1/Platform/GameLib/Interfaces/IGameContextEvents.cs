//-----------------------------------------------------------------------
// <copyright file = "IGameContextEvents.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;

    /// <summary>
    /// Interface for providing context events.
    /// </summary>
    public interface IGameContextEvents
    {
        /// <summary>
        /// Event handler of ActivateThemeContextEvent.
        /// </summary>
        event EventHandler<ActivateThemeContextEventArgs> ActivateThemeContextEvent;

        /// <summary>
        /// Event handler of InactivateThemeContextEvent.
        /// </summary>
        event EventHandler<InactivateThemeContextEventArgs> InactivateThemeContextEvent;

        /// <summary>
        /// Event Handler of SwitchThemeContextEvent.
        /// </summary>
        event EventHandler<SwitchThemeContextEventArgs> SwitchThemeContextEvent;
    }
}