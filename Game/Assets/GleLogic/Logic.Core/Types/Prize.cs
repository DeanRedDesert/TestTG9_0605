using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// The symbol selection strategies available for prizes.
	/// </summary>
	public enum PrizeStrategy
	{
		/// <summary>
		/// Winning symbols are grouped and the group is left aligned on the pattern.
		/// </summary>
		Left,

		/// <summary>
		/// Winning symbols are found anywhere on the pattern.
		/// </summary>
		Any,

		/// <summary>
		/// Winning symbols are grouped and the group is right aligned on the pattern.
		/// </summary>
		Right,

		/// <summary>
		/// Winning symbols are grouped and the group is left and right aligned on the pattern.
		/// </summary>
		Both
	}

	/// <summary>
	/// A prize that will be awarded by an Evaluator.
	/// </summary>
	public sealed class Prize : IToString, IFromString
	{
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
		public IReadOnlyList<PrizePay> PrizePays { get; }

		/// <summary>
		/// A list of symbols that are desired or required to award prize.
		/// </summary>
		public IReadOnlyList<PrizeSymbol> Symbols { get; }

		/// <summary>
		/// Constructor to initialise any required data.
		/// </summary>
		/// <param name="name">Name of the prize.</param>
		/// <param name="strategy">Prize strategy.</param>
		/// <param name="prizePays">The list of pays.</param>
		/// <param name="symbols">The list of symbols.</param>
		public Prize(string name, PrizeStrategy strategy, IReadOnlyList<PrizePay> prizePays, IReadOnlyList<PrizeSymbol> symbols)
		{
			Name = name;
			Strategy = strategy;
			PrizePays = prizePays;
			Symbols = symbols;
		}

		#region IToString Members

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			var pays = string.Join(" ", PrizePays.Select(pp => $"{pp.Count}:{pp.Value}"));
			var subs = string.Join(" ", Symbols.Select(s => $"{s.SymbolName}{(s.Required > 0 ? $":{s.Required}" : "")}"));
			return $"{Name} {Strategy} {pays} {subs}".ToSuccess();
		}

		/// <summary>Implementation of IToString.ListToString(object, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult ListToString(object list, string format)
		{
			if (format == "ML" && list is IReadOnlyList<Prize> prizes)
			{
				return prizes.ToStringArrays(
						new[] { "Name", "Strategy", "Pays", "Subs" },
						p => new[] { p.Name, p.Strategy.ToString(), Pays(p.PrizePays), Symbols(p.Symbols) })
					.ToTableResult();

				string Pays(IReadOnlyList<PrizePay> ps) => string.Join(" ", ps.Select(p => p.ToStringOrThrow("SL")));
				string Symbols(IReadOnlyList<PrizeSymbol> ss) => string.Join(" ", ss.Select(p => p.ToStringOrThrow("SL")));
			}

			return new NotSupported();
		}

		#endregion

		#region IFromString Members

		/// <summary>Implementation of IFromString.FromString(string?, string?)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult FromString(string s, string format)
		{
			var sp = s.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).ToArray();
			var pays = new List<PrizePay>();
			var symbols = new List<PrizeSymbol>();

			for (var i = 2; i < sp.Length; i++)
			{
				var token = sp[i];

				if (token.Contains(':'))
				{
					var sp2 = token.Split(':');

					if (int.TryParse(sp2[0], out var count))
						pays.Add(new PrizePay(count, int.Parse(sp2[1])));
					else
						symbols.Add(new PrizeSymbol(sp2[0], int.Parse(sp2[1])));
				}
				else
				{
					symbols.Add(new PrizeSymbol(token, 0));
				}
			}

			return new Prize(sp[0], (PrizeStrategy)Enum.Parse(typeof(PrizeStrategy), sp[1]), pays, symbols).ToSuccess();
		}

		/// <summary>Implementation of IToString.ListFromString(string?, string?, Type)</summary>
		// ReSharper disable once UnusedMember.Global
		public static IResult ListFromString(string s, string format, Type listType)
		{
			var lines = s.ToLines();
			var arr = new Prize[lines.Count - 1];

			for (var i = 1; i < lines.Count; i++)
			{
				var r = StringConverter.FromString(lines[i].Replace("|", ""), "SL", typeof(Prize));

				if (r is ObjectSuccess o)
					arr[i - 1] = (Prize)o.Value;
				else
					return r;
			}

			return new ObjectSuccess(listType == typeof(List<Prize>) ? (object)arr.ToList() : arr);
		}

		#endregion
	}
}