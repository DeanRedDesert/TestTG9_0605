//-----------------------------------------------------------------------
// <copyright file = "MessageAddedEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MessageStrip
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
	using System.Collections.Generic;

    /// <summary>
    /// Event arguments for UGP message being added.
    /// </summary>
    [Serializable]
    public class MessageAddedEventArgs : TransactionalEventArgs
    {
		/// <summary>
		/// The current messages.
		/// </summary>
		public List<string> Messages { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MessageAddedEventArgs(List<string> messages)
		{
			Messages = messages;
		}
    }
}
