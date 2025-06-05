using System.Collections.Generic;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// A prize that will be awarded by an Evaluator.
	/// Each prize hit is sequential from the StartingHitCount.
	/// E.g. 5 KING pays 500, 4 KING pays 100, 3 KING pays 50 would be
	/// StartingHitCount: 3 PrizePays: 50, 100, 500
	/// </summary>
	public sealed class MaskPrize : IToString
	{
		#region Properties

		/// <summary>
		/// The unique name of the prize.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The strategy to use for evaluating a prize.
		/// Current recognised values are: Any, Left, Right, Group and Both
		/// </summary>
		public PrizeStrategy Strategy { get; }

		/// <summary>
		/// A list of pay values that can be awarded for the prize.
		/// </summary>
		public IReadOnlyList<int> PrizePays { get; }

		/// <summary>
		/// The lowest hit count in this mask prize.
		/// </summary>
		public int StartingHitCount { get; }

		/// <summary>
		/// A list of symbol indexes that are desired or required to award prize.
		/// </summary>
		public IReadOnlyList<int> Symbols { get; }

		/// <summary>
		/// A list of symbol indexes that are desired or required to award prize.
		/// </summary>
		public IReadOnlyList<int> RequiredSymbolCounts { get; }

		#endregion

		#region Construction

		/// <summary>
		/// Constructor to initialise any required data.
		/// </summary>
		/// <param name="name">Name of the prize.</param>
		/// <param name="strategy">Prize strategy.</param>
		/// <param name="startingHitCount">The lowest count that will be considered a hit.</param>
		/// <param name="prizePays">The list of pays.</param>
		/// <param name="symbols">The list of symbols.</param>
		/// <param name="requiredSymbolCounts">The minimum number of symbols required</param>
		public MaskPrize(string name, PrizeStrategy strategy, int startingHitCount, IReadOnlyList<int> prizePays, IReadOnlyList<int> symbols, IReadOnlyList<int> requiredSymbolCounts)
		{
			Name = name;
			Strategy = strategy;
			StartingHitCount = startingHitCount;
			PrizePays = prizePays;
			Symbols = symbols;
			RequiredSymbolCounts = requiredSymbolCounts;
		}

		#endregion

		#region IToString

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format) => new NotSupported();

		/// <summary>Implementation of IToString.ListToString(object, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult ListToString(object list, string format)
		{
			if (format == "ML" && list is IReadOnlyList<MaskPrize> prizes)
			{
				return prizes.ToStringArrays(
						new[] { "Name", "Strategy", "1st Pay", "Pays", "Subs" },
						p => new[] { p.Name, p.Strategy.ToString(), p.StartingHitCount.ToString(), string.Join(" ", p.PrizePays), string.Join(" ", p.Symbols) })
					.ToTableResult();
			}

			return new NotSupported();
		}

		#endregion
	}
}