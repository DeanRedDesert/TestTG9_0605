using System;
using System.Linq;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// A single stop that is placed on a strip.
	/// </summary>
	public sealed class Stop : IToString, IFromString
	{
		/// <summary>
		/// A unique symbol name.
		/// This will be used in evaluators to check if a prize has hit.
		/// </summary>
		public string Symbol { get; }

		/// <summary>
		/// The weight of the stop. Default is 1.
		/// By increasing the Weight you will increase the chance that the stop will be selected in a populator (proportional to
		/// the weights of the other stops on the strip).
		/// </summary>
		public ulong Weight { get; }

		/// <summary>
		/// Constructor to initialise any required data.
		/// </summary>
		/// <param name="symbol">The symbol name.</param>
		/// <param name="weight">The weight.</param>
		public Stop(string symbol, ulong weight)
		{
			Symbol = symbol;
			Weight = weight;
		}

		#region IToString Members

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format) => Weight == 1 ? Symbol.ToSuccess() : $"{Symbol}:{Weight}".ToSuccess();

		#endregion

		#region IFromString Members

		/// <summary>Implementation of IFromString.FromString(string?, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		// ReSharper disable once UnusedParameter.Global
		public static IResult FromString(string s, string format)
		{
			var sp = s.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).ToArray();

			// .Net 4.8 doesn't support int.TryParse(string, CultureInfo, out int)
			switch (sp.Length)
			{
				case 1: return new Stop(sp[0], 1).ToSuccess();
				case 2 when ulong.TryParse(sp[1], out var w): return new Stop(sp[0], w).ToSuccess();
				default: return new Error($"Could not convert '{s}' to a Stop value");
			}
		}

		#endregion
	}
}