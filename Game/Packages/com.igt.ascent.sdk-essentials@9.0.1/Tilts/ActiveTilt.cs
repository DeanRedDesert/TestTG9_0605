//-----------------------------------------------------------------------
// <copyright file = "ActiveTilt.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Tilts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// An implementation of <see cref="IActiveTilt"/>.
    /// </summary>
    [Serializable]
    public class ActiveTilt : IActiveTilt
    {
        #region Constants

        /// <summary>
        /// The max length of the title string allowed by the Foundation.
        /// </summary>
        private const int MaxTitleLength = 39;

        /// <summary>
        /// The max length of the message string allowed by the Foundation.
        /// </summary>
        private const int MaxMessageLength = 119;

        #endregion Constants

        #region Fields

        /// <summary>
        /// A dictionary containing a list of cultures and a tilt message for each culture.
        /// </summary>
        private Dictionary<string, string> messageLocalizations;

        /// <summary>
        /// A dictionary containing a list of cultures and a tilt title for each culture.
        /// </summary>
        private Dictionary<string, string> titleLocalizations;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default parameterless constructor needed by Compact Serialization.
        /// </summary>
        public ActiveTilt()
        {
        }

        /// <summary>
        /// Constructs an instance of <see cref="ActiveTilt"/>.
        /// </summary>
        /// <param name="priority">
        /// The priority of the tilt. Tilts are sorted by priority.
        /// </param>
        /// <param name="messageDictionary">
        /// Defines a dictionary containing a list of cultures and a tilt message for each culture.
        /// </param>
        /// <param name="titleDictionary">
        /// Defines a dictionary containing a list of cultures and a tilt title for each culture.
        /// </param>
        /// <param name="gamePlayBehavior">
        /// Defines whether the tilt will block the game play. The tilt posted by an extension cannot block extension or other
        /// applications play. The tilt posted by an extension can only block game play. This is because the Foundation
        /// cannot block the extension from play.
        /// Optional parameter that defaults to <see cref="TiltGamePlayBehavior.NonBlocking"/>.
        /// </param>
        /// <param name="userInterventionRequired">
        /// Defines whether the tilt requires user notification through a protocol.
        /// Optional parameter that defaults to false.
        /// </param>
        /// <param name="progressiveLinkDown">
        /// Defines whether the tilt signals a game controlled progressive link down.
        /// Optional parameter that defaults to false.
        /// </param>
        /// <param name="attendantClear">
        /// Defines whether or not an attendant can clear this tilt.
        /// Optional parameter that defaults to false.
        /// </param>
        /// <param name="delayPreventGamePlayUntilNoGameInProgress">
        /// Defines whether delay the preventing of game play until all games and game like features
        /// (such as sports betting purchases) have been completed.
        /// Optional parameter that defaults to false.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="gamePlayBehavior"/> is not <see cref="TiltGamePlayBehavior.Blocking"/> but
        /// <paramref name="delayPreventGamePlayUntilNoGameInProgress"/> is true.
        /// </exception>
        public ActiveTilt(TiltPriority priority,
                          Dictionary<string, string> messageDictionary,
                          Dictionary<string, string> titleDictionary,
                          TiltGamePlayBehavior gamePlayBehavior = TiltGamePlayBehavior.NonBlocking,
                          bool userInterventionRequired = false,
                          bool progressiveLinkDown = false,
                          bool attendantClear = false,
                          bool delayPreventGamePlayUntilNoGameInProgress = false)
        {
            ConstructorInitialize(priority,
                                  messageDictionary,
                                  titleDictionary,
                                  gamePlayBehavior,
                                  userInterventionRequired,
                                  progressiveLinkDown,
                                  attendantClear,
                                  delayPreventGamePlayUntilNoGameInProgress);
        }

        /// <remarks />
        protected void ConstructorInitialize(TiltPriority priority,
                                             Dictionary<string, string> messageDictionary,
                                             Dictionary<string, string> titleDictionary,
                                             TiltGamePlayBehavior gamePlayBehavior,
                                             bool userInterventionRequired,
                                             bool progressiveLinkDown,
                                             bool attendantClear,
                                             bool delayPreventGamePlayUntilNoGameInProgress)
        {
            Priority = priority;
            titleLocalizations = titleDictionary ?? throw new ArgumentNullException(nameof(titleDictionary));
            messageLocalizations = messageDictionary ?? throw new ArgumentNullException(nameof(messageDictionary));
            if(delayPreventGamePlayUntilNoGameInProgress && gamePlayBehavior != TiltGamePlayBehavior.Blocking)
            {
                throw new ArgumentException($"{nameof(delayPreventGamePlayUntilNoGameInProgress)} can't be true " +
                                            $"when {nameof(gamePlayBehavior)} is NOT {TiltGamePlayBehavior.Blocking}");
            }

            GamePlayBehavior = gamePlayBehavior;
            ProgressiveLinkDown = progressiveLinkDown;
            UserInterventionRequired = userInterventionRequired;
            AttendantClear = attendantClear;
            DelayPreventGamePlayUntilNoGameInProgress = delayPreventGamePlayUntilNoGameInProgress;
        }

        #endregion Constructors

        #region IActiveTilt

        /// <inheritdoc />
        public TiltPriority Priority { get; private set; }

        /// <inheritdoc />
        public TiltGamePlayBehavior GamePlayBehavior { get; private set; }

        /// <inheritdoc />
        public bool UserInterventionRequired { get; private set; }

        /// <inheritdoc />
        public bool ProgressiveLinkDown { get; private set; }

        /// <inheritdoc />
        public bool AttendantClear { get; private set; }

        /// <inheritdoc />
        public bool DelayPreventGamePlayUntilNoGameInProgress { get; private set; }

        /// <inheritdoc />
        /// <remarks>
        /// The title may be truncated to the max length of the title string allowed by the Foundation.
        /// </remarks>
        public string GetLocalizedTitle(string culture)
        {
            var title = string.Empty;

            if(titleLocalizations.ContainsKey(culture))
            {
                title = titleLocalizations[culture];
            }

            return title.Substring(0, Math.Min(title.Length, MaxTitleLength));
        }

        /// <inheritdoc />
        /// <remarks>
        /// The message may be truncated to the max length of the message string allowed by the Foundation.
        /// </remarks>
        public string GetLocalizedMessage(string culture)
        {
            var message = string.Empty;

            if(messageLocalizations.ContainsKey(culture))
            {
                message = messageLocalizations[culture];
            }

            return message.Substring(0, Math.Min(message.Length, MaxMessageLength));
        }

        #endregion IActiveTilt

        #region ICompactSerializable

        /// <inheritdoc />
        public virtual void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, Priority);
            CompactSerializer.Write(stream, GamePlayBehavior);
            CompactSerializer.Write(stream, UserInterventionRequired);
            CompactSerializer.Write(stream, ProgressiveLinkDown);
            CompactSerializer.Write(stream, AttendantClear);
            CompactSerializer.Write(stream, DelayPreventGamePlayUntilNoGameInProgress);
            CompactSerializer.WriteDictionary(stream, titleLocalizations);
            CompactSerializer.WriteDictionary(stream, messageLocalizations);
        }

        /// <inheritdoc />
        public virtual void Deserialize(Stream stream)
        {
            Priority = CompactSerializer.ReadEnum<TiltPriority>(stream);
            GamePlayBehavior = CompactSerializer.ReadEnum<TiltGamePlayBehavior>(stream);
            UserInterventionRequired = CompactSerializer.ReadBool(stream);
            ProgressiveLinkDown = CompactSerializer.ReadBool(stream);
            AttendantClear = CompactSerializer.ReadBool(stream);
            DelayPreventGamePlayUntilNoGameInProgress = CompactSerializer.ReadBool(stream);
            titleLocalizations = CompactSerializer.ReadDictionary<string, string>(stream);
            messageLocalizations = CompactSerializer.ReadDictionary<string, string>(stream);
        }

        #endregion ICompactSerializable
    }
}
