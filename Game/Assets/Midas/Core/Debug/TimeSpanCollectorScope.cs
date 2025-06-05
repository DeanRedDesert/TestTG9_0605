using System;

namespace Midas.Core.Debug
{
	public readonly struct TimeSpanCollectorScope : IDisposable
	{
		private readonly TimeSpanCollector collector;

		public TimeSpanCollectorScope(TimeSpanCollector collector)
		{
			this.collector = collector;
			this.collector.Start();
		}

		public void Dispose()
		{
			collector.Stop();
		}
	}
}