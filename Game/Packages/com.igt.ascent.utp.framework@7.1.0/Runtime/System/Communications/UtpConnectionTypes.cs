// -----------------------------------------------------------------------
// <copyright file = "UtpConnectionTypes.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework.Communications
{
    /// <summary>
    /// Types of connections Utp Supports. 
    /// </summary>
    public enum UtpConnectionTypes
    {
        /// <summary>
        /// Used when connecting via websocket
        /// </summary>
        Websocket,

        /// <summary>
        /// Used when connection is local on the console
        /// </summary>
        Console
    }
}