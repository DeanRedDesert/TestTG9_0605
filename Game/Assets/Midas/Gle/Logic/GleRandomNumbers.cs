using System.Collections.Generic;
using Logic.Core.DecisionGenerator;
using Midas.Core;

namespace Midas.Gle.Logic
{
	internal sealed class GleRandomNumbers : RandomNumberGenerator
	{
		#region Fields

		private readonly FoundationRng rng;

		#endregion

		#region Construction

		/// <summary>
		/// Create the RNG provider using a list of existing values or requesting RNG values from the foundation.
		/// </summary>
		/// <param name="foundation">The gamelib to get RNG values from the foundation.</param>
		/// <param name="usedValues">The list of used values. Can be null or empty</param>
		/// <returns>An object that will return the provided RNG values </returns>
		/// <remarks>The creation order will use the provided values first and if there are no values provided it will automatically detect which foundation is running and use the appropriate request mechanism.</remarks>
		public static GleRandomNumbers Create(IFoundationShim foundation, IReadOnlyList<ulong> usedValues)
		{
			if (usedValues != null && usedValues.Count != 0)
				return new GleRandomNumbers(new StoredFoundationRng(usedValues));

			return new GleRandomNumbers(new AscentFoundationRng(foundation));
		}

		public GleRandomNumbers(FoundationRng rng)
		{
			this.rng = rng;
		}

		#endregion

		#region RandomNumberGenerator Overrides

		public override ulong NextULong() => rng.NextULong();

		#endregion

		#region RNG Classes

		#region FoundationRng Class

		/// <summary>
		/// Provides a way to abstract the different way of generating RNG values and stored the used values.
		/// </summary>
		public abstract class FoundationRng
		{
			public abstract ulong NextULong();
		}

		#endregion

		#region AscentFoundationRng Class

		/// <summary>
		/// Used for requesting RNG values from the Ascent Foundation. Games running on the Ascent foundation must use the minimum number of calls. We need to detect when the required numbers are less than int.Max and only use
		/// 1 call for the RNG instead of generating a double RNG value.
		/// </summary>
		public sealed class AscentFoundationRng : FoundationRng
		{
			private readonly IFoundationShim foundation;
			private readonly List<ulong> values = new List<ulong>();
			private const int RngsToRequest = 15;

			public AscentFoundationRng(IFoundationShim foundation)
			{
				this.foundation = foundation;
			}

			public override ulong NextULong()
			{
				if (values.Count == 0)
				{
					OptimalRequestCheck();

					var numbers = foundation.GetRandomNumbers(RngsToRequest * 2, 0, 0x3FFFFFFF);
					for (var i = 0; i < RngsToRequest; i++)
						values.Add((uint)numbers[i * 2] | ((ulong)numbers[i * 2 + 1] << 32));
				}

				var randomNumber = values[0];
				values.RemoveAt(0);
				return randomNumber;
			}

			[System.Diagnostics.Conditional("UNITY_EDITOR")]
			private void OptimalRequestCheck()
			{
				if (values.Capacity > RngsToRequest)
					Log.Instance.Warn($"Multiple requests for {RngsToRequest} RNG numbers have been made in a game cycle. If this warning is seen frequently, increase AscentFoundationRng.RngsToRequest to increase batching of RNGs and reduce the number of calls to the foundation.");
			}
		}

		#endregion

		#region StoredFoundationRng Class

		public sealed class StoredFoundationRng : FoundationRng
		{
			private readonly IReadOnlyList<ulong> rngValues;
			private int counter;

			public StoredFoundationRng(IReadOnlyList<ulong> rngValues)
			{
				this.rngValues = rngValues;
			}

			public override ulong NextULong()
			{
				return rngValues[counter++];
			}
		}

		#endregion

		#endregion
	}
}