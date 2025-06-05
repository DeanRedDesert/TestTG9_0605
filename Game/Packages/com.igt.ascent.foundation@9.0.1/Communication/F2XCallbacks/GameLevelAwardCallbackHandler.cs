// -----------------------------------------------------------------------
// <copyright file = "GameLevelAwardCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.GameLevelAward;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;
    using GameLevelLinkedData = Ascent.Communication.Platform.ReportLib.Interfaces.GameLevelLinkedData;

    /// <summary>
    /// This class implements callback methods supported by the F2X
    /// Game Level Award API category.
    /// </summary>
    internal class GameLevelAwardCallbackHandler : IGameLevelAwardCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling transactional events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// The callback interface for handling non-transactional events.
        /// </summary>
        private readonly INonTransactionalEventCallbacks nonTransactionalEventCallbacks;

        /// <summary>
        /// Initializes an instance of <see cref="GameLevelAwardCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling transactional events.</param>
        /// <param name="nonTransactionalEventCallbacks">
        /// The callback interface for handling non-transactional events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> or <paramref name="nonTransactionalEventCallbacks"/> is null.
        /// </exception>
        internal GameLevelAwardCallbackHandler(IEventCallbacks eventCallbacks, 
                                               INonTransactionalEventCallbacks nonTransactionalEventCallbacks)
        {
            if(eventCallbacks == null)
            {
                throw new ArgumentNullException("eventCallbacks");
            }

            if(nonTransactionalEventCallbacks == null)
            {
                throw new ArgumentNullException("nonTransactionalEventCallbacks");
            }

            this.eventCallbacks = eventCallbacks;
            this.nonTransactionalEventCallbacks = nonTransactionalEventCallbacks;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="themeIdentifier"/> or <paramref name="progressiveLevelPayvarDenomData"/>
        /// is null.
        /// </exception>
        public string ProcessGetThemeBasedGameLevelValues(ThemeIdentifier themeIdentifier,
            IEnumerable<ProgressiveLevelPayvarDenomData> progressiveLevelPayvarDenomData,
            out IEnumerable<GameLevelPayvarDenomData> callbackResult)
        {
            if(themeIdentifier == null)
            {
                throw new ArgumentNullException("themeIdentifier");
            }

            if(progressiveLevelPayvarDenomData == null)
            {
                throw new ArgumentNullException("progressiveLevelPayvarDenomData");
            }

            var getThemeBasedGameLevelValuesEventArgs =
                new GetGameLevelValuesEventArgs(themeIdentifier.Value,
                                                GetRawProgressiveData(progressiveLevelPayvarDenomData));

            nonTransactionalEventCallbacks.PostEvent(getThemeBasedGameLevelValuesEventArgs);

            var themeBasedData = getThemeBasedGameLevelValuesEventArgs.GameLevelValues;

            string errorMessage = null;
            if(themeBasedData != null)
            {
                callbackResult = ConvertGameLevelPayvarDenomData(themeBasedData);
            }
            else
            {
                callbackResult = null;

                errorMessage = getThemeBasedGameLevelValuesEventArgs.ErrorMessage;
                if(string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "Failed to generate theme based game level data.";
                }
            }

            return errorMessage;
        }

        /// <inheritdoc/>
        public string ProcessInitializeGameLevelData(ThemeIdentifier theme,
                                                     IEnumerable<PayvarDenominations> payvarDenominations)
        {
            var payvarDenominationInfos = payvarDenominations
                .SelectMany(payvarDenomination => payvarDenomination.ToPayvarDenominationInfos())
                .ToList();
            var initializeGameLevelDataEventArgs =
                new InitializeGameLevelDataEventArgs(theme.Value, payvarDenominationInfos);

            eventCallbacks.PostEvent(initializeGameLevelDataEventArgs);

            return initializeGameLevelDataEventArgs.ErrorMessage;
        }

        /// <summary>
        /// Gets the raw progressive data from Foundation, and converts it to game-level
        /// values indexed by <see cref="PaytableDenominationInfo"/>.
        /// </summary>
        /// <param name="progressiveLevelPayvarDenomDatas">
        /// The data of a theme identified by a <see cref="ThemeIdentifier"/>.
        /// </param>
        /// <returns>
        /// The game-level values indexed by <see cref="PaytableDenominationInfo"/>.
        /// Null if <paramref name="progressiveLevelPayvarDenomDatas"/> is null.
        /// </returns>
        private static IDictionary<PaytableDenominationInfo, IList<GameLevelLinkedData>> GetRawProgressiveData(
            IEnumerable<ProgressiveLevelPayvarDenomData> progressiveLevelPayvarDenomDatas)
        {
            var gameLevelValues = new Dictionary<PaytableDenominationInfo, IList<GameLevelLinkedData>>();
            foreach(var data in progressiveLevelPayvarDenomDatas)
            {
                var payvarIdentifier = data.Payvar.Value;
                foreach(var progressiveLevelDenomData in data.ProgressiveLevelDenomData)
                {
                    var denomination = (long)progressiveLevelDenomData.Denomination;
                    var payvarDenominationInfo = new PaytableDenominationInfo(payvarIdentifier, denomination);
                    if(progressiveLevelDenomData.ProgressiveLevelLinkedData != null)
                    {
                        var gameLevelLinkDataList = progressiveLevelDenomData.ProgressiveLevelLinkedData
                            .Select(progressiveLevelLinkedData => progressiveLevelLinkedData.ToGameLevelLinkedData())
                            .ToList();
                        gameLevelValues.Add(payvarDenominationInfo, gameLevelLinkDataList);
                    }
                    else
                    {
                        gameLevelValues.Add(payvarDenominationInfo, null);
                    }
                }
            }
            return gameLevelValues;
        }

        /// <summary>
        /// Converts the dictionary of game level progressive data to a list which represents the same data.
        /// </summary>
        /// <param name="gameLevelDataList">The game level progressive data to convert.</param>
        /// <returns>The representation of the game level progressive data in a list.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameLevelDataList"/> is null.
        /// </exception>
        private static IList<GameLevelPayvarDenomData> ConvertGameLevelPayvarDenomData(
            IDictionary<PaytableDenominationInfo, IList<GameLevelLinkedData>> gameLevelDataList)
        {
            if(gameLevelDataList == null)
            {
                throw new ArgumentNullException("gameLevelDataList");
            }

            var payvarDenomDatas = new Dictionary<string, List<GameLevelDenomData>>();

            foreach(var gameLevelData in gameLevelDataList)
            {
                var gameLevelDenomData = new GameLevelDenomData
                {
                    Denomination = checked((uint)gameLevelData.Key.Denomination),
                    GameLevelLinkedData = gameLevelData.Value == null
                        ? null
                        : gameLevelData.Value
                            .Select(gameLevelLinkedData => gameLevelLinkedData.ToInternalGameLevelLinkedData())
                            .ToList()
                };

                if(!payvarDenomDatas.ContainsKey(gameLevelData.Key.PaytableIdentifier))
                {
                    payvarDenomDatas.Add(gameLevelData.Key.PaytableIdentifier,
                        new List<GameLevelDenomData> { gameLevelDenomData });
                }
                else
                {
                    payvarDenomDatas[gameLevelData.Key.PaytableIdentifier].Add(gameLevelDenomData);
                }
            }

            return payvarDenomDatas
                .Select(payvarDenomData => new GameLevelPayvarDenomData
                                               {
                                                   Payvar = payvarDenomData.Key.ToPayvarIdentifier(),
                                                   GameLevelDenomData = payvarDenomData.Value
                                               })
                .ToList();
        }
    }
}