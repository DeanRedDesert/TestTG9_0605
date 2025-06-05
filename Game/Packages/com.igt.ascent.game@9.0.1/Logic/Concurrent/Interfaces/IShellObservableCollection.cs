// -----------------------------------------------------------------------
// <copyright file = "IShellObservableCollection.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;

    /// <summary>
    /// This interface defines a collection of observables provided by the Shell state machine framework.
    /// </summary>
    public interface IShellObservableCollection
    {
        /// <summary>
        /// Gets the observable of <see cref="DisplayControlState"/>.
        /// </summary>
        IObservable<DisplayControlState> ObservableDisplayControlState { get; }

        /// <summary>
        /// Gets the observable of <see cref="GamingMeters"/>.
        /// </summary>
        IObservable<GamingMeters> ObservableGamingMeters { get; }

        /// <summary>
        /// Gets the observable of <see cref="ChooserProperties"/>.
        /// </summary>
        IObservable<ChooserProperties> ObservableChooserProperties { get; }

        /// <summary>
        /// Gets the observable of <see cref="BankPlayProperties"/>.
        /// </summary>
        IObservable<BankPlayProperties> ObservableBankPlayProperties { get; }

        /// <summary>
        /// Gets the observable of the flag indicating if game is in progress.
        /// </summary>
        IObservable<bool> ObservableGameInProgress { get; }

        /// <summary>
        /// Gets the observable of the culture string.
        /// </summary>
        IObservable<string> ObservableCulture { get; }
    }
}