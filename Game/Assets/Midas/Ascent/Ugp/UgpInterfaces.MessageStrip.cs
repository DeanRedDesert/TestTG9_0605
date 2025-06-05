using System;
using System.Collections.Generic;
using IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MessageStrip;
using static Midas.Ascent.AscentFoundation;

namespace Midas.Ascent.Ugp
{
	public sealed partial class UgpInterfaces
	{
		private IUgpMessageStrip messageStrip;

		/// <summary>
		/// Event raised when message is added.
		/// </summary>
		public event EventHandler<MessageAddedEventArgs> MessageAdded;

		/// <summary>
		/// Event raised when message is removed.
		/// </summary>
		public event EventHandler<MessageRemovedEventArgs> MessageRemoved;

		private void InitMessageStrip()
		{
			messageStrip = GameLib.GetInterface<IUgpMessageStrip>();

			if (messageStrip != null)
			{
				messageStrip.MessageAdded += OnMessageAdded;
				messageStrip.MessageRemoved += OnMessageRemoved;
			}
		}

		private void DeInitMessageStrip()
		{
			if (messageStrip != null)
			{
				messageStrip.MessageAdded -= OnMessageAdded;
				messageStrip.MessageRemoved -= OnMessageRemoved;
			}

			messageStrip = null;
		}

		/// <summary>
		/// Retrieve all the messages.
		/// </summary>
		/// <returns>A list of message string.</returns>
		public IEnumerable<string> GetMessages()
		{
			return messageStrip == null ? Array.Empty<string>() : messageStrip.GetMessages();
		}

		private void OnMessageAdded(object s, MessageAddedEventArgs e)
		{
			MessageAdded?.Invoke(s, e);
		}

		private void OnMessageRemoved(object s, MessageRemovedEventArgs e)
		{
			MessageRemoved?.Invoke(s, e);
		}
	}
}