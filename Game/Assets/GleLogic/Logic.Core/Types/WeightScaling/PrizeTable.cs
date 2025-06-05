using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logic.Core.DecisionGenerator;
using Logic.Core.Utility;
using Logic.Core.WinCheck;

namespace Logic.Core.Types.WeightScaling
{
	/// <summary>
	/// A weighted list of unique ids that are used by the weight distribution system to create a new weighted list adjusted by weight distribution entries based on bet related information.
	/// The chances of each id being chosen are adjusted based on bet related details. Each id in this list has an associated scaling method.
	/// Any entries with a scaling method that is not 'None' will have the weight adjusted to ensure their probability of being chosen scales according to their scaling type.
	/// All entries without will be adjusted to ensure that the total weight does not change.  The adjustments process uses a set of weight distributor entries that are paired with the PrizeTable in the <see cref="PrizeReplacerEntry"/> class.
	/// </summary>
	public sealed class PrizeTable : IWeights, IToString
	{
		private readonly IStrip strip;
		private readonly IReadOnlyList<ScalingMethod> scalingMethods;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="strip">The weighted strip of unique ids.</param>
		/// <param name="scalingMethods">The ordered list of scaling methods associated with each id.</param>
		public PrizeTable(IStrip strip, IReadOnlyList<ScalingMethod> scalingMethods)
		{
			this.strip = strip;
			this.scalingMethods = scalingMethods;
		}

		/// <summary>
		/// Return the <see cref="ScalingMethod"/> associated with the id at this index.
		/// </summary>
		public ScalingMethod GetScalingMethod(ulong index) => scalingMethods[(int)index];

		/// <inheritdoc />
		public ulong GetTotalWeight() => strip.GetTotalWeight();

		/// <inheritdoc />
		public ulong GetIndexAtWeight(ulong weight) => strip.GetIndexAtWeight(weight);

		/// <inheritdoc />
		public ulong GetIndexAtWeight(ulong weight, ICollection<ulong> indexesToSkip) => strip.GetIndexAtWeight(weight, indexesToSkip);

		/// <inheritdoc />
		public ulong GetLength() => strip.GetLength();

		/// <inheritdoc />
		public ulong GetWeight(ulong index) => strip.GetWeight(index);

		/// <summary>
		/// Get the id at the specified index.
		/// </summary>
		public string GetId(ulong index) => strip.GetSymbol(index);

		/// <inheritdoc />
		public IResult ToString(string format)
		{
			var rows = new List<(string name, string weight, string scalingMethod)>();

			for (var i = 0UL; i < GetLength(); i++)
				rows.Add((GetId(i), GetWeight(i).ToString(), GetScalingMethod(i).ToString()));

			var maxNameStrLen = rows.Max(r => r.name.Length);
			var maxWeightStrLen = rows.Max(r => r.weight.Length);
			var maxScalingMethodStrLen = rows.Max(r => r.scalingMethod.Length);
			var sb = new StringBuilder();

			maxNameStrLen = maxNameStrLen < 4 ? 4 : maxNameStrLen;
			maxWeightStrLen = maxWeightStrLen < 6 ? 6 : maxWeightStrLen;
			maxScalingMethodStrLen = maxScalingMethodStrLen < 13 ? 13 : maxScalingMethodStrLen;

			sb.Append("Name".PadRight(maxNameStrLen) + " ");
			sb.Append("Weight".PadRight(maxWeightStrLen) + " ");
			sb.Append("ScalingMethod".PadRight(maxScalingMethodStrLen));
			sb.AppendLine();

			foreach (var row in rows)
			{
				sb.Append(row.name.PadRight(maxNameStrLen) + " ");
				sb.Append(row.weight.PadLeft(maxWeightStrLen) + " ");
				sb.Append(row.scalingMethod.PadRight(maxScalingMethodStrLen));
				sb.AppendLine();
			}

			return sb.ToString().ToSuccess();
		}
	}
}