// -----------------------------------------------------------------------
// <copyright file = "ICoplayerObservableCollection.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;
    using Communication.Platform.Interfaces;

    /// <summary>
    /// This interface defines a collection of observables provided by the Coplayer state machine framework.
    /// </summary>
    public interface ICoplayerObservableCollection
    {
        /// <summary>
        /// Gets the observable of <see cref="DisplayControlState"/>.
        /// </summary>
        IObservable<DisplayControlState> ObservableDisplayControlState { get; }

        /// <summary>
        /// Gets the observable of the flag indicating if if the player can bet.
        /// </summary>
        IObservable<bool> ObservableCanBetFlag { get; }

        /// <summary>
        /// Gets the observable of the flag indicating the player can commit a game-cycle.
        /// </summary>
        IObservable<bool> ObservableCanCommitGameCycleFlag { get; }

        /// <summary>
        /// Gets the observable of the player bettable meter value.
        /// </summary>
        IObservable<long> ObservablePlayerBettableMeter { get; }

        /// <summary>
        /// Gets the observable of the culture string.
        /// </summary>
        IObservable<string> ObservableCulture { get; }
    }
}