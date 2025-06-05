//-----------------------------------------------------------------------
// <copyright file = "SystemProgressiveController.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Communication.Standalone;
    using Logic.ProgressiveController;

    /// <summary>
    /// Class that simulates a foundation or other networked progressive controller.
    /// </summary>
    internal class SystemProgressiveController : AbstractProgressiveController, ISystemProgressiveController
    {
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
        /// <param name="configurationList">List of configurations for this
        ///                                 controller's controller levels.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="iGameLib"/> or <paramref name="name"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> contains illegal characters,
        /// </exception>
        /// <seealso cref="IGT.Game.Core.Communication.Foundation.Utility.ValidateCriticalDataName"/>
        /// <exception cref="InvalidStreamDataException">
        /// Thrown when <paramref name="configurationList"/> contains invalid data.
        /// </exception>
        public SystemProgressiveController(IGameLib iGameLib,
                                           string name,
                                           IList<ProgressiveConfiguration> configurationList)
            : base(iGameLib, name)
        {
            // Validate the level count.
            var levelCount = configurationList.Count;

            if(levelCount <= 0)
            {
                throw new InvalidStreamDataException("The controller level count must be greater than 0.");
            }

            // Instantiate the controller levels with the configuration list passed in.
            ControllerLevels = new List<ControllerLevel>(levelCount);

            for(var levelIndex = 0; levelIndex < levelCount; levelIndex++)
            {
                ControllerLevels.Add(new ControllerLevel(levelIndex, GameLibReference, ControllerName));
                ControllerLevels[levelIndex].Configuration = configurationList[levelIndex];
            }
        }

        #endregion

        #region AbstractProgressiveController Overrides

        /// <inheritdoc />
        public override event EventHandler<ProgressiveBroadcastEventArgs> ProgressiveBroadcastEvent;

        #endregion

        #region ISystemProgressiveController Members

        /// <inheritdoc cref="AbstractProgressiveController" />
        public new IList<ControllerLevel> ControllerLevels
        {
            get => base.ControllerLevels;
            private set => base.ControllerLevels = value as List<ControllerLevel> ?? new List<ControllerLevel>(value);
        }

        /// <inheritdoc />
        public void ReloadControllerLevels()
        {
            // Reload data from the critical data for each controller level.
            foreach(var controllerLevel in ControllerLevels)
            {
                controllerLevel.Reload();
            }
        }

        /// <inheritdoc />
        public void ClearLevelMapping()
        {
            LevelMapping.Clear();
        }

        /// <inheritdoc />
        public void SetLevelMapping(IDictionary<int, int> mappingList)
        {
            LevelMapping = new Dictionary<int, int>(mappingList);
        }

        /// <inheritdoc />
        public ProgressiveBroadcastData GetProgressiveBroadcastDataForControllerLevel(int controllerLevel)
        {
            if(controllerLevel < 0 || controllerLevel >= ControllerLevelCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(controllerLevel), $"Controller level {controllerLevel} does not exist.");
            }

            return new ProgressiveBroadcastData(ControllerLevels[controllerLevel].Amount.Displayable,
                                                ControllerLevels[controllerLevel].Configuration.PrizeString);
        }

        /// <inheritdoc />
        public void ContributeToEventBasedProgressive(int gameLevel, long amountNumerator, long amountDenominator)
        {
            var controllerLevel = GetControllerLevel(gameLevel);

            controllerLevel.ContributeEventBased(amountNumerator, amountDenominator, true);
        }

        #endregion
    }
}
