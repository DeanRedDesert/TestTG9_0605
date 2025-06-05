// -----------------------------------------------------------------------
// <copyright file = "ModuleEvent.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Utp.Framework
{
    using System;

    /// <summary>
    /// Manage events triggered by modules.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class ModuleEvent : Attribute
    {
        /// <summary>
        /// The name of the event
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string EventName { get; set; }

        /// <summary>
        /// A description of what the event does. 
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Description { get; set; }

        /// <summary>
        /// Definition for event arguments. 
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string EventArgs { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleEvent"/> class.
        /// </summary>
        public ModuleEvent()
        {
            EventName = "";
            Description = "";
            EventArgs = "";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleEvent"/> class.
        /// </summary>
        /// <param name="eventName">The eventName.</param>
        /// <param name="eventArgs">Argument definition for the event.</param>
        /// <param name="description">The description of what the event does.</param>
        public ModuleEvent(string eventName, string eventArgs, string description)
        {
            if(string.IsNullOrEmpty(eventArgs))
            {
                throw new ArgumentNullException("eventArgs");
            }

            EventName = eventName;
            Description = description;
            EventArgs = UtpTypeSerializer.GetTypeDefinition(eventArgs);
        }
    }
}