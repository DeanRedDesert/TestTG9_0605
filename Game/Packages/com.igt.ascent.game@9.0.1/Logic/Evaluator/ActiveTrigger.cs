//-----------------------------------------------------------------------
// <copyright file = "ActiveTrigger.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.IO;
    using CompactSerialization;
    using Schemas;

    /// <summary>
    /// The purpose of this class is to manage a trigger during the execution of a game.
    /// </summary>
    [Serializable]
    public class ActiveTrigger : ICompactSerializable
    {
        /// <summary>
        /// Trigger which this active trigger is tracking.
        /// </summary>
        private Trigger initiatingTrigger;

        /// <summary>
        /// The number of times this trigger has ran for the current game.
        /// </summary>
        public uint PlayedCount { private set; get; }

        /// <summary>
        /// The total number of games which have been awarded for this trigger for the current game.
        /// </summary>
        public uint TotalCount { private set; get; }

        /// <summary>
        /// The number of times which this trigger has been retriggered for the current game. The trigger count is not
        /// incremented until the trigger has been awarded twice. If multiple of the same trigger are awarded within
        /// a single evaluation, then each trigger other than the first added is counted as a retrigger.
        /// </summary>
        public uint RetriggeredCount { private set; get; }

        /// <summary>
        /// The number of games given to the player on a retrigger.
        /// </summary>
        public uint LastRetriggerAwardedAmount { private set; get; }

        /// <summary>
        /// The priority of this trigger.
        /// </summary>
        public uint Priority { get { return initiatingTrigger.executionPriority; } }

        /// <summary>
        /// The name of this trigger.
        /// </summary>
        public string Name { get { return initiatingTrigger.name; } }

        /// <summary>
        /// The maximum play count for the trigger.
        /// </summary>
        public uint MaxPlayCount { get { return initiatingTrigger.MaxPlayCount; } }

        /// <summary>
        /// Parameterless contructor used by <see cref="CompactSerializer" /> for serialization purpose.
        /// </summary>
        public ActiveTrigger()
        {
            initiatingTrigger = new Trigger();
        }
        
        /// <summary>
        /// Create an instance of the active trigger with the given initiating trigger.
        /// </summary>
        /// <param name="initiatingTrigger">Initial trigger for this active trigger.</param>
        /// <exception cref="EvaluatorConfigurationException">
        /// Thrown if the play count of a trigger exceeds the maximum count for the trigger.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if initiating trigger is null.</exception>
        public ActiveTrigger(Trigger initiatingTrigger)
        {
            if (initiatingTrigger == null)
            {
                throw new ArgumentNullException("initiatingTrigger",
                                                "Initiating trigger for active trigger may not be null");
            }

            if (initiatingTrigger.PlayCount > initiatingTrigger.MaxPlayCount)
            {
                throw new EvaluatorConfigurationException(
                    "The play count of a trigger may not be greater than the max play count for that trigger. Trigger: " +
                    initiatingTrigger.name);
            }

            //Create a copy of the initiating trigger.
            this.initiatingTrigger = new Trigger(initiatingTrigger);

            TotalCount += initiatingTrigger.PlayCount;
        }

        /// <summary>
        /// Update the active trigger with a retrigger. The retrigger count will be increased and the TotalCount will be
        /// increased by the play count contained in the trigger. If adding the play count would exceed the maximum
        /// number of plays specified by the initiating trigger, then the total count will be set to the max. The return
        /// value will reflect the number of plays awarded by the trigger after any modifications related to the max
        /// play or retrigger counts. Retriggers beyond the retrigger maximum will increase the retriggered count but
        /// will not affect the total count.
        /// </summary>
        /// <param name="retrigger">Retrigger to update the active trigger with.</param>
        /// <returns>The number of plays awarded by the trigger.</returns>
        /// <exception cref="TriggerException">
        /// Thrown when the retrigger name does not match the active trigger name.
        /// </exception>
        public uint Retrigger(Trigger retrigger)
        {
            if (retrigger == null)
            {
                throw new ArgumentNullException("retrigger", "Argument may not be null");
            }

            if (retrigger.name != Name)
            {
                throw new TriggerException("Cannot retrigger an active trigger with a different name. Active name: " +
                                           Name + " Retrigger name: " + retrigger.name);
            }

            LastRetriggerAwardedAmount = 0;

            RetriggeredCount++;

            //If the retrigger count has been met then there is no need to process the trigger.
            if (RetriggeredCount > initiatingTrigger.MaxRetriggers)
            {
                LastRetriggerAwardedAmount = 0;
            }
            else
            {
                if (retrigger.PlayCount > initiatingTrigger.MaxPlayCount - TotalCount)
                {
                    LastRetriggerAwardedAmount = initiatingTrigger.MaxPlayCount - TotalCount;
                }
                else
                {
                    LastRetriggerAwardedAmount = retrigger.PlayCount;
                }
            }

            TotalCount += LastRetriggerAwardedAmount;

            return LastRetriggerAwardedAmount;
        }

        /// <summary>
        /// Increment the played count. 
        /// </summary>
        /// <returns>True if the trigger has remaining plays.</returns>
        /// <exception cref="TriggerException">
        /// Thrown if the number of plays has exceeded the total count.
        /// </exception>
        public bool Play()
        {
            PlayedCount++;

            if (PlayedCount > TotalCount)
            {
                throw new TriggerException("The play count for the trigger: " + initiatingTrigger.name +
                                           " has exceeded the total count.");
            }

            return PlayedCount < TotalCount;
        }

        /// <summary>
        /// Stop playing any remaining plays, this is achieved by setting the number of total plays
        /// equal to the current number of plays. 
        /// Designed to be used when a win cap is hit and the trigger needs to exit immediately.
        /// </summary>
        public void StopTrigger()
        {
            // set total count equal to played count so the trigger has no remaining games
            TotalCount = PlayedCount;
        }

        #region ICompactSerializable Members

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, initiatingTrigger);
            CompactSerializer.Write(stream, PlayedCount);
            CompactSerializer.Write(stream, TotalCount);
            CompactSerializer.Write(stream, RetriggeredCount);
            CompactSerializer.Write(stream, LastRetriggerAwardedAmount);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            initiatingTrigger = CompactSerializer.ReadSerializable<Trigger>(stream);
            PlayedCount = CompactSerializer.ReadUint(stream);
            TotalCount = CompactSerializer.ReadUint(stream);
            RetriggeredCount = CompactSerializer.ReadUint(stream);
            LastRetriggerAwardedAmount = CompactSerializer.ReadUint(stream);
        }

        #endregion
    }
}
