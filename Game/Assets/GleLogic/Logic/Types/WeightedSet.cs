using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Types;
using Logic.Core.Utility;

namespace Logic.Types
{
	public sealed class WeightedSet : IToString
	{
		public Money Denom { get; }
		public long BetMultiplier { get; }
		public Credits LinesBet { get; }
		public string Percentage { get; }
		public ulong Set1Weight { get; }
		public ulong Set2Weight { get; }

		public WeightedSet(Money denom, long betMultiplier, Credits linesBet, string percentage, ulong set1Weight, ulong set2Weight)
		{
			Denom = denom;
			BetMultiplier = betMultiplier;
			LinesBet = linesBet;
			Percentage = percentage;
			Set1Weight = set1Weight;
			Set2Weight = set2Weight;
		}

		public IResult ToString(string format) => $"{Percentage} {Denom.ToStringOrThrow("G")} {LinesBet.ToStringOrThrow("G")} {BetMultiplier} {Set1Weight} {Set2Weight}".ToSuccess();
	}

	public sealed class WeightedSets : IToString
	{
		private readonly IReadOnlyDictionary<string, WeightedSet> weightedSets;

		public static WeightedSets CreateFrom(IReadOnlyList<WeightedSet> list) => new WeightedSets(list.ToDictionary(ws => $"{ws.Denom.ToCents()}{ws.BetMultiplier}{ws.LinesBet.ToUInt64()}{ws.Percentage}"));

		public WeightedSets(IReadOnlyDictionary<string, WeightedSet> weightedSets) => this.weightedSets = weightedSets;

		public WeightedSet GetItem(Money denom, long betMultiplier, Credits linesBet, string percentage)
		{
			var key = $"{denom.ToCents()}{betMultiplier}{linesBet.ToUInt64()}{percentage}";

			if (!weightedSets.TryGetValue(key, out var data))
				throw new Exception($"No match found for a weighted set: {denom.ToCents()} {percentage} {linesBet.ToUInt64()} {betMultiplier}");

			return data;
		}

		public IResult ToString(string format)
		{
			if (format != "ML")
				return new NotSupported();

			var arr = new List<List<string>> { new List<string>() { "Percentage", "Denom", "LinesBet", "BetMultiplier", "Set1", "Set2" } };
			arr.AddRange(weightedSets.Select(weightedSet => new List<string>
			{
				$"{weightedSet.Value.Percentage}",
				$"{weightedSet.Value.Denom.ToStringOrThrow("G")}",
				$"{weightedSet.Value.LinesBet.ToStringOrThrow("G")}",
				$"{weightedSet.Value.BetMultiplier}",
				$"{weightedSet.Value.Set1Weight}",
				$"{weightedSet.Value.Set2Weight}"
			}));

			return arr.ToTableResult();
		}
	}
}