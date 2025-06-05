using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.LogicToPresentation;

namespace Midas.Presentation.Game
{
	/// <summary>
	/// Interface to allow the utility mode to be used from the <see cref="GameFlow"/> without it knowing the specifics of the utility mode.
	/// </summary>
	public interface IUtilityHandler
	{
		/// <summary>
		/// Is the gaff enabled.
		/// </summary>
		bool IsEnable { get; }

		/// <summary>
		/// Run the coroutine to provide the gaff functionality
		/// </summary>
		/// <returns>A collection of coroutine instructions.</returns>
		IEnumerator<CoroutineInstruction> Run();
	}

	public sealed class UtilityHandleMessage : IMessage
	{
		public IUtilityHandler UtilityHandler { get; }

		public UtilityHandleMessage(IUtilityHandler gaffHandler)
		{
			UtilityHandler = gaffHandler;
		}
	}
}