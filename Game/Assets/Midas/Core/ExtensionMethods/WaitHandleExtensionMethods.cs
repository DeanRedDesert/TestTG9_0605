using System;
using System.Threading;

namespace Midas.Core.ExtensionMethods
{
	public static class WaitHandleExtensionMethods
	{
		// ReSharper disable InconsistentNaming
		private const uint WAIT_IO_COMPLETION = 0x000000c0;

		private const uint WAIT_FAILED = 0xFFFFFFFF;
		// ReSharper restore InconsistentNaming

		/// <summary>
		/// Waits for any of the handles in the <paramref name="waitHandles"/> array to be signaled or for the timeout
		/// to elapse.
		/// </summary>
		/// <remarks>
		/// This method is designed to replace <see cref="WaitHandle.WaitAny(WaitHandle[], Int32)"/> method.
		/// It fixes some threading bugs when running with Unity debugger.
		/// Please call this method instead whenever <see cref="WaitHandle.WaitAny(WaitHandle[], Int32)"/> is needed.
		/// </remarks>
		/// <param name="waitHandles">Wait handles to wait for a signal.</param>
		/// <param name="millisecondsTimeout">Timeout to wait for a signal.</param>
		/// <returns>The handle which was signaled, or null if the timeout elapses.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown <paramref name="waitHandles"/> is null.
		/// </exception>
		public static WaitHandle WaitAny(this WaitHandle[] waitHandles, int millisecondsTimeout)
		{
			if (waitHandles == null)
			{
				throw new ArgumentNullException(nameof(waitHandles));
			}

			WaitHandle result = null;
			var waiting = true;

			while (waiting)
			{
				// Always resume waiting for the original timeout after being interrupted.
				// This is to simplify implementation until further requests.
				// Otherwise we'll have to add timers in this function to remember
				// how long we had waited before the interruption.
				var signal = WaitHandle.WaitAny(waitHandles, millisecondsTimeout);

				switch ((uint)signal)
				{
					case WaitHandle.WaitTimeout:
						// When timeout is infinite, we should not receive TIMEOUT.
						// This is just a double check to make sure that we are not
						// returning null in case of infinite timeout.
						waiting = millisecondsTimeout == Timeout.Infinite;
						break;

					case WAIT_IO_COMPLETION:
					case WAIT_FAILED:
						// Wait has been interrupted.  Keep waiting.
						break;

					default:
						if (signal < waitHandles.Length)
						{
							result = waitHandles[signal];
							waiting = false;
						}

						break;
				}
			}

			return result;
		}
	}
}