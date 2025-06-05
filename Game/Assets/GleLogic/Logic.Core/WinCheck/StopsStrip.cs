using System;
using System.Collections.Generic;
using Logic.Core.Types;
using Logic.Core.Utility;

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// Used to support collections of Stop objects.  Can be constructed in zero weight support or not.
	/// If true then the optimization in GetIndexAtWeight is disabled.  If false an exception is thrown on construction
	/// when a zero weighted stop is encountered.
	/// </summary>
	public sealed class StopsStrip : IStrip, IToString, IToCode
	{
		#region Fields

		private readonly IReadOnlyList<Stop> stops;
		private readonly bool supportZeroWeight;
		private readonly ulong totalWeight;
		private readonly Func<ulong, ulong> getWeight;

		#endregion

		#region Construction

		public StopsStrip(IReadOnlyList<Stop> stops, bool supportZeroWeight = false)
		{
			this.stops = stops;
			this.supportZeroWeight = supportZeroWeight;

			totalWeight = 0UL;

			foreach (var stop in stops)
			{
				if (!supportZeroWeight && stop.Weight == 0)
					throw new ArgumentException("Individual weights cannot be zero.");

				totalWeight += stop.Weight;
			}

			if (totalWeight == 0UL)
				throw new ArgumentException("TotalWeight cannot be zero.");

			getWeight = w => stops[(int)w].Weight;
		}

		public static StopsStrip Create(string stops, bool supportsZeroWeight)
		{
			var st = stops.FromStringOrThrow<IReadOnlyList<Stop>>("SL");
			return new StopsStrip(st, supportsZeroWeight);
		}

		#endregion

		#region Implementation of IStrip

		/// <inheritdoc />
		public ulong GetTotalWeight() => totalWeight;

		/// <inheritdoc />
		public ulong GetIndexAtWeight(ulong weight)
		{
			if (weight >= totalWeight)
				throw new ArgumentException("Weight value exceeds total weight!");

			if (!supportZeroWeight && totalWeight == (ulong)stops.Count)
				return weight;

			return StripHelper.GetIndexAtWeight(weight, (ulong)stops.Count, getWeight);
		}

		/// <inheritdoc />
		public ulong GetIndexAtWeight(ulong weight, ICollection<ulong> preChosenIndexes)
		{
			if (weight >= totalWeight)
				throw new ArgumentException("Weight value exceeds total weight!");

			return StripHelper.GetIndexAtWeight(weight, (ulong)stops.Count, preChosenIndexes, getWeight);
		}

		/// <inheritdoc />
		public ulong GetLength() => (ulong)stops.Count;

		/// <inheritdoc />
		public ulong GetWeight(ulong index) => stops[(int)index].Weight;

		/// <inheritdoc />
		public string GetSymbol(ulong index) => stops[(int)index].Symbol;

		#endregion

		#region Implementation of IToString

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			return this.StripToString(format);
		}

		#endregion

		#region Implementation of IToCode

		/// <inheritdoc cref="IToCode.ToCode(CodeGenArgs?)" />
		public IResult ToCode(CodeGenArgs args)
		{
			if (!StringConverter.TryToString(stops, "SL", out var s1))
				return new Error(s1);

			var s2 = CodeConverter.ToCodeOrThrow(args, supportZeroWeight);
			return $"{CodeConverter.ToCode<StopsStrip>(args)}.{nameof(Create)}(\"{s1}\", {s2})".ToSuccess();
		}

		#endregion
	}
}