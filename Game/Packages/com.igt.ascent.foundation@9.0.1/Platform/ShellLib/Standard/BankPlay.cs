// -----------------------------------------------------------------------
// <copyright file = "BankPlay.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Standard
{
    using System;
    using Game.Core.Communication.Foundation.F2X;
    using Interfaces;
    using Platform.Interfaces;
    using Restricted.EventManagement.Interfaces;
    using F2XBetLimits = Game.Core.Communication.Foundation.F2X.Schemas.Internal.BankPlay.BetLimits;

    /// <summary>
    /// Implementation of the <see cref="IBankPlay"/> that uses
    /// F2X to communicate with the Foundation to support bank play.
    /// </summary>
    internal sealed class BankPlay : BankPlayBase
    {
        #region Private Fields

        /// <summary>
        /// The interface for the bank play category.
        /// </summary>
        private readonly CategoryInitializer<IBankPlayCategory> bankPlayCategory;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public BankPlay(object eventSender,
                        IEventDispatcher transactionalEventDispatcher,
                        IEventDispatcher nonTransactionalEventDispatcher)
            : base(eventSender, transactionalEventDispatcher, nonTransactionalEventDispatcher)
        {
            bankPlayCategory = new CategoryInitializer<IBankPlayCategory>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the instance of <see cref="BankPlay"/> whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="category">
        /// The category interface for communicating with the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is null.
        /// </exception>
        public void Initialize(IBankPlayCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            bankPlayCategory.Initialize(category);
        }

        #endregion

        #region BankPlayBase Overrides

        /// <inheritdoc/>
        public override void NewContext(IShellContext shellContext)
        {
            // Get all config data and the initial property values.

            var configData = bankPlayCategory.Instance.GetConfigData();

            SupportPlayerTransfersForBettable = configData.SupportPlayerTransfersForBettable;

            MachineWideBetConstraints =
                new MachineWideBetConstraints(configData.MachineWideBetConstraints.TokenizedAmount.Value,
                                              (MidGameMaxBetBehavior)(int)configData.MachineWideBetConstraints.MidGameMaxBetBehavior,
                                              ToPublic(configData.MachineWideBetConstraints.StartingBetLimits),
                                              ToPublic(configData.MachineWideBetConstraints.MidGameBetLimits));

            var properties = bankPlayCategory.Instance.GetBankPlayProperties();

            if(!properties.CanBetSpecified ||
               !properties.CanCommitGameCycleSpecified ||
               !properties.CashoutOfferableSpecified ||
               !properties.PlayerBettableTransferOfferableSpecified)
            {
                throw new MissingFieldException("One or more fields are missing from BankPlayProperties sent by Foundation");
            }

            BankPlayProperties = new BankPlayProperties(properties.CanBet,
                                                        properties.CanCommitGameCycle,
                                                        properties.CashoutOfferable,
                                                        properties.PlayerBettableTransferOfferable);

            var meters = bankPlayCategory.Instance.GetPlayerMeters();

            GamingMeters = new GamingMeters(meters.PlayerTransferable,
                                            meters.PlayerBettable,
                                            meters.PaidMeter);
        }

        /// <inheritdoc/>
        public override void RequestCashout()
        {
            bankPlayCategory.Instance.PlayerCashoutRequest();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Converts an F2X bet limits to a public type of bet limits.
        /// </summary>
        /// <param name="betLimits">The F2X bet limits to convert.</param>
        /// <returns>The converted public bet limits.</returns>
        private static BetLimits ToPublic(F2XBetLimits betLimits)
        {
            return new BetLimits(betLimits.MinBet.Value, betLimits.MaxBet.Value);
        }

        #endregion
    }
}