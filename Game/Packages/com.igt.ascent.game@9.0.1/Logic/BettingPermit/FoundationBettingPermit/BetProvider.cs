//-----------------------------------------------------------------------
// <copyright file = "BetProvider.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.FoundationBettingPermit
{
    using System;
    using Communication.Platform.GameLib.Interfaces;
    using Game.Core.Logic.BetFramework;
    using Game.Core.Logic.BetFramework.Exceptions;
    using Game.Core.Logic.BetFramework.Interfaces;
    using Game.Core.Logic.Services;

    /// <summary>
    /// Generic bet provider for the BetData class.
    /// </summary>
    public class BetProvider : BetProviderBase, IGameLibEventListener
    {
        #region Game Services

        /// <summary>
        /// Get the current panel style from the bet configuration.
        /// </summary>
        /// <exception cref="InvalidBetTypeException">
        /// Thrown if <see cref="BetProviderBase.Configuration"/> isn't a <see cref="BetConfiguration"/>.
        /// </exception>
        [GameService]
        public PanelStyles PanelStyle
        {
            get
            {
                if(!(Configuration is BetConfiguration config))
                {
                    throw new InvalidBetTypeException(Configuration.GetType(), typeof(BetConfiguration));
                }
                return config.PanelStyle;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="BetProvider"/>.
        /// </summary>
        /// <param name="data"><see cref="IBetData"/> instance to expose to the presentation.</param>
        /// <param name="configuration"><see cref="IBetConfiguration"/> instance to expose to the presentation.</param>
        /// <param name="modifiers"><see cref="IBetModifiers"/> instance to expose to the presentation.</param>
        /// <param name="gameLib"><see cref="IGameLib"/> instance to expose to the presentation.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        public BetProvider(IBetData data, IBetConfiguration configuration, IBetModifiers modifiers, IGameLib gameLib)
            : base(data, configuration, modifiers, new FoundationBettingPermit(gameLib), gameLib?.GameDenomination ?? 0)
        {
            GameLib = gameLib ?? throw new ArgumentNullException(nameof(gameLib));
            GameLib.BankStatusChangedEvent += HandleBankStatusChangedEvent;
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Boolean flag indicating whether the player wager is offerable, i.e betting permitted.
        /// </summary>
        /// <remarks>
        /// This is nullable so that no additional query to the Foundation for the bank lock status
        /// is needed.  The cached value can be assigned when the bank status changed event is
        /// received.
        /// </remarks>
        private bool? isPlayerWagerOfferable;

        /// <summary>
        /// Handler for bank status events received from the Foundation.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs"><see cref="BankStatusChangedEventArgs"/> bank status changed event arguments.</param>
        private void HandleBankStatusChangedEvent(object sender, BankStatusChangedEventArgs eventArgs)
        {
            if(!isPlayerWagerOfferable.HasValue || isPlayerWagerOfferable.Value != eventArgs.Status.IsPlayerWagerOfferable)
            {
                isPlayerWagerOfferable = eventArgs.Status.IsPlayerWagerOfferable;
                UpdateModifiers();
            }
        }

        #endregion

        #region Protected Members

        /// <summary>
        /// Game lib to expose to the presentation.
        /// </summary>
        protected IGameLib GameLib { get; }

        #endregion

        #region IGameLibEventListener Implementation

        /// <inheritdoc />
        public void UnregisterGameLibEvents(IGameLib gameLib)
        {
            GameLib.BankStatusChangedEvent -= HandleBankStatusChangedEvent;
        }

        #endregion
    }
}
