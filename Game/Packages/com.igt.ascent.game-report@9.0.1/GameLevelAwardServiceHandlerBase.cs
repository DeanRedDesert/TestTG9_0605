// -----------------------------------------------------------------------
// <copyright file = "GameLevelAwardServiceHandlerBase.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using Interfaces;

    /// <summary>
    /// The implementation of <see cref="IGameLevelAwardServiceHandler"/>.
    /// </summary>
    /// <remarks>
    /// Custom game report needs to implement this abstract class if it will support
    /// the <see cref="ReportingServiceType.GameLevelAward"/>.
    /// </remarks>
    public abstract class GameLevelAwardServiceHandlerBase : IGameLevelAwardServiceHandler
    {
        #region IGameLevelAwardServiceHandler Members

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters is null.</exception>
        public void InitializeForGameLevelQuery(
            string themeIdentifier,
            IEnumerable<PaytableDenominationInfo> paytableDenominationInfos)
        {
            if(themeIdentifier == null)
            {
                throw new ArgumentNullException(nameof(themeIdentifier));
            }

            if(paytableDenominationInfos == null)
            {
                throw new ArgumentNullException(nameof(paytableDenominationInfos));
            }

            var gameLevelDataContexts = paytableDenominationInfos.Select(
                payvarDenominationInfo => new GameLevelDataContext(themeIdentifier,
                                                                   payvarDenominationInfo.PaytableIdentifier,
                                                                   payvarDenominationInfo.Denomination));
            InitializeAllGameLevelData(gameLevelDataContexts);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters is null.</exception>
        public IDictionary<PaytableDenominationInfo, IList<GameLevelLinkedData>> GetThemeBasedGameLevelValues(
            string themeIdentifier,
            IDictionary<PaytableDenominationInfo, IList<GameLevelLinkedData>> rawProgressiveData)
        {
            if(themeIdentifier == null)
            {
                throw new ArgumentNullException(nameof(themeIdentifier));
            }

            if(rawProgressiveData == null)
            {
                throw new ArgumentNullException(nameof(rawProgressiveData));
            }

            var gameValues = rawProgressiveData.ToDictionary(
                progressiveData => new GameLevelDataContext(
                    themeIdentifier,
                    progressiveData.Key.PaytableIdentifier,
                    progressiveData.Key.Denomination),
                progressiveData => progressiveData.Value);

            var customGameLevelValues = AdjustAllGameLevelValues(gameValues);

            return customGameLevelValues.ToDictionary(
                customGameLevelValue => new PaytableDenominationInfo(
                    customGameLevelValue.Key.PaytableIdentifier,
                    customGameLevelValue.Key.Denomination),
                customGameLevelValue => customGameLevelValue.Value);
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method does nothing by default in base class.
        /// The derived class should override this method to clean up any out-dated data
        /// when the report object is going out of context.
        /// </remarks>
        public virtual void CleanUpData()
        {
        }

        /// <inheritdoc />
        /// <remarks>
        /// The derived class should override this method to clean up the resources of the service handler,
        /// such as un-register the event handlers with the <see cref="IReportLib"/>.
        /// This method is different from the <see cref="CleanUpData"/>, which is used for cleaning up the
        /// out-dated cache data when changing context.
        /// </remarks>
        public abstract void CleanUpResources(IReportLib reportLib);

        #endregion

        #region Protected Virtual Methods

        /// <summary>
        /// Executes initialization work for a specific game level data context as needed.
        /// </summary>
        /// <param name="gameLevelDataContext">
        /// The context used to indicate the theme, paytable and the denomination information.
        /// </param>
        /// <remarks>
        /// This method does nothing by default in base class.
        /// Derived class should override this method to do the initialization work as needed.
        /// </remarks>
        // ReSharper disable once UnusedParameter.Global
        protected virtual void InitializeGameLevelData(GameLevelDataContext gameLevelDataContext)
        {
        }

        /// <summary>
        /// Executes initialization work for a collection of game level data contexts.
        /// </summary>
        /// <param name="gameLevelDataContexts">
        /// The context collection used to indicate the themes, paytables and the denominations information.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameLevelDataContexts"/> is null.
        /// </exception>
        /// <remarks>
        /// The derived class should override this method if it needs to initialize for all contexts at one time.
        /// </remarks>
        protected virtual void InitializeAllGameLevelData(IEnumerable<GameLevelDataContext> gameLevelDataContexts)
        {
            if(gameLevelDataContexts == null)
            {
                throw new ArgumentNullException(nameof(gameLevelDataContexts));
            }

            foreach(var gameLevelDataContext in gameLevelDataContexts)
            {
                InitializeGameLevelData(gameLevelDataContext);
            }
        }

        /// <summary>
        /// Adjust the game level progressive data sent from the Foundation.
        /// </summary>
        /// <param name="gameLevelDataContext">
        /// The context indicating the theme paytable and denomination.
        /// </param>
        /// <param name="rawProgressiveData">
        /// The raw progressive data sent from Foundation.
        /// </param>
        /// <returns>
        /// The adjusted game level values associated with the game level context data.
        /// </returns>
        /// <remarks>
        /// The derived class should override this method to adjust the game level value as needed.
        /// </remarks>
        protected virtual IList<GameLevelLinkedData> AdjustGameLevelValues(
            // ReSharper disable once UnusedParameter.Global
            GameLevelDataContext gameLevelDataContext,
            IList<GameLevelLinkedData> rawProgressiveData)
        {
            return rawProgressiveData;
        }

        /// <summary>
        /// Adjust the game level progressive data of all the themes, paytables
        /// and denominations sent from the Foundation.
        /// </summary>
        /// <param name="rawProgressiveDatas">
        /// The raw progressive data sent from the Foundation.
        /// </param>
        /// <returns>
        /// The adjusted game level progressive data of all the themes, paytables and
        /// denominations.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="rawProgressiveDatas"/> is null.
        /// </exception>
        /// <remarks>
        /// The derived class should override this method as needed to adjust the game level values at one time.
        /// </remarks>
        protected virtual IDictionary<GameLevelDataContext, IList<GameLevelLinkedData>> AdjustAllGameLevelValues(
            IDictionary<GameLevelDataContext, IList<GameLevelLinkedData>> rawProgressiveDatas)
        {
            if(rawProgressiveDatas == null)
            {
                throw new ArgumentNullException(nameof(rawProgressiveDatas));
            }

            return rawProgressiveDatas.ToDictionary(
                rawProgressiveData => rawProgressiveData.Key,
                rawProgressiveData => AdjustGameLevelValues(rawProgressiveData.Key, rawProgressiveData.Value));
        }

        #endregion
    }
}