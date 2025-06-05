//-----------------------------------------------------------------------
// <copyright file = "EgmConfigData.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
    using F2X;

    /// <summary>
    /// Implementation of <see cref="IEgmConfigData"/> interface that requests
    /// EGM-wide data from the Foundation.
    /// </summary>
    internal class EgmConfigData : IEgmConfigData
    {
        #region Private Fields

        private readonly ITransactionVerification transactionVerification;

        private IEgmConfigDataCategory egmConfigDataCategory;

        /// <summary>
        /// Cached win cap behavior. Will be null if the value has not been cached.
        /// </summary>
        /// <remarks>The type is nullable, because the type being cached is a struct.</remarks>
        private WinCapBehaviorInfo? cachedWinCapBehaviorInfo;

        /// <summary>
        /// The cached marketing behavior. Will be null when it is not cached.
        /// </summary>
        private MarketingBehavior cachedMarketingBehavior;

        /// <summary>
        /// The cached bet selection style. Will be null when it is not cached.
        /// </summary>
        private BetSelectionStyleInfo cachedBetSelectionStyleInfo;

        /// <summary>
        /// The cached progressive win cap. Will be null when it is not cached.
        /// </summary>
        /// <remarks>
        /// Nullable type is used to indicate presence of cache. This removes ambiguity with valid values.
        /// </remarks>
        private long? cachedProgressiveWinCap;

        /// <summary>
        /// The cached total win cap. Will be null when it is not cached.
        /// </summary>
        /// <remarks>
        /// Nullable type is used to indicate presence of cache. This removes ambiguity with valid values.
        /// </remarks>
        private long? cachedTotalWinCap;

        /// <summary>
        /// The cached minimum bet. Will be null when it is not cached.
        /// </summary>
        /// <remarks>
        /// Nullable type is used to indicate presence of cache. This removes ambiguity with valid values.
        /// </remarks>
        private long? cachedMinimumBet;

        /// <summary>
        /// The cached ancillary monetary limit. Will be null when it is not cached.
        /// </summary>
        /// <remarks>
        /// Nullable type is used to indicate presence of cache. This removes ambiguity with valid values.
        /// </remarks>
        private long? cachedAncillaryMonetaryLimit;

        /// <summary>
        /// The cached value for the requirement of whether a video reels presentation should be displayed for a stepper game.
        /// </summary>
        /// <remarks>
        /// Nullable type is used to indicate presence of cache. This removes ambiguity with valid values.
        /// </remarks>
        private bool? cachedDisplayVideoReelsForStepper;

        /// <summary>
        /// The flag indicating whether <see cref="cachedBonusSoaaSettings"/> has been cached.
        /// This is needed because null is a valid value for <see cref="cachedBonusSoaaSettings"/>,
        /// e.g. when running with older Foundations.
        /// </summary>
        private bool bonusSoaaSettingsCached;

        /// <summary>
        /// The value for the Bonus Single Option Auto Advance settings.
        /// </summary>
        private BonusSoaaSettings cachedBonusSoaaSettings;

        /// <summary>
        /// The cached value for the requirement of whether higher total bets must return a higher RTP than a lesser bet.
        /// </summary>
        /// <remarks>
        /// Nullable type is used to indicate presence of cache. This removes ambiguity with valid values.
        /// </remarks>
        private bool? cachedRtpOrderedByBetRequired;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes an instance of <see cref="EgmConfigData"/>.
        /// </summary>
        /// <param name="transactionVerification">
        /// The interface for verifying that a transaction is open.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="transactionVerification"/> is null.
        /// </exception>
        public EgmConfigData(ITransactionVerification transactionVerification)
        {
            this.transactionVerification = transactionVerification ?? throw new ArgumentNullException(nameof(transactionVerification));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes class members whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="egmConfigDataHandler">
        /// The interface for communicating with the Foundation.
        /// </param>
        public void Initialize(IEgmConfigDataCategory egmConfigDataHandler)
        {
            egmConfigDataCategory = egmConfigDataHandler ?? throw new ArgumentNullException(nameof(egmConfigDataHandler));
        }

        /// <summary>
        /// Inform the EGM config data class that a new context has been activated.
        /// </summary>
        /// <remarks>This resets all cached data. Configuration data may change on context changes.</remarks>
        public void NewContext()
        {
            // NOTE: Do not query Foundation for any values in this method
            // as it does not work for Report object.
            cachedWinCapBehaviorInfo = null;
            cachedMarketingBehavior = null;
            cachedBetSelectionStyleInfo = null;
            cachedProgressiveWinCap = null;
            cachedTotalWinCap = null;
            cachedMinimumBet = null;
            cachedAncillaryMonetaryLimit = null;
            cachedDisplayVideoReelsForStepper = null;
            cachedBonusSoaaSettings = null;
            bonusSoaaSettingsCached = false;
            cachedRtpOrderedByBetRequired = null;
        }

        #endregion

        #region IEgmConfigData Members

        /// <inheritdoc/>
        public long GetMinimumBet()
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();
            if(cachedMinimumBet == null)
            {
                cachedMinimumBet = egmConfigDataCategory.GetMinimumBet().Value;
            }
            return cachedMinimumBet.Value;
        }

        /// <inheritdoc/>
        public long GetAncillaryMonetaryLimit()
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            if(cachedAncillaryMonetaryLimit == null)
            {
                cachedAncillaryMonetaryLimit = egmConfigDataCategory.GetAncillaryMonetaryLimit().Value;
            }
            return cachedAncillaryMonetaryLimit.Value;
        }

        /// <inheritdoc/>
        public WinCapBehaviorInfo GetWinCapBehaviorInfo()
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            if(cachedWinCapBehaviorInfo == null)
            {
                var replyContent = egmConfigDataCategory.GetWinCapBehavior();
                cachedWinCapBehaviorInfo = new WinCapBehaviorInfo((WinCapBehavior)replyContent.WinCapBehavior,
                                                                  replyContent.WinCapAmount?.Value ?? 0,
                                                                  replyContent.WinCapMultiplier);
            }

            return cachedWinCapBehaviorInfo.Value;
        }

        /// <inheritdoc/>
        public MarketingBehavior GetMarketingBehavior()
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            return cachedMarketingBehavior ?? (cachedMarketingBehavior = new MarketingBehavior
            {
                TopScreenGameAdvertisement =
                    (TopScreenGameAdvertisementType)egmConfigDataCategory.GetMarketingBehavior()
            });
        }

        /// <inheritdoc/>
        public BetSelectionStyleInfo GetDefaultBetSelectionStyle()
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            if(cachedBetSelectionStyleInfo == null)
            {
                var style = egmConfigDataCategory.GetConfigDataDefaultBetSelectionStyle();

                // There is a bug in H-Series Foundation (AF-2711) where the returned DefaultBetSelectionStyle
                // object is null. This was fixed in I-Series, but will not be fixed in H. So we handle it
                // here by returning a BetSelectionStyleInfo object using the default constructor.
                if(style != null)
                {
                    cachedBetSelectionStyleInfo = new BetSelectionStyleInfo((BetSelectionBehavior)style.NumberOfSubsets,
                                                                            (BetSelectionBehavior)style.BetPerSubset,
                                                                            (SideBetSelectionBehavior)style.SideBet);
                }
                else
                {
                    cachedBetSelectionStyleInfo = new BetSelectionStyleInfo();
                }
            }

            return cachedBetSelectionStyleInfo;
        }

        /// <inheritdoc/>
        public long GetProgressiveWinCap()
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            if(cachedProgressiveWinCap == null)
            {
                var amount = egmConfigDataCategory.GetConfigDataProgressiveWinCap();
                cachedProgressiveWinCap = amount?.Value ?? 0;
            }

            return cachedProgressiveWinCap.Value;
        }

        /// <inheritdoc/>
        public long GetTotalWinCap()
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            if(cachedTotalWinCap == null)
            {
                var amount = egmConfigDataCategory.GetConfigDataTotalWinCap();
                cachedTotalWinCap = amount?.Value ?? 0;
            }

            return cachedTotalWinCap.Value;
        }

        /// <inheritdoc />
        public bool GetDisplayVideoReelsForStepper()
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            if(cachedDisplayVideoReelsForStepper == null)
            {
                cachedDisplayVideoReelsForStepper = egmConfigDataCategory.GetConfigDataDisplayVideoReelsForStepper();
            }

            return cachedDisplayVideoReelsForStepper.Value;
        }

        /// <inheritdoc />
        public BonusSoaaSettings GetBonusSoaaSettings()
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            if(!bonusSoaaSettingsCached)
            {
                var settings = egmConfigDataCategory.GetConfigDataGameFeatureSingleOptionAutoAdvanceSettings();

                cachedBonusSoaaSettings = settings == null
                                              ? null
                                              : new BonusSoaaSettings(settings.GameFeatureSingleOptionAutoAdvanceAllowed,
                                                                      settings.GameFeatureSingleOptionAutoAdvanceMinDelaySpecified
                                                                          ? settings.GameFeatureSingleOptionAutoAdvanceMinDelay
                                                                          : (uint?)null);

                bonusSoaaSettingsCached = true;
            }

            return cachedBonusSoaaSettings;
        }

        /// <inheritdoc />
        public bool GetRtpOrderedByBetRequired()
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();

            if(cachedRtpOrderedByBetRequired == null)
            {
                cachedRtpOrderedByBetRequired = egmConfigDataCategory.GetConfigDataRtpOrderedByBetRequired();
            }

            return cachedRtpOrderedByBetRequired.Value;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if this object has been initialized correctly before being used.
        /// </summary>
        /// <exception cref="CommunicationInterfaceUninitializedException">
        /// Thrown when any API is called before Initialize is called.
        /// </exception>
        private void CheckInitialization()
        {
            if(egmConfigDataCategory == null)
            {
                throw new CommunicationInterfaceUninitializedException(
                    "EgmConfigData cannot be used without calling its Initialize method first.");
            }
        }

        #endregion
    }
}
