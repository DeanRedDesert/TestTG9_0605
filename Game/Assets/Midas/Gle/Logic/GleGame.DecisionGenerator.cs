using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.DecisionGenerator;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.WinCheck;
using Midas.Gle.LogicToPresentation;

namespace Midas.Gle.Logic
{
	public sealed partial class GleGame
	{
		/// <summary>
		/// Decision generator that is used to initialise persistent data in the game after a RAM clear.
		/// The process is real random numbers are selected for all decisions except for the user decisions where the default values from SimpleDecisionGenerator.
		/// </summary>
		private sealed class PersistentInitDecisionGen : IDecisionGenerator
		{
			private readonly DecisionGenerator decisionGen;
			private readonly SceneInitDecisionGen sceneInitDecisionGen;

			public PersistentInitDecisionGen(RandomNumberGenerator rng)
			{
				decisionGen = new DecisionGenerator(rng);
				sceneInitDecisionGen = new SceneInitDecisionGen();
			}

			public bool GetDecision(ulong trueWeight, ulong falseWeight, Func<string> getContext)
			{
				return decisionGen.GetDecision(trueWeight, falseWeight, getContext);
			}

			public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				return decisionGen.ChooseIndexes(indexCount, count, allowDuplicates, getName, getContext);
			}

			public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<string> getContext)
			{
				return decisionGen.ChooseIndexes(indexCount, count, allowDuplicates, getWeight, totalWeight, getName, getContext);
			}

			public IReadOnlyList<ulong> ChooseIndexes(IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				return decisionGen.ChooseIndexes(weights, count, allowDuplicates, getName, getContext);
			}

			public IReadOnlyList<ulong> PickIndexes(ulong indexCount, uint minCount, uint maxCount, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				return sceneInitDecisionGen.PickIndexes(indexCount, minCount, maxCount, allowDuplicates, getName, getContext);
			}
		}

		/// <summary>
		/// Used to initialise a stage for presentation of a stage before the first real stage result has been generated.
		/// </summary>
		public sealed class SceneInitDecisionGen : IDecisionGenerator
		{
			public bool GetDecision(ulong trueWeight, ulong falseWeight, Func<string> getContext) => false;

			public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				return CreateDummyIndexes(count, allowDuplicates);
			}

			public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<string> getContext)
			{
				return CreateDummyIndexes(count, allowDuplicates);
			}

			public IReadOnlyList<ulong> ChooseIndexes(IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				return CreateDummyIndexes(count, allowDuplicates);
			}

			public IReadOnlyList<ulong> PickIndexes(ulong indexCount, uint minCount, uint maxCount, bool allowDuplicates, Func<ulong, string> getName,
				Func<string> getContext)
			{
				return CreateDummyIndexes(minCount, allowDuplicates);
			}

			private static IReadOnlyList<ulong> CreateDummyIndexes(ulong count, bool allowDuplicates)
			{
				var result = new ulong[count];

				if (!allowDuplicates)
				{
					for (var i = 0u; i < count; i++)
						result[(int)i] = i;
				}

				return result;
			}
		}

		/// <summary>
		/// Decision generator that returns player choices when requested but otherwise just makes normal decisions.
		/// </summary>
		public sealed class RuntimeDecisionGen : IDecisionGenerator
		{
			private readonly IDecisionGenerator decisionGen;
			private readonly IReadOnlyDictionary<string, GleUserSelection> userDecisions;
			private readonly bool isSkippingFeature;

			public RuntimeDecisionGen(RandomNumberGenerator rng, IReadOnlyDictionary<string, GleUserSelection> userDecisions, bool isSkippingFeature)
			{
				decisionGen = new DecisionGenerator(rng);
				this.userDecisions = userDecisions;
				this.isSkippingFeature = isSkippingFeature;
			}

			#region Implementation of IDecisionGenerator

			bool IDecisionGenerator.GetDecision(ulong trueWeight, ulong falseWeight, Func<string> getContext)
			{
				return decisionGen.GetDecision(trueWeight, falseWeight, getContext);
			}

			IReadOnlyList<ulong> IDecisionGenerator.ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				return decisionGen.ChooseIndexes(indexCount, count, allowDuplicates, getName, getContext);
			}

			IReadOnlyList<ulong> IDecisionGenerator.ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<string> getContext)
			{
				return decisionGen.ChooseIndexes(indexCount, count, allowDuplicates, getWeight, totalWeight, getName, getContext);
			}

			public IReadOnlyList<ulong> ChooseIndexes(IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				return decisionGen.ChooseIndexes(weights, count, allowDuplicates, getName, getContext);
			}

			IReadOnlyList<ulong> IDecisionGenerator.PickIndexes(ulong indexCount, uint minCount, uint maxCount, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				if (isSkippingFeature)
					return ((IDecisionGenerator)this).ChooseIndexes(indexCount, minCount, allowDuplicates, getName, getContext);

				var context = getContext();

				if (userDecisions == null || !userDecisions.TryGetValue(context, out var indexes))
					throw new InvalidOperationException($"Missing player decision {context}");

				if (indexes.Selections.Count < minCount)
					throw new InvalidOperationException($"Not enough selections made for player decision {context}. Required min: {minCount}, Selections: {indexes.Selections.Count}");

				if (indexes.Selections.Count > maxCount)
					throw new InvalidOperationException($"Too many selections made for player decision {context}. Required max: {maxCount}, Selections: {indexes.Selections.Count}");

				return indexes.Selections.Select(v => (ulong)v).ToArray();
			}

			#endregion
		}

		/// <summary>
		/// Decision generator that returns generated player choices when requested but otherwise just makes normal decisions.
		/// Used for generating the final result of a gaff.
		/// </summary>
		private sealed class GaffRuntimeDecisionGen : IDecisionGenerator
		{
			private readonly IDecisionGenerator decisionGen;

			public GaffRuntimeDecisionGen(RandomNumberGenerator rng)
			{
				decisionGen = new DecisionGenerator(rng);
			}

			#region Implementation of IDecisionGenerator

			bool IDecisionGenerator.GetDecision(ulong trueWeight, ulong falseWeight, Func<string> getContext)
			{
				return decisionGen.GetDecision(trueWeight, falseWeight, getContext);
			}

			IReadOnlyList<ulong> IDecisionGenerator.ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				return decisionGen.ChooseIndexes(indexCount, count, allowDuplicates, getName, getContext);
			}

			IReadOnlyList<ulong> IDecisionGenerator.ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<string> getContext)
			{
				return decisionGen.ChooseIndexes(indexCount, count, allowDuplicates, getWeight, totalWeight, getName, getContext);
			}

			public IReadOnlyList<ulong> ChooseIndexes(IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				return decisionGen.ChooseIndexes(weights, count, allowDuplicates, getName, getContext);
			}

			IReadOnlyList<ulong> IDecisionGenerator.PickIndexes(ulong indexCount, uint minCount, uint maxCount, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				var count = decisionGen.ChooseIndexes(maxCount - minCount + 1, 1, false, i => (i + minCount).ToString(), () => getContext() + "_count")[0] + minCount;
				var indexes = decisionGen.ChooseIndexes(indexCount, (uint)count, allowDuplicates, i => i.ToString(), () => getContext() + "_selections");
				return indexes;
			}

			#endregion
		}

		/// <summary>
		/// Decision generator that gathers the player decisions needed for the next game cycle. If there are gaff random numbers
		/// available then the picks will be pre-selected, and they will appear in the PreGeneratedPicks object.
		/// </summary>
		private sealed class NextCyclePicksDecisionGen : IDecisionGenerator
		{
			private readonly IReadOnlyList<ulong> gaffRngs;
			private ulong rngIndex;
			private readonly List<PickIndexesDecision> orderedPicks = new List<PickIndexesDecision>();

			/// <summary>
			/// An ordered list of all requested decisions.
			/// </summary>
			public IReadOnlyList<PickIndexesDecision> OrderedPicks => orderedPicks;

			public Dictionary<string, IReadOnlyList<int>> PreGeneratedPicks { get; }

			public NextCyclePicksDecisionGen(IReadOnlyList<ulong> gaffRngs)
			{
				this.gaffRngs = gaffRngs;

				if (gaffRngs != null)
					PreGeneratedPicks = new Dictionary<string, IReadOnlyList<int>>();
			}

			/// <inheritdoc />
			public bool GetDecision(ulong trueWeight, ulong falseWeight, Func<string> getContext)
			{
				if (gaffRngs != null)
					rngIndex++;

				return false;
			}

			/// <inheritdoc />
			public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				if (gaffRngs != null)
					rngIndex += count;

				return CreateDummyIndexes(count, allowDuplicates);
			}

			/// <inheritdoc />
			public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<string> getContext)
			{
				if (gaffRngs != null)
					rngIndex += count;

				return CreateDummyIndexes(count, allowDuplicates);
			}

			/// <inheritdoc />
			public IReadOnlyList<ulong> ChooseIndexes(IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				if (gaffRngs != null)
					rngIndex += count;

				return CreateDummyIndexes(count, allowDuplicates);
			}

			public IReadOnlyList<ulong> PickIndexes(ulong indexCount, uint minCount, uint maxCount, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				var context = getContext();

				if (gaffRngs != null)
				{
					var count = minCount + gaffRngs[(int)rngIndex++];
					var picks = new int[count];
					for (var i = 0u; i < count; i++)
						picks[i] = (int)gaffRngs[(int)rngIndex++];

					PreGeneratedPicks.Add(context, picks);
				}

				orderedPicks.Add(new PickIndexesDecision(context, indexCount, minCount, maxCount, allowDuplicates, getName));

				return CreateDummyIndexes(minCount, allowDuplicates);
			}

			private static IReadOnlyList<ulong> CreateDummyIndexes(ulong count, bool allowDuplicates)
			{
				var result = new ulong[count];

				if (!allowDuplicates)
				{
					for (var i = 0u; i < count; i++)
						result[(int)i] = i;
				}

				return result;
			}
		}

		/// <summary>
		/// Records all decisions made in a game cycle so they can saved for recovery and history.
		/// </summary>
		private sealed class DecisionRecorder : IDecisionGenerator
		{
			private readonly List<ulong> decisions = new List<ulong>();
			private IDecisionGenerator decisionGen;

			public IReadOnlyList<ulong> Decisions => decisions;

			public void ChangeDecisionGenerator(IDecisionGenerator value) => decisionGen = value;

			#region Implementation of IDecisionGenerator

			public bool GetDecision(ulong trueWeight, ulong falseWeight, Func<string> getContext)
			{
				var d = decisionGen.GetDecision(trueWeight, falseWeight, getContext);
				decisions.Add(d ? 1u : 0);
				return d;
			}

			public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				var d = decisionGen.ChooseIndexes(indexCount, count, allowDuplicates, getName, getContext);
				decisions.AddRange(d);
				return d;
			}

			public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<string> getContext)
			{
				var d = decisionGen.ChooseIndexes(indexCount, count, allowDuplicates, getWeight, totalWeight, getName, getContext);
				decisions.AddRange(d);
				return d;
			}

			public IReadOnlyList<ulong> ChooseIndexes(IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				var d = decisionGen.ChooseIndexes(weights, count, allowDuplicates, getName, getContext);
				decisions.AddRange(d);
				return d;
			}

			public IReadOnlyList<ulong> PickIndexes(ulong indexCount, uint minCount, uint maxCount, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				var d = decisionGen.PickIndexes(indexCount, minCount, maxCount, allowDuplicates, getName, getContext);
				decisions.Add((ulong)d.Count);
				decisions.AddRange(d);
				return d;
			}

			#endregion
		}

		/// <summary>
		/// Replays decisions that were recorded with <see cref="DecisionRecorder"/>.
		/// </summary>
		private sealed class ReplayDecisionGen : IDecisionGenerator
		{
			private int index;
			private readonly IReadOnlyList<ulong> decisions;

			public ReplayDecisionGen(IReadOnlyList<ulong> decisions)
			{
				this.decisions = decisions;
			}

			public bool GetDecision(ulong trueWeight, ulong falseWeight, Func<string> getContext)
			{
				return decisions[index++] != 0;
			}

			public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				return GetDecisions(count);
			}

			public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<string> getContext)
			{
				return GetDecisions(count);
			}

			public IReadOnlyList<ulong> ChooseIndexes(IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				return GetDecisions(count);
			}

			public IReadOnlyList<ulong> PickIndexes(ulong indexCount, uint minCount, uint maxCount, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
			{
				return GetDecisions((uint)decisions[index++]);
			}

			private IReadOnlyList<ulong> GetDecisions(uint count)
			{
				var result = new ulong[(int)count];
				for (var i = 0; i < count; i++)
					result[i] = decisions[index++];

				return result;
			}
		}

		private sealed class GleGaffChecker : RandomNumberGenerator
		{
			private readonly IList<ulong> rngValues;
			private int counter;

			public GleGaffChecker(IList<ulong> rngValues)
			{
				this.rngValues = rngValues;
			}

			public bool CheckIfRngsOk()
			{
				if (counter == rngValues.Count)
					return true;

				if (counter < rngValues.Count)
					Log.Instance.Warn("UTP Gaff used too few RNG values. Check your configuration.");
				else if (counter > rngValues.Count)
					Log.Instance.Warn("UTP Gaff used too many RNG values. Check your configuration.");

				return false;
			}

			public override ulong NextULong()
			{
				var result = counter < rngValues.Count ? rngValues[counter] : 0;
				counter++;
				return result;
			}
		}

		private sealed class QuickRng : RandomNumberGenerator
		{
			private readonly Random random = new Random();

			public override ulong NextULong()
			{
				var bytes = new byte[8];
				random.NextBytes(bytes);
				return BitConverter.ToUInt64(bytes, 0);
			}
		}
	}
}