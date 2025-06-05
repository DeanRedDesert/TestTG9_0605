//-----------------------------------------------------------------------
// <copyright file = "StandaloneUgpMessageStrip.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MessageStrip
{
    using System;
    using System.Collections.Generic;
    using Interfaces;

    /// <summary>
    /// Standalone implementation of the UgpMessageStrip extended interface.
    /// </summary>
    internal sealed class StandaloneUgpMessageStrip : IUgpMessageStrip, IInterfaceExtension
    {
        #region IUgpMessageStrip Implementation

        /// <inheritdoc/>
        /// <remarks>
        /// This event has empty add/remove methods to indicate it is unimplemented. This will prevent any warnings
        /// about the member being unused. If the event is implemented, then it should be changed to be auto-implemented.
        /// </remarks>
        public event EventHandler<MessageAddedEventArgs> MessageAdded
        {
            add
            {
            }
            remove
            {
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This event has empty add/remove methods to indicate it is unimplemented. This will prevent any warnings
        /// about the member being unused. If the event is implemented, then it should be changed to be auto-implemented.
        /// </remarks>
        public event EventHandler<MessageRemovedEventArgs> MessageRemoved
        {
            add
            {
            }
            remove
            {
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetMessages()
        {
            return new List<string>();
        }

        #endregion
    }
}