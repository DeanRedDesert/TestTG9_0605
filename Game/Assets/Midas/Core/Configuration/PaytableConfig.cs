using System.Collections.Generic;

namespace Midas.Core.Configuration
{
	public class PaytableConfig
	{
		public IReadOnlyList<string> PaytableFilenames { get; }
		public IReadOnlyList<int> ActivePaytables { get; }

		public PaytableConfig(IReadOnlyList<string> paytableFilenames, IReadOnlyList<int> activePaytables)
		{
			PaytableFilenames = paytableFilenames;
			ActivePaytables = activePaytables;
		}
	}
}