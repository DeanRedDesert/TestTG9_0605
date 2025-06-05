// -----------------------------------------------------------------------
// <copyright file = "ILayeredContextActivationEventsDependency.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;

    /// <summary>
    /// This interface defines events that occur when a context at a specific layer
    /// is activated and/or inactivated.
    /// </summary>
    public interface ILayeredContextActivationEventsDependency
    {
        /// <summary>
        /// Event that occurs when a context at a specific layer is activated.
        /// </summary>
        event EventHandler<LayeredContextActivationEventArgs> ActivateLayeredContextEvent;

        /// <summary>
        /// Event that occurs when a context at a specific layer is inactivated.
        /// </summary>
        event EventHandler<LayeredContextActivationEventArgs> InactivateLayeredContextEvent;
    }
}