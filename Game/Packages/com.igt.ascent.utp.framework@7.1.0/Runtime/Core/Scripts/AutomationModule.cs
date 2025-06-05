// -----------------------------------------------------------------------
//  <copyright file = "AutomationModule.cs" company = "IGT">
//      Copyright (c) 2017-2019 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable MemberCanBeProtected.Global

namespace IGT.Game.Utp.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Communications;
    using UnityEngine;

    /// <summary>
    /// The base class for all modules to inherit from. This allows UTP core to know
    /// that this class can be accept AutomationCommands. 
    /// </summary>
    public abstract class AutomationModule : ScriptableObject
    {
        #region Private Fields

        /// <summary>
        /// Module methods array.
        /// </summary>
        private MethodInfo[] moduleMethods;

        /// <summary>
        /// Utp controller instance.
        /// </summary>
        private UtpController utpController;

        /// <summary>
        /// List of module commands.
        /// </summary>
        private List<ModuleCommand> commands;

        /// <summary>
        /// List of module events.
        /// </summary>
        private List<ModuleEvent> events;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets the utp controller.
        /// </summary>
        public UtpController UtpController
        {
            get { return utpController ?? (utpController = FindObjectOfType(typeof(UtpController)) as UtpController); }
        }

        /// <summary>
        /// The public name of the module.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The public name of the module.
        /// </summary>
        public virtual string Description
        {
            get { return ""; }
        }

        /// <summary>
        /// The module version.
        /// </summary>
        public virtual Version ModuleVersion
        {
            get { return new Version(0, 0); }
        }

        /// <summary>
        /// If true, the module will init when Initialize is called, even if it's already loaded.
        /// </summary>
        public virtual bool ForceInitialize
        {
            get { return false; }
        }

        /// <summary>
        /// Returns a list of commands the module supports.
        /// </summary>
        /// <remarks>This command is automatically generated from the attributes of the methods</remarks>
        public virtual List<ModuleCommand> Commands
        {
            get
            {
                if(commands != null)
                {
                    return commands;
                }

                commands = UtpModuleUtilities.GetModuleCommands(GetType());
                return commands;
            }
        }

        /// <summary>
        /// Gets a list of events implemented in a module.
        /// </summary>
        public virtual List<ModuleEvent> Events
        {
            get
            {
                if(events != null)
                {
                    return events;
                }

                events = UtpModuleUtilities.GetModuleEvents(GetType());
                return events;
            }
        }

        /// <summary>
        /// The communications object for the module. 
        /// </summary>
        public IUtpCommunication Communications { get; set; }

        /// <summary>
        /// Current status of the module.
        /// </summary>
        public ModuleStatuses ModuleStatus { get; set; }

        /// <summary>
        /// Error response types that are used to specify what kind of error occurred when calling SendErrorCommand.
        /// </summary>
        public enum ErrorResponses
        {
            /// <summary>
            /// A general error used when there's no other suitable error response.
            /// </summary>
            GeneralError,

            /// <summary>
            /// Used when attempting to communicate with an object that is not found in the scene (i.e. when a
            /// required object is inactive when queried).
            /// </summary>
            ReferenceObjectNotFound,

            /// <summary>
            /// Used when a specified object isn't found in the scene (i.e. when a button name is provided in
            /// PressButton(), but a button isn't found with the provided name).
            /// </summary>
            SpecifiedObjectNotFound,

            /// <summary>
            /// Used when a specified object is expected to exist only once in the scene, but multiple instances are
            /// found (i.e. when a button name is provided in PressButton(), but multiple buttons are found with the
            /// provided name).
            /// </summary>
            MultipleMatchesFound,

            /// <summary>
            /// Used when an expected parameter was not received with the incoming command.
            /// </summary>
            ParameterNotFound,

            /// <summary>
            /// Used when a non-null parameter value was expected, but a null value was received.
            /// </summary>
            ParameterValueNull,

            /// <summary>
            /// Used when the received parameter contains a value of an invalid type.
            /// </summary>
            ParameterValueInvalid
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Initializes the module.
        /// </summary>
        /// <returns>True is module is initialized and able to process commands</returns>
        public abstract bool Initialize();

        /// <summary>
        /// Called during "Get Modules" to handle any setup required before Initialize is called.
        /// </summary>
        public virtual void PreInitialize()
        {
        }

        /// <summary>
        /// Called for disposing of a module
        /// This can be used to unregister events or destroy objects when unloaded.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Handles command execution and event subscriptions. 
        /// </summary>
        /// <param name="data">AutomationCommander data.</param>
        /// <returns>decision outcome.</returns>
        public virtual bool Execute(AutomationCommander data)
        {
            //TODO: cache all commands on init so we do not have to do the reflection each time.
            var cmd = data.AutoCommand.Command;
            bool result = false;

            // check to see if this is an event subscription
            if(data.AutoCommand.IsEvent)
            {
                UtpEventSubscriber.Subscribe(data);
            }
            else // its a command, not an event subscription
            {
                if(moduleMethods == null)
                {
                    moduleMethods = GetType().GetMethods();
                }

                var execMethod = moduleMethods.ToList().FirstOrDefault(m => m.Name == cmd);

                if(execMethod == null)
                {
                    Debug.LogWarning("UTP: Could not find method '" + cmd + "'");
                }
                else
                {
                    // Try to invoke the method for the command with the Automation Command params. 
                    try
                    {
                        var exec = execMethod.Invoke(this, new object[] {data.AutoCommand, data.Communication});
                        result = (bool) exec;
                    }
                    catch(Exception ex)
                    {
                        // Failed to convert result into bool, or execute command. 
                        result = false;

                        //  Check if this exception is a ModuleException, or a TargetInvocationException containing a ModuleException
                        var moduleException = ex as ModuleException;
                        var targetInvocationException = ex as TargetInvocationException;
                        if (moduleException == null && targetInvocationException != null)
                        {
                            var innerModException = targetInvocationException.InnerException as ModuleException;
                            if (innerModException != null)
                            {
                                moduleException = innerModException;
                            }
                        }

                        if (moduleException != null)
                        {
                            Debug.LogWarning("UTP: A ModuleException occurred during command execution. Command: " + data.AutoCommand + " Error: " + ex);
                            SendErrorCommand(data.AutoCommand.Command, moduleException.ErrorMessage, data.Communication, moduleException.ErrorResponse);
                        }
                        else
                        {
                            Debug.LogWarning("UTP: An error occurred during command execution. Command: " + data.AutoCommand + " Error: " + ex);
                            SendErrorCommand(data.AutoCommand.Command, ex.ToString(), data.Communication);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Sends automation command
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <param name="parameters">Parameters to send with the command</param>
        /// <param name="sender">If provided, the command will only be sent to the specified client. Otherwise it will broadcast to all clients</param>
        /// <param name="isEvent">Setting this parameter to true will specific that this command is in fact an event. <remarks>Do not use this command directly to send an event; use SendEvent instead.</remarks></param>
        /// <returns>Sent status</returns>
        public bool SendCommand(string command, List<AutomationParameter> parameters, IUtpCommunication sender = null, bool isEvent = false)
        {
            var automationCommand = new AutomationCommand(Name, command, parameters) { IsEvent = isEvent };

            return SendCommand(automationCommand, sender);
        }

        /// <summary>
        /// Sends an automation command
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <param name="sender">The client to send the command to</param>
        /// <returns>Sent status</returns>
        public bool SendCommand(AutomationCommand command, IUtpCommunication sender)
        {
            bool sent = false;
            try
            {
                // Add timestamp to event commands
                if(command.IsEvent)
                {
                    if(command.Parameters == null)
                    {
                        command.Parameters = new List<AutomationParameter>();
                    }
					
                    if(!command.Parameters.Exists(p => p.Name == "Timestamp"))
                    {
                        command.Parameters.Add(new AutomationParameter("Timestamp", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"), "string", "Timestamp of when the event occurred."));
                    }
                }

                // if the sending connection is specified, use that one. 
                if(sender != null)
                {
                    sent = sender.Send(command);
                }
                else if(Communications != null)
                {
                    // Else try to use the default connection. 
                    sent = Communications.Send(command);
                }
                else
                {
                    Debug.LogWarning("UTP: Sender not provided and Communications is null. Nobody to send to.");
                }
            }
            catch(Exception ex)
            {
                // An unexpected error occurred, notify the user that this happened.
				Debug.LogWarning("UTP: Error sending command. Error: " + ex);
            }

            return sent;
        }

        /// <summary>
        /// Sends an error response command
        /// </summary>
        /// <param name="command">The command related to the error to send</param>
        /// <param name="errorMessage">The error message</param>
        /// <param name="sender">If provided, the command will only be sent to the specified client. Otherwise it will broadcast to all clients</param>
        /// <param name="errorResponse">The error response.</param>
        /// <returns>Sent status</returns>
        public bool SendErrorCommand(string command, string errorMessage, IUtpCommunication sender = null, ErrorResponses errorResponse = ErrorResponses.GeneralError)
        {
            var errorTypeParam = new AutomationParameter("ErrorType", errorResponse.ToString(), "text", "The error response type.");
            var errorMessageParam = new AutomationParameter("ErrorMessage", errorMessage, "text", "A message describing the error that occurred.");
            return SendCommand("ErrorResponse" + command, new List<AutomationParameter> { errorTypeParam, errorMessageParam }, sender);
        }

        /// <summary>
        /// Sends the event to the subscriber.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="broadcast">if set to <c>true</c> [broadcast].</param>
        protected void SendEvent(string eventName, List<AutomationParameter> parameters, bool broadcast = false)
        {
            if(broadcast)
            {
                SendCommand(eventName, parameters, null, true);
            }
            else
            {
                var subscribers = UtpEventSubscriber.UtpSubscriptions.ContainsKey(Name)
                    ? UtpEventSubscriber.UtpSubscriptions[Name].GetSubscribers(eventName)
                    : null;

                if(subscribers != null)
                {
                    foreach(IUtpCommunication subscriber in subscribers)
                    {
                        SendCommand(eventName, parameters, subscriber, true);
                    }
                }
            }
        }

        #endregion Public Methods

        #region Module Commands

        /// <summary>
        /// Reinitializes the module.
        /// </summary>
        /// <remarks>This command is automatically generated from the attributes of the methods</remarks>
        [ModuleCommand("Reinitialize", "bool Initialized", "Reinitializes the module.")]
        public bool Reinitialize(AutomationCommand command, IUtpCommunication sender)
        {
            var initialized = Initialize();
            ModuleStatus = initialized ? ModuleStatuses.InitializedEnabled : ModuleStatuses.InitializedDisabled;
            SendCommand("Reinitialize", new List<AutomationParameter> { new AutomationParameter("Initialized", initialized) }, sender);
            return true;
        }

        #endregion Module Commands
    }
}