// -----------------------------------------------------------------------
// <copyright file = "IUtpCommunication.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Communications
{
    /// <summary>
    /// Interface for UTP communications.
    /// </summary>
    public interface IUtpCommunication
    {
        /// <summary>
        /// Sends the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        bool Send(AutomationCommand command);
    }
}