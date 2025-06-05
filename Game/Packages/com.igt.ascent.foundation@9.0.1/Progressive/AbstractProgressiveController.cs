//-----------------------------------------------------------------------
// <copyright file = "AbstractProgressiveController.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.ProgressiveController
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.OutcomeList;
    using Ascent.OutcomeList.Interfaces;
    using Communication.Foundation;

    /// <summary>
    /// The abstract base for all progressive controllers,
    /// including game controlled progressive controllers
    /// and simulated system progressive controllers used
    /// by Standalone Game Lib.
    /// </summary>
    /// <remarks>
    /// This class is for internal use within Core only.
    /// Game developers should use <see cref="BaseProgressiveController"/> or one of
    /// the derived classes as the base for any extended progressive controller feature.
    /// </remarks>
    public abstract class AbstractProgressiveController : IProgressiveController
    {
        #region Fields

        /// <summary>
        /// Reference to the Game Lib needed for accessing
        /// configurations and critical data.
        /// </summary>
        protected readonly IGameLib GameLibReference;

        /// <summary>
        /// List of controller levels.
        /// </summary>
        protected List<ControllerLevel> ControllerLevels;

        /// <summary>
        /// Mapping between the game levels and controller levels
        /// for current theme context, keyed by game levels.
        /// </summary>
        protected Dictionary<int, int> LevelMapping = new Dictionary<int, int>();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor that specifies the name of the controller,
        /// and the game lib that can be used to access critical data.
        /// </summary>
        /// <param name="iGameLib">Reference to a game lib needed for critical
        ///                        data operations.</param>
        /// <param name="name">Name of the progressive controller.
        ///                    It must conform to the requirement
        ///                    of a critical data path, since the name
        ///                    is used as the critical data path for
        ///                    this controller.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="iGameLib"/> is null or
        /// <paramref name="name"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> contains illegal characters.
        /// </exception>
        /// <seealso cref="IGT.Game.Core.Communication.Foundation.Utility.ValidateCriticalDataName"/>
        protected AbstractProgressiveController(IGameLib iGameLib, string name)
        {
            if(iGameLib == null)
            {
                throw new ArgumentNullException(nameof(iGameLib));
            }

            if(string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if(!Utility.ValidateCriticalDataName(name))
            {
                throw new ArgumentException("Controller name contains illegal characters.", nameof(name));
            }

            ControllerName = name;
            GameLibReference = iGameLib;
        }

        #endregion

        #region IProgressiveController Members

        #region Events and Properties

        /// <inheritdoc />
        public abstract event EventHandler<ProgressiveBroadcastEventArgs> ProgressiveBroadcastEvent;

        /// <inheritdoc />
        public string ControllerName { get; private set; }

        /// <inheritdoc />
        public int ControllerLevelCount
        {
            get
            {
                return ControllerLevels.Count;
            }
        }

        /// <inheritdoc />
        public int GameLevelCount
        {
            get
            {
                return LevelMapping.Count;
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual bool IsGameLevelLinked(int gameLevel)
        {
            return LevelMapping.ContainsKey(gameLevel);
        }

        /// <inheritdoc />
        public virtual ProgressiveConfiguration GetProgressiveConfiguration(int gameLevel)
        {
            var controllerLevel = GetControllerLevel(gameLevel);

            return controllerLevel.Configuration;
        }

        /// <inheritdoc />
        public virtual IDictionary<int, ProgressiveConfiguration> GetAllProgressiveConfigurations()
        {
            var result = new Dictionary<int, ProgressiveConfiguration>(GameLevelCount);

            foreach(var gameLevel in LevelMapping.Keys)
            {
                result[gameLevel] = GetProgressiveConfiguration(gameLevel);
            }

            return result;
        }

        /// <inheritdoc />
        public virtual long GetProgressiveAmount(int gameLevel)
        {
            var controllerLevel = GetControllerLevel(gameLevel);

            return controllerLevel.Amount.Displayable;
        }

        /// <inheritdoc />
        public virtual IDictionary<int, long> GetAllProgressiveAmounts()
        {
            var result = new Dictionary<int, long>(GameLevelCount);

            foreach(var gameLevel in LevelMapping.Keys)
            {
                result[gameLevel] = GetProgressiveAmount(gameLevel);
            }

            return result;
        }

        /// <inheritdoc />
        public virtual void ContributeToProgressive(int gameLevel, long bet, long denomination)
        {
            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            var controllerLevel = GetControllerLevel(gameLevel);

            checked
            {
                controllerLevel.Contribute(bet * denomination, true);
            }
        }

        /// <inheritdoc />
        public virtual void ContributeToAllProgressives(long bet, long denomination, bool saveToCriticalData)
        {
            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            checked
            {
                foreach(var gameLevel in LevelMapping.Keys)
                {
                    var controllerLevel = GetControllerLevel(gameLevel);
                    controllerLevel.Contribute(bet * denomination, saveToCriticalData);
                }
            }
        }

        /// <inheritdoc />
        public void ContributeToAllEventBasedProgressives(long bet, long denomination, bool saveToCriticalData)
        {
            if(bet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bet), "Bet cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            foreach(var gameLevel in LevelMapping.Keys)
            {
                var controllerLevel = GetControllerLevel(gameLevel);
                controllerLevel.ContributeEventBased(bet, denomination, saveToCriticalData);
            }
        }

        /// <inheritdoc />
        public virtual void ValidateProgressiveHit(ProgressiveAward progressiveAward)
        {
            if(progressiveAward == null)
            {
                throw new ArgumentNullException(nameof(progressiveAward));
            }

            if(progressiveAward.HitState == ProgressiveAwardHitState.PotentialHit &&
               progressiveAward.GameLevel.HasValue)
            {
                var controllerLevel = GetControllerLevel((int)progressiveAward.GameLevel);

                controllerLevel.MarkHit(progressiveAward);
            }
        }

        /// <inheritdoc />
        public virtual void ResetProgressiveHits()
        {
            foreach(var controllerLevel in ControllerLevels.Where(controllerLevel => controllerLevel.HitFlag))
            {
                controllerLevel.ResetHit();
            }
        }

        /// <inheritdoc />
        public virtual ProgressiveHitRecord GetProgressiveHitRecord(int gameLevel)
        {
            var controllerLevel = GetControllerLevel(gameLevel);

            // Return a copy of the record.
            return new ProgressiveHitRecord
            {
                HitCount = controllerLevel.HitRecord.HitCount,
                TotalHitAmount = controllerLevel.HitRecord.TotalHitAmount
            };
        }

        /// <inheritdoc />
        public virtual IDictionary<int, ProgressiveHitRecord> GetAllProgressiveHitRecords()
        {
            var result = new Dictionary<int, ProgressiveHitRecord>(GameLevelCount);

            foreach(var gameLevel in LevelMapping.Keys)
            {
                result[gameLevel] = GetProgressiveHitRecord(gameLevel);
            }

            return result;
        }

        /// <inheritdoc />
        public virtual ProgressiveBroadcastData GetProgressiveBroadcastData(int gameLevel)
        {
            var controllerLevel = GetControllerLevel(gameLevel);

            return new ProgressiveBroadcastData(controllerLevel.Amount.Displayable,
                                                controllerLevel.Configuration.PrizeString);
        }

        /// <inheritdoc />
        public virtual IDictionary<int, ProgressiveBroadcastData> GetAllProgressiveBroadcastData()
        {
            var result = new Dictionary<int, ProgressiveBroadcastData>(GameLevelCount);

            foreach(var gameLevel in LevelMapping.Keys)
            {
                result[gameLevel] = GetProgressiveBroadcastData(gameLevel);
            }

            return result;
        }

        #endregion

        #endregion

        #region Protected Methods

        /// <summary>
        /// Get the controller level to which a game level is linked.
        /// </summary>
        /// <param name="gameLevel">The game level.</param>
        /// <returns>The controller level to which the game level is linked.</returns>
        /// <exception cref="GameLevelNotLinkedException">
        /// Thrown when the specified game level is not linked.
        /// </exception>
        protected virtual ControllerLevel GetControllerLevel(int gameLevel)
        {
            if(!IsGameLevelLinked(gameLevel))
            {
                throw new GameLevelNotLinkedException(gameLevel);
            }

            var result = ControllerLevels[LevelMapping[gameLevel]];

            return result;
        }

        #endregion
    }
}
