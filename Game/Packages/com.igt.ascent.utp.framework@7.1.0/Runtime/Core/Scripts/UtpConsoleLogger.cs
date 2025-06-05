// -----------------------------------------------------------------------
//  <copyright file = "UtpConsoleLogger.cs" company = "IGT">
//      Copyright (c) 2016 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Global
// ReSharper disable EventNeverSubscribedTo.Global
namespace IGT.Game.Utp.Framework
{
    using System;
    using Communications;
    using UnityEngine;

    /// <summary>
    /// IUtpCommunication class for logging to the console.
    /// </summary>
    /// <seealso cref="IGT.Game.Utp.Framework.Communications.IUtpCommunication" />
    public class UtpConsoleLogger : IUtpCommunication
    {
        /// <summary>
        /// Sends the specified command to the console.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>True if able to log the command.</returns>
        public bool Send(AutomationCommand command)
        {
            Debug.Log(command.ToString());
            return true;
        }

        /// <summary>
        /// Occurs when [automation command received].
        /// </summary>
#pragma warning disable 67
        public event EventHandler<AutomationCommandArgs> AutomationCommandReceived;
#pragma warning restore 67
    }
}