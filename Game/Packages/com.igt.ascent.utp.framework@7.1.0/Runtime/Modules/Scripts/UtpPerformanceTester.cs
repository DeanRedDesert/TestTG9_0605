// -----------------------------------------------------------------------
// <copyright file = "UtpPerformanceTester.cs" company = "IGT">
//     Copyright © 2014-2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global
namespace IGT.Game.Utp.Modules
{
    using System;
    using Framework;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Framework.Communications;

    [ModuleEvent("SendEvents", "AutomationCommand", "Performance test that sends multiple events.")]
    [ModuleEvent("SendParamsEvent", "AutomationCommand", "Performance test that sends an event with multiple params.")]
    [ModuleEvent("IntentionalErrorEvent", "AutomationCommand", "Performance test intentionally throws an error.")]
    public class UtpPerformanceTester : AutomationModule
    {
        #region AutomationModule Overrides

        public override string Name
        {
            get { return "PerformanceTester"; }
        }

        public override bool Initialize()
        {
            return true;
        }

        #endregion AutomationModule Overrides

        #region Module Commands
        // ReSharper disable UnusedParameter.Global

        /// <summary>
        /// This ModuleCommand method delays the game for a specified amount of time.
        /// </summary>
        /// <param name="command">The incoming command.</param>
        /// <param name="sender">The sender of the command.</param>
        [ModuleCommand("DelayGame", "void", "Delays the game in ms.",
            new[] { "delayTime|int|The amount of time to delay the game." })]
        public bool DelayGame(AutomationCommand command, IUtpCommunication sender)
        {
            int delayTime;

            var param = command.Parameters.FirstOrDefault();
            var paramValue = param == null ? "" : param.Value;

            if(Int32.TryParse(paramValue, out delayTime))
            {
                Thread.Sleep(delayTime);
            }

            //  Create the list of AutomationParameters to contain the return value
            var param1 = new AutomationParameter("GameStartTime", "Finished delaying the game.");
            var paramsList = new List<AutomationParameter> { param1 };

            //  Send the return value
            return SendCommand(command.Command, paramsList, sender);
        }

        /// <summary>
        /// This ModuleCommand method sends commands the number of times specified. 
        /// </summary>
        /// <param name="command">The incoming command.</param>
        /// <param name="sender">The sender of the command.</param>
        [ModuleCommand("SendCommands", "void", "Sends a specified number of commands..",
            new[] { "commandToSend|int|The number of commands to send." })]
        public bool SendCommands(AutomationCommand command, IUtpCommunication sender)
        {
            int commandCount;
            var param = command.Parameters.FirstOrDefault();
            var paramValue = param == null ? "" : param.Value;

            if(Int32.TryParse(paramValue, out commandCount))
            {

                for(int t = 1; t <= commandCount; t++)
                {
                    var param1 = new AutomationParameter("SendCommands" + t.ToString(), "The current command index.");
                    var paramsList = new List<AutomationParameter> { param1 };

                    SendCommand(command.Command, paramsList, sender);
                }

                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// This ModuleCommand method sends events the number of times specified. 
        /// </summary>
        /// <param name="command">The incoming command.</param>
        /// <param name="sender">The sender of the command.</param>
        [ModuleCommand("SendEvents", "void", "Sends the specified number of events.",
            new[] { "eventsToSend|int|The number of events to send." })]
        public bool SendEvents(AutomationCommand command, IUtpCommunication sender)
        {
            int commandCount;
            var param = command.Parameters.FirstOrDefault();
            var paramValue = param == null ? "" : param.Value;

            if(Int32.TryParse(paramValue, out commandCount))
            {

                for(int t = 1; t <= commandCount; t++)
                {
                    var param1 = new AutomationParameter("SendEvents" + t.ToString(), "The current command index.");
                    var paramsList = new List<AutomationParameter> { param1 };

                    SendEvent(command.Command, paramsList);
                }

                return true;

            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// This ModuleCommand method sends a command with the number of specified parameters. 
        /// </summary>
        /// <param name="command">The incoming command.</param>
        /// <param name="sender">The sender of the command.</param>
        [ModuleCommand("SendParams", "void", "Sends a command with the specified number of params.",
            new[] { "paramsToSend|int|The number of parameters to send." })]
        public bool SendParams(AutomationCommand command, IUtpCommunication sender)
        {
            int paramCount;
            var param = command.Parameters.FirstOrDefault();
            var paramValue = param == null ? "" : param.Value;

            if(Int32.TryParse(paramValue, out paramCount))
            {
                var paramsList = new List<AutomationParameter>();

                for(int t = 1; t <= paramCount; t++)
                {
                    var parameter = new AutomationParameter("SendParams" + t.ToString(), "The current command index.");

                    paramsList.Add(parameter);
                }

                return SendCommand(command.Command, paramsList, sender);
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// This ModuleCommand method sends an event with the number of specified parameters. 
        /// </summary>
        /// <param name="command">The incoming command.</param>
        /// <param name="sender">The sender of the command.</param>
        [ModuleCommand("SendParamsEvent", "Event", "Sends an event with the specified number of parameters.",
            new[] { "paramsToSend|int|The number of parameters to send." })]
        public bool SendParamsEvent(AutomationCommand command, IUtpCommunication sender)
        {
            int paramCount;
            var param = command.Parameters.FirstOrDefault();
            var paramValue = param == null ? "" : param.Value;

            if(Int32.TryParse(paramValue, out paramCount))
            {
                var paramsList = new List<AutomationParameter>();

                for(int t = 1; t <= paramCount; t++)
                {
                    var parameter = new AutomationParameter("SendParamsEvent" + t.ToString(), "The current command index.");

                    paramsList.Add(parameter);
                }

                SendEvent(command.Command, paramsList);

                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// This ModuleCommand method sends a command with the number of specified parameters. 
        /// </summary>
        /// <param name="command">The incoming command.</param>
        /// <param name="sender">The sender of the command.</param>
        [ModuleCommand("IntentionalError", "void", "Throws an uncaught error for testing.")]
        public bool IntentionalError(AutomationCommand command, IUtpCommunication sender)
        {
            throw new Exception("Intentional Error thrown from PerformanceTester");
        }

        /// <summary>
        /// This ModuleCommand method sends a command with the number of specified parameters. 
        /// </summary>
        /// <param name="command">The incoming command.</param>
        /// <param name="sender">The sender of the command.</param>
        [ModuleCommand("IntentionalErrorEvent", "void", "Throws an uncaught error for testing.")]
        public bool IntentionalErrorEvent(AutomationCommand command, IUtpCommunication sender)
        {
            throw new Exception("Intentional Error thrown from PerformanceTester for an Event");
        }

        // ReSharper restore UnusedParameter.Global
        #endregion Module Commands
    }
}