using System;
using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.LogicToPresentation.Messages;

namespace Midas.Presentation.Game
{
	/// <summary>
	/// Interface to allow different gaff handlers to be used from the <see cref="GameFlow"/> without it knowing the specifics of the gaff.
	/// </summary>
	public interface IGaffHandler
	{
		/// <summary>
		/// Is the gaff enabled.
		/// </summary>
		bool IsEnable { get; }

		/// <summary>
		/// The priority of the gaff handler and does not support dynamic changing. The higher the number the higher the priority.
		/// </summary>
		int Priority { get; }

		/// <summary>
		/// Run the coroutine to provide the gaff functionality
		/// </summary>
		/// <returns>A collection of coroutine instructions.</returns>
		IEnumerator<CoroutineInstruction> Run();
	}

	/// <summary>
	/// Message to allow a generic action to be run to allow gaffs not running on the presentation thread to make changes in a thread safe way.
	/// </summary>
	public sealed class DemoGaffActionMessage : DebugMessage
	{
		private readonly Action actionToRun;

		public DemoGaffActionMessage(Action actionToRun) => this.actionToRun = actionToRun;

		public void RunAction() => actionToRun();
	}

	/// <summary>
	/// Message to manage the <see cref="IGaffHandler"/>.
	/// </summary>
	public sealed class DemoGaffHandleMessage : DebugMessage
	{
		public IGaffHandler GaffHandler { get; }
		public bool IsActive { get; }

		public DemoGaffHandleMessage(IGaffHandler gaffHandler, bool isActive)
		{
			GaffHandler = gaffHandler;
			IsActive = isActive;
		}
	}
}