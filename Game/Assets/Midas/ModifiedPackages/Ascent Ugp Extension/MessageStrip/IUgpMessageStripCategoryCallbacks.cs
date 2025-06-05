//-----------------------------------------------------------------------
// <copyright file = "IUgpMessageStripCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MessageStrip
{
	using System.Collections.Generic;

	/// <summary>
	/// Interface to accept callbacks from the UGP Message Strip category.
	/// </summary>
	public interface IUgpMessageStripCategoryCallbacks
	{
		/// <summary>
		/// Method called when UgpMessageStripCategory AddMessage message is received from the foundation.
		/// </summary>
		/// <param name="messages">The current list of messages.</param>
		/// <returns>
		/// An error message if an error occurs; otherwise, null.
		/// </returns>
		string ProcessAddMessage(List<string> messages);

		/// <summary>
		/// Method called when UgpMessageCategory RemoveMessage message is received from the foundation.
		/// </summary>
		/// <param name="messages">The current list of messages.</param>
		/// <returns>
		/// An error message if an error occurs; otherwise, null.
		/// </returns>
		string ProcessRemoveMessage(List<string> messages);
	}
}
