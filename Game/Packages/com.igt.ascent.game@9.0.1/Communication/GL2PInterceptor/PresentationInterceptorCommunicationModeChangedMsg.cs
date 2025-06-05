//-----------------------------------------------------------------------
// <copyright file = "PresentationInterceptorCommunicationModeChangedMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using System.Text;

    /// <summary>
    /// Message object that is comprised of the parameters of the IGameLogicInterceptorClient
    /// interface function CommunicationModeChanged.
    /// </summary>
    [Serializable]
    public class PresentationInterceptorCommunicationModeChangedMsg : InterceptorGenericMsg
    {
        /// <summary>
        /// Empty Constructor required for certain types of serialization.
        /// </summary>
        private PresentationInterceptorCommunicationModeChangedMsg() { }

        /// <summary>
        /// Constructor for PresentationInterceptorCommunicationModeChangedMsg.
        /// </summary>
        /// <param name="mode">Current communication mode.</param>
        public PresentationInterceptorCommunicationModeChangedMsg(InterceptorCommunicationMode mode)
        {
            Mode = mode;
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

            return builder.ToString();
        }

        /// <summary>
        /// Gets current communication Mode.
        /// </summary>
        public InterceptorCommunicationMode Mode { get; private set; }
    }
}
