// -----------------------------------------------------------------------
// <copyright file = "ShellExposition.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Money;
    using Interfaces;

    /// <inheritdoc />
    /// <devdoc>
    /// This class implements both an update event and caching values for bank properties.
    /// </devdoc>
    internal sealed class ShellExposition : IShellExposition
    {
        #region Private Fields

        private bool canBet;
        private bool canCommitGameCycle;
        private long playerBettableMeter;

        #endregion

        #region Properties

        /// <summary>
        /// Gets and sets the CanBet flag which is part of <see cref="BankProperties"/>.
        /// </summary>
        internal bool CanBet
        {
            get => canBet;
            set
            {
                canBet = value;
                RaiseEvent(BankPropertiesUpdateEvent,
                           new BankPropertiesUpdateEventArgs(canBet: canBet, canCommitGameCycle: null, playerBettableMeter: null));
            }
        }

        /// <summary>
        /// Gets and sets the CanCommitGameCycle flag which is part of <see cref="BankProperties"/>.
        /// </summary>
        internal bool CanCommitGameCycle
        {
            get => canCommitGameCycle;
            set
            {
                canCommitGameCycle = value;
                RaiseEvent(BankPropertiesUpdateEvent,
                           new BankPropertiesUpdateEventArgs(canBet: null, canCommitGameCycle: canCommitGameCycle, playerBettableMeter: null));
            }
        }

        /// <summary>
        /// Gets and sets the PlayerBettableMeter which is part of <see cref="BankProperties"/>.
        /// </summary>
        internal long PlayerBettableMeter
        {
            get => playerBettableMeter;
            set
            {
                playerBettableMeter = value;
                RaiseEvent(BankPropertiesUpdateEvent,
                           new BankPropertiesUpdateEventArgs(canBet: null, canCommitGameCycle: null, playerBettableMeter: playerBettableMeter));
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ShellExposition"/>.
        /// </summary>
        /// <param name="initData">
        /// The initial data of the properties exposed to coplayers.
        /// </param>
        /// <devdoc>
        /// Internal class skips argument check.  The caller is responsible for passing in valid argument.
        /// </devdoc>
        public ShellExposition(CoplayerInitData initData)
        {
            MachineWideBetConstraints = initData.MachineWideBetConstraints;
            CreditFormatter = initData.CreditFormatter;
            MinimumBaseGameTime = initData.MinimumBaseGameTime;
            MinimumFreeSpinTime = initData.MinimumFreeSpinTime;
            CreditMeterBehavior = initData.CreditMeterBehavior;
            MaxBetButtonBehavior = initData.MaxBetButtonBehavior;
            DisplayVideoReelsForStepper = initData.DisplayVideoReelsForStepper;

            CanBet = initData.CanBet;
            CanCommitGameCycle = initData.CanCommitGameCycle;
            PlayerBettableMeter = initData.PlayerBettable;
        }

        /// <summary>
        /// Initialize a new instance of <see cref="ShellExposition"/> with the default values.
        /// </summary>
        public ShellExposition()
        {
            MachineWideBetConstraints = new MachineWideBetConstraints(1, 
                                                                      MidGameMaxBetBehavior.LimitByTotalOfStartingPlusMidGame,
                                                                      new BetLimits(100, 1), 
                                                                      new BetLimits(100, 1));
            CreditFormatter = CreditFormatter.DefaultUS;
            MinimumBaseGameTime = 0;
            MinimumFreeSpinTime = 0;
            CreditMeterBehavior = CreditMeterDisplayBehaviorMode.AlwaysCurrency;
            MaxBetButtonBehavior = MaxBetButtonBehavior.BetAvailableCredits;
            DisplayVideoReelsForStepper = false;

            CanBet = true;
            CanCommitGameCycle = true;
            PlayerBettableMeter = 0;
        }

        #endregion

        #region IShellExposition Implementation

        /// <inheritdoc />
        public MachineWideBetConstraints MachineWideBetConstraints { get; }

        /// <inheritdoc />
        public CreditFormatter CreditFormatter { get; }

        /// <inheritdoc />
        public int MinimumBaseGameTime { get; }

        /// <inheritdoc />
        public int MinimumFreeSpinTime { get; }

        /// <inheritdoc />
        public CreditMeterDisplayBehaviorMode CreditMeterBehavior { get; }

        /// <inheritdoc />
        public MaxBetButtonBehavior MaxBetButtonBehavior { get; }

        /// <inheritdoc />
        public bool DisplayVideoReelsForStepper { get; }

        /// <inheritdoc />
        public BankProperties BankProperties => new BankProperties(CanBet, CanCommitGameCycle, PlayerBettableMeter);

        /// <inheritdoc />
        public event EventHandler<BankPropertiesUpdateEventArgs> BankPropertiesUpdateEvent;

        #endregion

        #region Private Methods

        /// <summary>
        /// Raises an event.
        /// </summary>
        /// <typeparam name="TEventArgs">Type of event to raise.</typeparam>
        /// <param name="handler">The event handler that raises the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void RaiseEvent<TEventArgs>(EventHandler<TEventArgs> handler, TEventArgs eventArgs) where TEventArgs : EventArgs
        {
            handler?.Invoke(this, eventArgs);
        }

        #endregion
    }
}