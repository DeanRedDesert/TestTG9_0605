//-----------------------------------------------------------------------
// <copyright file = "GameTilt.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Tilts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CompactSerialization;

    /// <summary>
    /// This class represents a game side tilt to be used by non-concurrent games,
    /// most likely posted by the game presentation.
    /// </summary>
    /// <remarks>
    /// It does not allow the customization of some of the flags.
    /// </remarks>
    /// <devdoc>
    /// It supports different flags than <see cref="ShellTilt"/>.
    /// </devdoc>
    [Serializable]
    public class GameTilt : ActiveTilt, ITilt
    {
        #region Constructors

        /// <summary>
        /// Default parameterless constructor needed by Compact Serialization.
        /// </summary>
        public GameTilt()
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="GameTilt"/> based on a Game Tilt Definition.
        /// </summary>
        /// <param name="tiltDefinition">The tilt definition used for initialization.</param>
        public GameTilt(IGameTiltDefinition tiltDefinition)
        {
            if(tiltDefinition == null)
            {
                throw new ArgumentNullException(nameof(tiltDefinition), "Provided tilt definition must not be null");
            }

            if(tiltDefinition.TitleLocalizations == null)
            {
                throw new NullReferenceException("TitleLocalizations in provided tiltDefinition is null");
            }

            if(tiltDefinition.MessageLocalizations == null)
            {
                throw new NullReferenceException("MessageLocalizations in provided tiltDefinition is null");
            }

            // F2L tilt manager does not support the attendantClear and delayPreventGamePlayUntilNoGameInProgress flags yet.
            // Always use false for them.
            ConstructorInitialize(tiltDefinition.Priority,
                                  tiltDefinition.MessageLocalizations.ToDictionary(pair => pair.First, pair => pair.Second),
                                  tiltDefinition.TitleLocalizations.ToDictionary(pair => pair.First, pair => pair.Second),
                                  tiltDefinition.GamePlayBehavior,
                                  tiltDefinition.UserInterventionRequired,
                                  tiltDefinition.GameControlledProgressiveLinkDown,
                                  false,
                                  false);

            DiscardBehavior = tiltDefinition.DiscardBehavior;
        }

        /// <summary>
        /// Protected constructor allowing the inherited class to customize all the fields.
        /// </summary>
        protected GameTilt(TiltPriority priority,
                           Dictionary<string, string> messageDictionary,
                           Dictionary<string, string> titleDictionary,
                           TiltGamePlayBehavior gamePlayBehavior,
                           bool userInterventionRequired,
                           bool progressiveLinkDown,
                           bool attendantClear,
                           bool delayPreventGamePlayUntilNoGameInProgress,
                           TiltDiscardBehavior discardBehavior)
            : base(priority,
                   messageDictionary,
                   titleDictionary,
                   gamePlayBehavior,
                   userInterventionRequired,
                   progressiveLinkDown,
                   attendantClear,
                   delayPreventGamePlayUntilNoGameInProgress)
        {
            DiscardBehavior = discardBehavior;
        }

        #endregion

        #region Implementation of ITilt

        /// <inheritdoc />
        public TiltDiscardBehavior DiscardBehavior { get; private set; }

        #endregion

        #region ICompactSerializable Members

        /// <inheritdoc/>
        public override void Serialize(System.IO.Stream stream)
        {
            base.Serialize(stream);
            CompactSerializer.Write(stream, DiscardBehavior);
        }

        /// <inheritdoc/>
        public override void Deserialize(System.IO.Stream stream)
        {
            base.Deserialize(stream);
            DiscardBehavior = CompactSerializer.ReadEnum<TiltDiscardBehavior>(stream);
        }

        #endregion
    }
}
