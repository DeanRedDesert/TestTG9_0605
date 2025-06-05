//-----------------------------------------------------------------------
// <copyright file = "TriggerManager.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CompactSerialization;
    using Schemas;

    /// <summary>
    /// The purpose of this class is to manage active triggers during the execution of a game. Once a trigger has been
    /// initiated during a game this class will track the current progress of that trigger, and provide information
    /// which allows the developer to enforce the rules associated with that trigger (max play count, max retrigger
    /// count, etc).
    /// </summary>
    [Serializable]
    public class TriggerManager : ICompactSerializable
    {
        /// <summary>
        /// List of triggers which are currently active.
        /// </summary>
        private List<ActiveTrigger> activeTriggers = new List<ActiveTrigger>();

        /// <summary>
        /// Get the highest priority active trigger which has plays remaining.
        /// </summary>
        /// <returns>
        /// The highest priority active trigger. If there are no triggers with remaining play counts, or there are no
        /// triggers active, then null will be returned.
        /// </returns>
        public ActiveTrigger GetPriorityTrigger()
        {
            long highestPriority = -1;
            ActiveTrigger highestPriorityTrigger = null;

            foreach (var activeTrigger in
                activeTriggers.Where(
                    activeTrigger =>
                    activeTrigger.Priority > highestPriority && activeTrigger.PlayedCount < activeTrigger.TotalCount)
                )
            {
                highestPriorityTrigger = activeTrigger;
                highestPriority = activeTrigger.Priority;
            }

            return highestPriorityTrigger;
        }

        /// <summary>
        /// Get the active trigger for the given trigger name.
        /// </summary>
        /// <param name="triggerName">The name of the trigger to get.</param>
        /// <returns>
        /// An active trigger containing the trigger information for the given trigger name. Returns null if there was 
        /// not a trigger with the specified name.
        /// </returns>
        public ActiveTrigger GetTrigger(string triggerName)
        {
            return activeTriggers.FirstOrDefault(trigger => trigger.Name == triggerName);
        }

        /// <summary>
        /// Add or update an active trigger based on the passed trigger. If an active trigger with the same name as the
        /// passed trigger is found, then its total count and retrigger count will be updated. If an active trigger is
        /// not present, then a new active trigger is created based on the passed trigger.
        /// </summary>
        /// <param name="trigger">Trigger to add or update.</param>
        /// <exception cref="ArgumentNullException">Thrown when the trigger parameter is null.</exception>
        public void Trigger(Trigger trigger)
        {
            if (trigger == null)
            {
                throw new ArgumentNullException("trigger", "Parameter may not be null");
            }

            var existingTrigger = activeTriggers.Find(activeTrigger => activeTrigger.Name == trigger.name);

            if (existingTrigger != null)
            {
                existingTrigger.Retrigger(trigger);
            }
            else
            {
                activeTriggers.Add(new ActiveTrigger(trigger));
            }
        }

        /// <summary>
        /// Update the play count of an active trigger. This should be called after the evaluation associated with the
        /// trigger has been performed.
        /// </summary>
        /// <param name="triggerName">The name of the trigger which was played.</param>
        /// <returns>True if the trigger has remaining plays.</returns>
        /// <exception cref="TriggerException">Thrown if the specified trigger is not active.</exception>
        /// <example><code>
        /// <![CDATA[
        /// var currentTrigger = triggerManager.GetPriorityTrigger();
        /// 
        /// if (currentTrigger != null)
        /// {
        ///     EvaluationResult evalResult = null;
        ///     if(currentTrigger.Name == "FreeGameLines")
        ///     {
        ///         evalResult = SlotEvaluator.EvaluatePaytableSection(freeGameSection, randomNumbers, betDefinitions);
        ///     }
        ///     else if(currentTrigger.Name == "FreeGameMultiway")
        ///     {
        ///         evalResult = SlotEvaluator.EvaluatePaytableSection(freeGameMultiwaySection, randomNumbers, betDefinitions);
        ///     }
        ///     else
        ///     {
        ///         //Error on unknown trigger.
        ///     }
        ///
        ///     //Inform the trigger manager that the trigger was played.
        ///     triggerManager.Play(currentTrigger.Name);
        /// }
        /// ]]>
        /// </code></example>
        public bool Play(string triggerName)
        {
            var triggerToPlay = activeTriggers.Find(activeTrigger => activeTrigger.Name == triggerName);
            if (triggerToPlay == null)
            {
                throw new TriggerException("Trigger is not active and therefore cannot be played: " + triggerName);
            }

            return triggerToPlay.Play();
        }

        /// <summary>
        /// Add or update an active trigger for every trigger contained in the passed list of win outcome items.
        /// </summary>
        /// <param name="winOutcomeItems">Items to add triggers from.</param>
        /// <example><code>
        /// <![CDATA[
        /// //In this example we start with a new instance of the trigger manager, but normally a trigger manager would
        /// //be re-used, and therefore it would need to be cleared between games with ClearTriggers.
        /// triggerManager = new TriggerManager();
        ///
        /// var evalutionResults = SlotEvaluator.EvaluatePaytableSection(paytableSection, randomNumbers, betDefinitions);
        ///
        /// triggerManager.AddTriggersFromWinOutcomeItems(evalutionResults.WinOutcome.WinOutcomeItems);
        /// 
        /// //nextTrigger will either be the highest priority trigger contained in the win outcome, or null if there were
        /// //no triggers.
        /// nextTrigger = triggerManager.GetPriorityTrigger();
        /// ]]>
        /// </code></example>
        public void AddTriggersFromWinOutcomeItems(IEnumerable<WinOutcomeItem> winOutcomeItems)
        {
            foreach (var trigger in
                from winOutcomeItem in winOutcomeItems
                from trigger in winOutcomeItem.Prize.Trigger
                select trigger)
            {
                Trigger(trigger);
            }
        }

        /// <summary>
        /// Clear the list of active triggers. This should be called before the start of a new game.
        /// </summary>
        public void ClearTriggers()
        {
            activeTriggers.Clear();
        }

        /// <summary>
        /// Stop any plays on the trigger.
        /// Designed to be used when a win cap is hit and the trigger needs to exit immediately.
        /// </summary>
        /// <param name="triggerName">The name of the trigger to be stopped.</param>
        /// <exception cref="TriggerException">Thrown if the specified trigger is not active.</exception>
        public void StopTrigger(string triggerName)
        {
            var triggerToStop = activeTriggers.Find(activeTrigger => activeTrigger.Name == triggerName);

            if(triggerToStop == null)
            {
                throw new TriggerException("Trigger is not active and therefore cannot be removed: " + triggerName);
            }

            triggerToStop.StopTrigger();
        }

        #region ICompactSerializable Members

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, activeTriggers);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            activeTriggers = CompactSerializer.ReadListSerializable<ActiveTrigger>(stream);
        }

        #endregion
    }
}
