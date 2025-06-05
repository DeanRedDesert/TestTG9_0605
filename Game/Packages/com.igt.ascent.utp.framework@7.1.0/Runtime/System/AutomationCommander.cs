// -----------------------------------------------------------------------
// <copyright file = "AutomationCommander.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework
{
    using Communications;

    /// <summary>
    /// An Automation Command Wrapper for the controller and module to communicate with.
    /// </summary>
    public class AutomationCommander
    {
        /// <summary>
        /// The type of connection being sent to.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public UtpConnectionTypes ConnectionType { get; set; }

        /// <summary>
        /// The communication object that sends data. 
        /// </summary>
        public IUtpCommunication Communication { get; set; }

        /// <summary>
        /// The command to send with parameters. 
        /// </summary>
        public AutomationCommand AutoCommand { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationCommander"/> class.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="command">The command.</param>
        /// <param name="type">The type of connection.</param>
        public AutomationCommander(IUtpCommunication communication,
            AutomationCommand command,
            UtpConnectionTypes type = UtpConnectionTypes.Websocket)
        {
            ConnectionType = type;
            Communication = communication;
            AutoCommand = command;
        }
    }
}