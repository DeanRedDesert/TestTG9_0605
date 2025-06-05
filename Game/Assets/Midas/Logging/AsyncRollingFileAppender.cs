using System;
using System.Collections.Concurrent;
using System.Threading;
using log4net.Appender;
using log4net.Core;

namespace Midas.Logging
{
	// ReSharper disable once UnusedType.Global - Constructed internally by log4net
	public sealed class AsyncRollingFileAppender : RollingFileAppender
	{
		#region Fields

		private readonly ConcurrentQueue<LoggingEvent> eventsToWrite = new ConcurrentQueue<LoggingEvent>();
		private readonly AutoResetEvent newEventArrivedEvent = new AutoResetEvent(false);
		private readonly AutoResetEvent exitEvent = new AutoResetEvent(false);
		private readonly Thread writerThread;

		#endregion

		#region Construction

		public AsyncRollingFileAppender()
		{
			writerThread = new Thread(WriteDataToFile)
			{
				Name = "AsyncRollingFileAppender"
			};
			writerThread.Start();
		}

		#endregion

		#region RollingFileAppender overrides

		protected override void Append(LoggingEvent loggingEvent)
		{
			// on fatal error we write directly and write older events also
			if (loggingEvent.Level.Value >= Level.Error.Value)
			{
				WriteEnqueuedEvents();
				base.Append(loggingEvent);
			}
			else
			{
				loggingEvent.Fix = FixFlags.LocationInfo | FixFlags.ThreadName;
				eventsToWrite.Enqueue(loggingEvent);
				newEventArrivedEvent.Set();
			}
		}

		protected override void Append(LoggingEvent[] loggingEvents)
		{
			foreach (var loggingEvent in loggingEvents)
				Append(loggingEvent);
		}

		protected override void OnClose()
		{
			exitEvent.Set();
			writerThread.Join();
			base.OnClose();
		}

		#endregion

		#region Private Methods

		private void WriteDataToFile()
		{
			try
			{
				var handles = new WaitHandle[] { newEventArrivedEvent, exitEvent };
				while (WaitHandle.WaitAny(handles) == 0)
					WriteEnqueuedEvents();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private void WriteEnqueuedEvents()
		{
			while (eventsToWrite.TryDequeue(out var loggingEvent))
				base.Append(loggingEvent);
		}

		#endregion
	}
}