//-----------------------------------------------------------------------
// <copyright file = "GameLogicInterceptorRequestCommunicationModeMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using System.Text;

    /// <summary>
    /// Message object that is comprised of the parameters of the IGameLogicInterceptorClient
    /// interface function RequestCommunicationMode.
    /// </summary>
    [Serializable]
    public class GameLogicInterceptorRequestCommunicationModeMsg : InterceptorGenericMsg
    {
        /// <summary>
        /// Empty Constructor required for certain types of serialization.
        /// </summary>
        private GameLogicInterceptorRequestCommunicationModeMsg() { }

        /// <summary>
        /// Constructor for GameLogicInterceptorRequestCommunicationModeMsg.
        /// </summary>
        /// <param name="mode">Requested communication mode.</param>
        /// <param name="key">Security key.</param>
        public GameLogicInterceptorRequestCommunicationModeMsg(InterceptorCommunicationMode mode, string key)
        {
            Mode = mode;
            Key = key;
        }

        /// <summary>
        /// Display contents of object as string.
        /// </summary>
        /// <returns>string representation of object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            builder.AppendLine("\tMode:" + Mode);
            builder.AppendLine("\tKey:" + Key);

            return builder.ToString();
        }

        /// <summary>
        /// Gets requested communication mode.
        /// </summary>
        public InterceptorCommunicationMode Mode { get; private set; }

        /// <summary>
        /// Gets security key.
        /// </summary>
        public string Key { get; private set; }
    }
}
