using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// A symbol that triggers a prize.
	/// </summary>
	public sealed class PrizeSymbol : IToString, IFromString
	{
		/// <summary>
		/// The unique name of the symbol (declared in Stop.Name) that will trigger the prize.
		/// </summary>
		public string SymbolName { get; }

		/// <summary>
		/// The number of instances of the symbol that are required to trigger the prize.
		/// The default value 0 indicates that the Symbol is desired but not required.
		/// </summary>
		public int Required { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="symbolName">The name of the symbol.</param>
		/// <param name="required">The number required to trigger the prize.</param>
		public PrizeSymbol(string symbolName, int required)
		{
			SymbolName = symbolName;
			Required = required;
		}

		#region IToString Members

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format) => $"{SymbolName}{(Required > 0 ? $":{Required}" : "")}".ToSuccess();

		#endregion

		#region IFromString Members

		/// <summary>Implementation of IFromString.FromString(string?, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult FromString(string s, string format)
		{
			var sp = s.Split(':');

			// .Net 4.8 doesn't support int.TryParse(string, CultureInfo, out int)
			switch (sp.Length)
			{
				case 1: return new PrizeSymbol(sp[0], 0).ToSuccess();
				case 2 when int.TryParse(sp[1], out var v): return new PrizeSymbol(sp[0], v).ToSuccess();
				default: return new Error($"Could not convert '{s}' to a PrizePay value");
			}
		}

		#endregion
	}
}