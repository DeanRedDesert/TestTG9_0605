//-----------------------------------------------------------------------
// <copyright file = "GameCycleBetting.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Standard
{
    using System;
    using System.Collections.Generic;
    using Game.Core.Communication.Foundation.F2X;
    using Interfaces;
    using Platform.Interfaces;

    /// <summary>
    /// Implementation of the <see cref="IGameCycleBetting"/> interface that is backed by the F2X.
    /// </summary>
    internal class GameCycleBetting : IGameCycleBetting, IContextCache<ICoplayerContext>
    {
        #region Private Members

        /// <summary>
        /// The cached interface of the GameCycleBetting category.
        /// </summary>
        private readonly CategoryInitializer<IGameCycleBettingCategory> gameCycleBettingCategory = new CategoryInitializer<IGameCycleBettingCategory>();

        private readonly GamePlayStore gamePlayStore;

        private const string PrefixClassPath = "CoplayerLib/GameCycleBetting/";

        // Critical data blocks for the cached bet values.
        private readonly SingleCriticalData<long?> committedBetBlock = new SingleCriticalData<long?>(PrefixClassPath + "CommittedBet");
        private readonly SingleCriticalData<long> startingBetBlock = new SingleCriticalData<long>(PrefixClassPath + "StartingBet");
        private readonly SingleCriticalData<long> accumulatedMidGameBlock = new SingleCriticalData<long>(PrefixClassPath + "AccumulatedMidGameBet");

        private long? committedBet;

        private long? CommittedBet
        {
            get => committedBet;
            set
            {
                if(committedBet != value)
                {
                    committedBet = value;
                    committedBetBlock.Data = committedBet;
                    gamePlayStore.Write(committedBetBlock);
                }
            }
        }

        private long startingBet;

        private long StartingBet
        {
            get => startingBet;
            set
            {
                if(startingBet != value)
                {
                    startingBet = value;
                    startingBetBlock.Data = startingBet;
                    gamePlayStore.Write(startingBetBlock);
                }
            }
        }

        private long accumulatedMidGameBet;

        private long AccumulatedMidGameBet
        {
            get => accumulatedMidGameBet;
            set
            {
                if(accumulatedMidGameBet != value)
                {
                    accumulatedMidGameBet = value;
                    accumulatedMidGameBlock.Data = accumulatedMidGameBet;
                    gamePlayStore.Write(accumulatedMidGameBlock);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of the <see cref="GameCycleBetting"/>.
        /// </summary>
        /// <param name="gamePlayStore">
        /// A reference to the <see cref="GamePlayStore"/> required for saving to Critical Data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gamePlayStore"/> is null.
        /// </exception>
        public GameCycleBetting(GamePlayStore gamePlayStore)
        {
            // This could happen if Coplayer Lib constructs stuff in wrong order.

            this.gamePlayStore = gamePlayStore ?? throw new ArgumentNullException(nameof(gamePlayStore));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes class members whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="category">
        /// The interface of the game cycle betting category.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is null.
        /// </exception>
        public void Initialize(IGameCycleBettingCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            gameCycleBettingCategory.Initialize(category);
        }

        #endregion

        #region IGameCycleBetting Implementation

        /// <inheritdoc/>
        public GameCycleBettingConfigData ConfigData { get; private set; }

        /// <inheritdoc />
        public bool CommitBet(long betInCredits, long denomination)
        {
            if(betInCredits < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(betInCredits), "Bet cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            var checkedAmount = checked(betInCredits * denomination);

            var result = gameCycleBettingCategory.Instance.CommitBet(checkedAmount);
            if(result)
            {
                CommittedBet = checkedAmount;
            }

            StartingBet = 0;
            AccumulatedMidGameBet = 0;

            return result;
        }

        /// <inheritdoc />
        public long? GetCommittedBet()
        {
            return CommittedBet;
        }

        /// <inheritdoc />
        public void UncommitBet()
        {
            if(!CommittedBet.HasValue)
            {
                throw new InvalidOperationException($"{nameof(UncommitBet)} can not be called without calling {nameof(CommitBet)} first.");
            }

            gameCycleBettingCategory.Instance.UncommitBet();

            CommittedBet = null;
            StartingBet = 0;
            AccumulatedMidGameBet = 0;
        }

        /// <inheritdoc />
        public void PlaceStartingBet(bool isMaxBet)
        {
            if(!CommittedBet.HasValue)
            {
                throw new InvalidOperationException($"{nameof(PlaceStartingBet)} can not be called without calling {nameof(CommitBet)} first.");
            }

            gameCycleBettingCategory.Instance.PlaceStartingBet(isMaxBet);

            StartingBet = CommittedBet.Value;
            CommittedBet = null;
        }

        /// <inheritdoc />
        public long GetStartingBet()
        {
            return StartingBet;
        }

        /// <inheritdoc />
        public bool PlaceMidGameBet(long betInCredits, long denomination)
        {
            if(betInCredits < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(betInCredits), "Bet cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            var checkedAmount = checked(betInCredits * denomination);

            var result = gameCycleBettingCategory.Instance.PlaceBet(checkedAmount);

            if(result)
            {
                AccumulatedMidGameBet = checked(AccumulatedMidGameBet + checkedAmount);
            }

            return result;
        }

        /// <inheritdoc />
        public long GetAccumulatedMidGameBet()
        {
            return AccumulatedMidGameBet;
        }

        #endregion

        #region IContextCache Implementation

        /// <inheritdoc />
        public void NewContext(ICoplayerContext coplayerContext)
        {
            var configData = gameCycleBettingCategory.Instance.GetConfigData();
            ConfigData = new GameCycleBettingConfigData(
                MoneyCalc.ConvertToCredits(configData.MaxBetAmount, coplayerContext.Denomination),
                MoneyCalc.ConvertToCredits(configData.MinBetAmount, coplayerContext.Denomination, true),
                MoneyCalc.ConvertToCredits(configData.ButtonPanelMinBetAmount, coplayerContext.Denomination, true)
            );

            LoadBetStatusFromCriticalData();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads the starting committed and accumulated mid-game bet values from the <see cref="GamePlayStore"/>.
        /// </summary>
        private void LoadBetStatusFromCriticalData()
        {
            var dataBlock = gamePlayStore.Read(new List<CriticalDataName>
                                                   {
                                                       committedBetBlock.Name,
                                                       startingBetBlock.Name,
                                                       accumulatedMidGameBlock.Name
                                                   });

            committedBet = dataBlock.GetCriticalData<long?>(committedBetBlock.Name);
            startingBet = dataBlock.GetCriticalData<long>(startingBetBlock.Name);
            accumulatedMidGameBet = dataBlock.GetCriticalData<long>(accumulatedMidGameBlock.Name);
        }

        #endregion
    }
}