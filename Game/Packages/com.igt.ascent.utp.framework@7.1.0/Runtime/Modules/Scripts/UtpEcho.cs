// -----------------------------------------------------------------------
// <copyright file = "UtpEcho.cs" company = "IGT">
//     Copyright © 2014-2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global
namespace IGT.Game.Utp.Modules
{
    using Framework;
    using Framework.Communications;
    using UnityEngine;

    [ModuleEvent("Echo", "AutomationCommand", "This event occurs when an EchoEvent command is received")]
    public class UtpEcho : AutomationModule
    {
        public override string Name
        {
            get { return "Echo"; }
        }

        public override bool Initialize()
        {
            return true;
        }

        [ModuleCommand("Echo", "AutomationCommand", "Echos the response back to the sender",
            new[] { "EchoText|String|Information that will be in the echo" })]
        public bool Echo(AutomationCommand command, IUtpCommunication sender)
        {
            Debug.Log("Echoing");
            return SendCommand(command.Command, command.Parameters, sender);
        }

        [ModuleCommand("EchoEvent", "void", "Broadcasts echo to all connected clients",
            new[] { "EchoText|String|Information that will be in the echo" })]
        // ReSharper disable once UnusedParameter.Global
        public bool EchoEvent(AutomationCommand command, IUtpCommunication sender)
        {
            Debug.Log("Broadcasting Echo");
            SendEvent("Echo", command.Parameters);
            return true;
        }
    }
}