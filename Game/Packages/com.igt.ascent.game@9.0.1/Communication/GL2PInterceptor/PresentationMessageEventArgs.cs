//-----------------------------------------------------------------------
// <copyright file = "PresentationMessageEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using Presentation.CommServices;

    /// <summary>
    /// Event argument that contains a reference to a PresentationGenericMsg object.
    /// </summary>
    public class PresentationMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor for PresentationMessageEventArgs.
        /// </summary>
        /// <param name="message">GameLogicGenericMsg object.</param>
        /// <exception cref="ArgumentNullException">Thrown message parameter is null.</exception>
        public PresentationMessageEventArgs(PresentationGenericMsg message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message", "Message argument cannot be null.");
            }

            Message = message;
        }

        /// <summary>
        /// Gets PresentationGenericMsg object.
        /// </summary>
        public PresentationGenericMsg Message { get; private set; }
    }
}
