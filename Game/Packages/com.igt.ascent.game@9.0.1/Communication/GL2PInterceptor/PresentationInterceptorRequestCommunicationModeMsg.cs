//-----------------------------------------------------------------------
// <copyright file = "PresentationInterceptorRequestCommunicationModeMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using System.Text;

    /// <summary>
    /// Message object that is comprised of the parameters of the IPInterceptorClient
    /// interface function RequestCommunicationMode.
    /// </summary>
    [Serializable]
    public class PresentationInterceptorRequestCommunicationModeMsg : InterceptorGenericMsg
    {
        /// <summary>
        /// Empty Constructor required for certain types of serialization.
        /// </summary>
        private PresentationInterceptorRequestCommunicationModeMsg() { }

        /// <summary>
        /// Constructor for PresentationInterceptorRequestCommunicationModeMsg
        /// </summary>
        /// <param name="mode">Requested communication mode.</param>
        /// <param name="key">Security key.</param>
        public PresentationInterceptorRequestCommunicationModeMsg(InterceptorCommunicationMode mode, string key)
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
