// -----------------------------------------------------------------------
// <copyright file = "CoplayerObservableCollection.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using Communication.Platform.Interfaces;
    using Interfaces;
    using ObserverPattern;

    /// <inheritdoc cref="ICoplayerObservableCollection"/>
    internal sealed class CoplayerObservableCollection : ObservableCollectionBase, ICoplayerObservableCollection
    {
        #region Private Fields

        private Observable<DisplayControlState> observableDisplayControlState;
        private Observable<bool> observableCanBetFlag;
        private Observable<bool> observableCanCommitGameCycleFlag;
        private Observable<long> observablePlayerBettableMeter;
        private Observable<string> observableCulture;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CoplayerObservableCollection"/>
        /// that only instantiates observables but does not provide any functionality.
        /// Designed to be used by LDC.
        /// </summary>
        public CoplayerObservableCollection()
        {
            CreateObservables();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CoplayerObservableCollection"/>.
        /// </summary>
        /// <param name="initData">
        /// The initial data of observables.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="initData"/> is null.
        /// </exception>
        public CoplayerObservableCollection(CoplayerInitData initData)
        {
            if(initData == null)
            {
                throw new ArgumentNullException(nameof(initData));
            }

            CreateObservables();

            // Set initial values of the observables.
            observableDisplayControlState.SetValue(initData.DisplayControlState);
            observableCanBetFlag.SetValue(initData.CanBet);
            observableCanCommitGameCycleFlag.SetValue(initData.CanCommitGameCycle);
            observablePlayerBettableMeter.SetValue(initData.PlayerBettable);
            observableCulture.SetValue(initData.Culture);
        }

        #endregion

        #region ICoplayerObservableCollection Implementation

        /// <inheritdoc/>
        public IObservable<DisplayControlState> ObservableDisplayControlState => observableDisplayControlState;

        /// <inheritdoc/>
        public IObservable<bool> ObservableCanBetFlag => observableCanBetFlag;

        /// <inheritdoc/>
        public IObservable<bool> ObservableCanCommitGameCycleFlag => observableCanCommitGameCycleFlag;

        /// <inheritdoc/>
        public IObservable<long> ObservablePlayerBettableMeter => observablePlayerBettableMeter;

        /// <inheritdoc/>
        public IObservable<string> ObservableCulture => observableCulture;

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override void Dismiss()
        {
            DismissObservables();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Handles a display control state event.
        /// </summary>
        /// <param name="eventArgs">The event data.</param>
        /// <devdoc>
        /// We don't want to subscribe to game framework runner's ShellEventReceived directly
        /// so that the handler won't be invoked unnecessarily for events like parcel comm etc.
        /// This could be changed to the event table approach if needed in the future.
        /// </devdoc>
        internal void HandleDisplayControlState(DisplayControlEventArgs eventArgs)
        {
            observableDisplayControlState.SetValue(eventArgs.DisplayControlState);
        }

        /// <summary>
        /// Handles a property relay event.
        /// </summary>
        /// <param name="eventArgs">The event data.</param>
        /// <devdoc>
        /// We don't want to subscribe to game framework runner's ShellEventReceived directly
        /// so that the handler won't be invoked unnecessarily for events like parcel comm etc.
        /// This could be changed to the event table approach if needed in the future.
        /// </devdoc>
        internal void HandlePropertyRelay(PropertyRelayEventArgs eventArgs)
        {
            switch(eventArgs.PropertyId)
            {
                case PropertyRelay.CanBetFlag:
                {
                    var canBet = ((PropertyRelayDataEventArgs<bool>)eventArgs).Data;
                    observableCanBetFlag.SetValue(canBet);
                    break;
                }
                case PropertyRelay.CanCommitGameCycleFlag:
                {
                    var canCommitGameCycle = ((PropertyRelayDataEventArgs<bool>)eventArgs).Data;
                    observableCanCommitGameCycleFlag.SetValue(canCommitGameCycle);
                    break;
                }
                case PropertyRelay.PlayerBettableMeter:
                {
                    var playerBettableMeter = ((PropertyRelayDataEventArgs<long>)eventArgs).Data;
                    observablePlayerBettableMeter.SetValue(playerBettableMeter);
                    break;
                }
                case PropertyRelay.CultureString:
                {
                    var culture = ((PropertyRelayDataEventArgs<string>)eventArgs).Data;
                    observableCulture.SetValue(culture);
                    break;
                }
            }
        }

        #endregion

        #region Private Methods

        private void CreateObservables()
        {
            observableDisplayControlState = new Observable<DisplayControlState>();
            observableCanBetFlag = new Observable<bool>();
            observableCanCommitGameCycleFlag = new Observable<bool>();
            observablePlayerBettableMeter = new Observable<long>();
            observableCulture = new Observable<string>();
        }

        private void DismissObservables()
        {
            observableDisplayControlState.Dismiss();
            observableCanBetFlag.Dismiss();
            observableCanCommitGameCycleFlag.Dismiss();
            observablePlayerBettableMeter.Dismiss();
            observableCulture.Dismiss();
        }

        #endregion
    }
}