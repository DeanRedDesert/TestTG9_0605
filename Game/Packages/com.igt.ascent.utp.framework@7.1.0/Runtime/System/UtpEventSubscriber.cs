// -----------------------------------------------------------------------
// <copyright file = "UtpEventSubscriber.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Communications;

    /// <summary>
    /// Track and subscribe connections to events.
    /// </summary>
    public static class UtpEventSubscriber
    {
        /// <summary>
        /// Subscription tracker.
        /// </summary>
        public static Dictionary<string, SubscriptionManager> UtpSubscriptions = new Dictionary<string, SubscriptionManager>();

        /// <summary>
        /// Subscribes a connection to an event
        /// </summary>
        /// <param name="commander">The commander data.</param>
        /// <returns>True if subscribed, false if unsubscribed.</returns>
        public static bool Subscribe(AutomationCommander commander)
        {
            if(commander == null)
            {
                throw new ArgumentNullException("commander");
            }

            var result = false;

            if(commander.AutoCommand.IsEvent)
            {
                if(commander.AutoCommand.Parameters != null && commander.AutoCommand.Parameters.Count == 2)
                {
                    try
                    {
                        var moduleName = commander.AutoCommand.Module;
                        var eventName = AutomationParameter.GetParameterValues(commander.AutoCommand, "EventName").FirstOrDefault();
                        var eventAction = AutomationParameter.GetParameterValues(commander.AutoCommand, "Action").FirstOrDefault();
                        var eventResponse = new AutomationCommand(commander.AutoCommand.Module, commander.AutoCommand.Command, new List<AutomationParameter>());
                        AutomationParameter.SetParameter(eventResponse, "ModuleName", moduleName);
                        AutomationParameter.SetParameter(eventResponse, "EventName", eventName);

                        if(eventName != null && eventAction != null)
                        {
                            AutomationParameter.SetParameter(eventResponse, "Action", "Unknown");
                            var action = eventAction.ToLower().Equals("add") ? SubscriptionManager.EventSubscriptionAction.Add : SubscriptionManager.EventSubscriptionAction.Remove;
                            UpdateEventSubscriber(moduleName, eventName, commander.Communication, action);
                            AutomationParameter.SetParameter(eventResponse, "Action", action.ToString());
                            result = true;
                        }
                        else
                        {
                            Console.WriteLine("UTP: Event subscription request with incomplete parameters");
                        }

                        AutomationParameter.SetParameter(eventResponse, "Result", result.ToString());
                        commander.Communication.Send(eventResponse);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("An error occurred during an event subscription attempt. Command: " + commander.AutoCommand + " Error: " + ex);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Updates event subscriber information.
        /// </summary>
        /// <param name="module">The module the event is attached to.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="action">the subscription action (i.e. add or remove).</param>
        /// <returns></returns>
        private static void UpdateEventSubscriber(string module, string eventName, IUtpCommunication subscriber, SubscriptionManager.EventSubscriptionAction action)
        {
            if(UtpSubscriptions == null)
            {
                UtpSubscriptions = new Dictionary<string, SubscriptionManager>();
            }

            if(!UtpSubscriptions.ContainsKey(module))
            {
                UtpSubscriptions.Add(module, new SubscriptionManager());
            }

            try
            {
                if(action == SubscriptionManager.EventSubscriptionAction.Add)
                {
                    UtpSubscriptions[module].AddSubscriber(subscriber, eventName);
                }
                else
                {
                    UtpSubscriptions[module].RemoveSubscriber(subscriber, eventName);
                }
            }
            catch(Exception exception)
            {
                Console.WriteLine("Exception found: " + exception.Message);
            }
        }

        /// <summary>
        /// Handles the events sent from modules.
        /// </summary>
        public class SubscriptionManager
        {
            readonly Dictionary<string, List<IUtpCommunication>> eventSubscriberMap = new Dictionary<string, List<IUtpCommunication>>();

            /// <summary>
            /// Gets the subscribers for an event.
            /// </summary>
            /// <param name="eventName">Name of the event.</param>
            /// <returns>a list of subscriber event names</returns>
            public ICollection<IUtpCommunication> GetSubscribers(string eventName)
            {
                lock(eventSubscriberMap)
                {
                    if(eventSubscriberMap.ContainsKey(eventName) == false)
                        return null;

                    if(eventSubscriberMap[eventName] == null || eventSubscriberMap[eventName].Count == 0)
                        return null;

                    return eventSubscriberMap[eventName];
                }
            }

            /// <summary>
            /// Adds the subscribe.
            /// </summary>
            /// <param name="subscriber">The subscriber.</param>
            /// <param name="eventName">Name of the event.</param>
            public void AddSubscriber(IUtpCommunication subscriber, string eventName)
            {
                lock(eventSubscriberMap)
                {
                    if(eventSubscriberMap.ContainsKey(eventName) == false)
                        eventSubscriberMap.Add(eventName, new List<IUtpCommunication>());

                    if(eventSubscriberMap[eventName].Exists(x => x.GetHashCode() == subscriber.GetHashCode()) == false)
                    {
                        eventSubscriberMap[eventName].Add(subscriber);
                    }
                }
            }

            /// <summary>
            /// Removes the subscriber.
            /// </summary>
            /// <param name="subscriber">The subscriber.</param>
            /// <param name="eventName">Name of the event.</param>
            public void RemoveSubscriber(IUtpCommunication subscriber, string eventName = null)
            {
                lock(eventSubscriberMap)
                {
                    if(eventName == null) // this means remove the subscriber from all events
                    {
                        foreach(var keyValuePair in eventSubscriberMap)
                        {
                            int index = keyValuePair.Value.FindIndex(x => x.GetHashCode() == subscriber.GetHashCode());
                            if(index >= 0)
                            {
                                eventSubscriberMap[keyValuePair.Key].RemoveAt(index);
                            }
                        }
                    }

                    else if(eventSubscriberMap.ContainsKey(eventName) && eventSubscriberMap[eventName] != null &&
                        eventSubscriberMap[eventName].Count > 0)
                    {
                        int index = eventSubscriberMap[eventName].FindIndex(x => x.GetHashCode() == subscriber.GetHashCode());
                        if(index >= 0)
                        {
                            eventSubscriberMap[eventName].RemoveAt(index);
                        }
                    }
                }
            }

            /// <summary>
            /// Definitions for how a subscription should be updated
            /// </summary>
            public enum EventSubscriptionAction
            {
                /// <summary>
                /// Used when subscribing to an event
                /// </summary>
                Add,

                /// <summary>
                /// Used when unsubscribing to an event
                /// </summary>
                Remove
            }
        }
    }
}