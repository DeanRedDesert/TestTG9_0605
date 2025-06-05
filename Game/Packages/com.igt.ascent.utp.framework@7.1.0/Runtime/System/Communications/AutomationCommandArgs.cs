// -----------------------------------------------------------------------
// <copyright file = "AutomationCommandArgs.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Communications
{
    using System;

    /// <summary>
    /// AutomationCommandArgs
    /// Container for the AutomationCommand, as the design uses an EventHandler to pass the received data.
    /// </summary>
    public class AutomationCommandArgs : EventArgs
    {
        /// <summary>
        /// Data.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// AutomationCommand.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public AutomationCommand Command { get; set; }

        /// <summary>
        /// Hash.
        /// </summary>
        public Int32 Hash { get; set; }

        /// <summary>
        /// AutomationCommandArgs()
        /// Constructor.
        /// Conveniently converts the string data into an AutomationCommand.
        /// </summary>
        /// <param name="data">The AutomationCommand as a string.</param>
        public AutomationCommandArgs(string data)
        {
            Data = data;
            try
            {
                Command = AutomationCommand.Deserialize(Data);
            }
            catch
            {
                Command = null;
            }
        }
    }
}