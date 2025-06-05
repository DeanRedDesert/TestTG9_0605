// -----------------------------------------------------------------------
// <copyright file = "IStandalonePlayStatusDependency.cs" company = "IGT">
//     Copyright (c) 2023 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;

    /// <summary>
    /// An interface that provides various game play status information to interface extensions.
    /// </summary>
    public interface IStandalonePlayStatusDependency
    {
        /// <summary>
        /// Occurs when game-in-progress status is changed.
        /// </summary>
        event EventHandler<GameInProgressStatusEventArgs> GameInProgressStatusEvent;

        /// <summary>
        /// Occurs when money-on-machine status is changed.
        /// </summary>
        event EventHandler<MoneyOnMachineStatusEventArgs> MoneyOnMachineStatusEvent;

        /// <summary>
        /// Gets whether any game is in progress.
        /// </summary>
        /// <returns>
        /// True if any game is in progress; False otherwise.
        /// </returns>
        bool IsGameInProgress();

        /// <summary>
        /// Gets whether there is money on EGM.
        /// </summary>
        /// <returns>
        /// True if there is money on EGM; False otherwise.
        /// </returns>
        bool IsMoneyOnMachine();
    }
}