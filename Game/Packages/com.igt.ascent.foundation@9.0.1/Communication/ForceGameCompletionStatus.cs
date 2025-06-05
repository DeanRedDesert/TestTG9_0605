// -----------------------------------------------------------------------
// <copyright file = "ForceGameCompletionStatus.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Represents the current status of a Force Game-Completion condition.
    /// </summary>
    [Serializable]
    public class ForceGameCompletionStatus : ICompactSerializable, IDeepCloneable
    {
        /// <summary>
        /// Gets whether a force game-completion condition exists.
        /// </summary>
        /// <remarks>
        /// If true, no NEW game-cycles may be 
        /// started (as enforced by the Foundation).  If the Game is in mid game-cycle, the Game is 
        /// to take control and auto-pick/auto-finish for the player WHEN the FinishTime is reached.
        /// </remarks>
        public bool ForceCompletion { get; private set; }

        /// <summary>
        /// If <see cref="ForceCompletion"/> is <see langword="true"/>, gets the UTC time down to the second, 
        /// at which point the Game may guide the player to cash-out.  
        /// </summary>
        /// <remarks>
        /// As per the GLI #23 standard, if the WarningTime is in the past, the Game may reject the initiation 
        /// of any ancillary or bonus play.
        /// </remarks>
        public DateTime WarningTime { get; private set; }

        /// <summary> 
        /// If <see cref="ForceCompletion"/> is <see langword="true"/>, gets the UTC time down to the second, 
        /// at which point the Game is to take control and 
        /// auto-pick/auto-finish the game-cycle.
        /// </summary>
        /// <remarks>
        /// The Game should generally provide a 
        /// count-down display to the player which ends at the FinishTime.  If the FinishTime 
        /// is in the past, the Game should take control immediately after displaying the player 
        /// message (assuming the game is waiting on player input/picks).
        /// </remarks>
        public DateTime FinishTime { get; private set; }

        /// <summary>
        /// If <see cref="ForceCompletion"/> is <see langword="true"/>, gets a dictionary of localized versions 
        /// of the force game-completion message keyed by culture. 
        /// </summary>
        /// <remarks>
        /// Assuming the Game is in mid game-cycle, and is or will wait on player input/picks: 
        /// this message should generally be shown to the player when the ForcedCompletion 
        /// condition is detected. The Game should select the message that is paired with 
        /// the current culture. If the current culture is not included in the list, 
        /// the Game may pick another culture as the Game deems appropriate. 
        /// Refer to GLI Standard #23 (or other jurisdictional specific material).
        /// </remarks>
        public IDictionary<string, string> LocalizedMessages { get; private set; }

        /// <summary>
        /// Instantiates a new <see cref="ForceGameCompletionStatus"/> without the
        /// force game-completion condition.
        /// </summary>
        public ForceGameCompletionStatus()
            : this(false, new DateTime(), new DateTime(), null)
        { }

        /// <summary>
        /// Instantiates a new <see cref="ForceGameCompletionStatus"/>.
        /// </summary>
        /// <param name="forceCompletion">Whether the force game-completion condition exists.</param>
        /// <param name="warningTime">Time when the Game should guide the player to cash-out.</param>
        /// <param name="finishTime">
        /// Time when the Game should auto-pick/auto-finish any current games
        /// and when no more new game-cycles may be started.
        /// </param>
        /// <param name="localizedMessages">
        /// Dictionary of localized force game-completion
        /// messages keyed by culture.
        /// </param>
        public ForceGameCompletionStatus(bool forceCompletion,
                                         DateTime warningTime,
                                         DateTime finishTime,
                                         IDictionary<string, string> localizedMessages)
        {
            ForceCompletion = forceCompletion;
            WarningTime = warningTime;
            FinishTime = finishTime;
            LocalizedMessages = localizedMessages ?? new Dictionary<string, string>();
        }

        #region ICompactSerializable Members

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Serialize(stream, ForceCompletion);
            CompactSerializer.Serialize(stream, WarningTime.Ticks);
            CompactSerializer.Serialize(stream, FinishTime.Ticks);
            CompactSerializer.Serialize(stream, LocalizedMessages);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            ForceCompletion = CompactSerializer.ReadBool(stream);
            WarningTime = new DateTime(CompactSerializer.ReadLong(stream));
            FinishTime = new DateTime(CompactSerializer.ReadLong(stream));
            LocalizedMessages = CompactSerializer.ReadDictionary<string, string>(stream);
        }

        #endregion

        #region IDeepCloneable Members

        /// <inheritdoc/>
        public object DeepClone()
        {
            var messageClone = new Dictionary<string, string>(LocalizedMessages);
            return new ForceGameCompletionStatus(ForceCompletion, WarningTime, FinishTime, messageClone);
        }

        #endregion
    }
}
