// -----------------------------------------------------------------------
// <copyright file = "BetConfigurationBase.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework
{
    using System;
    using Exceptions;
    using Interfaces;

    /// <summary>
    /// A base implementation of <see cref="IBetConfiguration"/>.
    /// </summary>
    public class BetConfigurationBase : IBetConfiguration
    {
        #region IBetConfiguration

        /// <inheritdoc/>
        public virtual bool IsValid(IBetData betData)
        {
            if(betData == null)
            {
                throw new ArgumentNullException(nameof(betData));
            }
            var valid = betData.Enabled && betData.IsValid();
            if(valid && betData.Commit)
            {
                var total = betData.Total();
                valid = total >= MinimumWager && total <= MaximumWager;
            }
            return valid;
        }

        /// <inheritdoc/>
        public long MaximumWager { get; private set; }

        /// <inheritdoc/>
        public long MinimumWager { get; private set; }

        /// <inheritdoc/>
        public long ButtonPanelMinBet { get; private set; }

        /// <inheritdoc/>
        public long NumberOfBetItems
        {
            get
            {
                if(!numberOfBetItems.HasValue)
                {
                    throw new UninitializedVariableException("NumberOfBetItems");
                }
                return numberOfBetItems.Value;
            }
            set => numberOfBetItems = value;
        }

        /// <inheritdoc/>
        public long MaximumWagerPerBetItem
        {
            get
            {
                if(!maximumWagerPerBetItem.HasValue)
                {
                    throw new UninitializedVariableException("MaximumWagerPerBetItem");
                }
                return maximumWagerPerBetItem.Value;
            }
            set => maximumWagerPerBetItem = value;
        }

        #endregion

        #region Constructors 

        /// <summary>
        /// Initializes a new instance of <see cref="BetConfigurationBase"/>.
        /// </summary>
        /// <param name="maximumWager">
        /// The maximum wager allowed by the system.
        /// </param>
        /// <param name="minimumWager">
        /// The minimum wager allowed by the system.
        /// </param>
        /// <param name="buttonPanelMinBet">
        /// The minimum bet the button panel should be configured for.
        /// </param>
        public BetConfigurationBase(long maximumWager, long minimumWager, long buttonPanelMinBet)
        {
            Initialize(maximumWager, minimumWager, buttonPanelMinBet);
        }

        /// <summary>
        /// Default parameter-less constructor.
        /// </summary>
        protected BetConfigurationBase()
        {
        }

        /// <summary>
        /// Initializes internal fields.
        /// </summary>
        /// <param name="maximumWager">
        /// The maximum wager allowed by the system.
        /// </param>
        /// <param name="minimumWager">
        /// The minimum wager allowed by the system.
        /// </param>
        /// <param name="buttonPanelMinBet">
        /// The minimum bet the button panel should be configured for.
        /// </param>
        protected void Initialize(long maximumWager, long minimumWager, long buttonPanelMinBet)
        {
            MaximumWager = maximumWager;
            MinimumWager = minimumWager;
            ButtonPanelMinBet = buttonPanelMinBet;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The current betting style available to the player. This indicates which button map to use,
        /// and can be used to subdivide bet modifier lists to reduce validation time.
        /// </summary>
        public PanelStyles PanelStyle { get; set; }

        #endregion

        #region Private fields

        /// <summary>
        /// The number of bet items. Null if not set.
        /// </summary>
        private long? numberOfBetItems;

        /// <summary>
        /// The maximum wager per bet item. Null if not set.
        /// </summary>
        private long? maximumWagerPerBetItem;

        #endregion
    }
}