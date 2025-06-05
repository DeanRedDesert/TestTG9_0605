//-----------------------------------------------------------------------
// <copyright file = "PresentationGenericMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Presentation.CommServices
{
    using System;

    /// <summary>
    ///    Base Class for all messages that will be put into the Presentation
    ///    Host Message Queue.
    /// </summary>
    [Serializable]
    public class PresentationGenericMsg
    {
        /// <summary>
        ///    Indicates type of message object.
        /// </summary>
        /// <devdov>
        ///    Currently the actual data type is used, in future implementations
        ///    other means of identifying the type of message object may be
        ///    required.
        /// </devdov>
        public Type MessageType
        {
            get { return GetType(); }
        }
    }
}
