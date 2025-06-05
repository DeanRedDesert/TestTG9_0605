using System;
using UnityEngine;

namespace Midas.Presentation.Sequencing
{
	/// <summary>
	/// Designed to cause all things that are basically "run forever" during a sequence to finish as soon as they can and let the sequence continue.
	/// </summary>
	public interface ICompletionNotifier
	{
		event Action<ICompletionNotifier> Complete;
	}

	/// <summary>
	/// MonoBehaviour version of a completion notifier.
	/// </summary>
	public abstract class CompletionNotifier : MonoBehaviour, ICompletionNotifier
	{
		public event Action<ICompletionNotifier> Complete;

		/// <summary>
		/// Raises the complete event.
		/// </summary>
		protected void PostComplete()
		{
			Complete?.Invoke(this);
		}
	}
}