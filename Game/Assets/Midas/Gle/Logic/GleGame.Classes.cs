using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gaff.Core;
using Gaff.Core.Conditions;
using Gaff.Core.DecisionMakers;
using Gaff.Core.GaffEditor;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.Engine;
using Logic.Core.Types;
using Logic.Core.Utility;
using Midas.Core;
using Midas.Core.General;
using Midas.Core.Serialization;
using Midas.Gle.LogicToPresentation;

namespace Midas.Gle.Logic
{
	public partial class GleGame
	{
		internal sealed class GleStakeCombination : IStakeCombination
		{
			private static readonly IReadOnlyDictionary<string, Stake> nameToStakeMapping = new Dictionary<string, Stake>
			{
				{ "BetMultiplier", Stake.BetMultiplier },
				{ "LinesBet", Stake.LinesBet },
				{ "Multiway", Stake.Multiway },
				{ "AnteBet", Stake.AnteBet }
			};

			public IReadOnlyDictionary<Stake, long> Values { get; }

			public Inputs Inputs { get; }

			public Credit TotalBet { get; }

			public BetCategory BetCategory { get; }

			public GleStakeCombination(Inputs inputs, BetCategory betCategory)
			{
				Stake GetStake(string inputName)
				{
					if (!nameToStakeMapping.TryGetValue(inputName, out var stake))
						throw new InvalidOperationException($"{inputName} is not a supported logic input");

					return stake;
				}

				var values = new Dictionary<Stake, long>();

				var configNames = GleGameData.InputSets.GetConfigurationInputNames();
				var justInputs = GleGameData.InputSets.GetInputNames().Where(n => !configNames.Contains(n));

				foreach (var name in justInputs)
				{
					if (inputs.TryGetInput(name, out var val))
					{
						switch (val)
						{
							case long l:
								values[GetStake(name)] = l;
								break;
							case Credits c:
								values[GetStake(name)] = (long)c.ToUInt64();
								break;
							default: throw new InvalidOperationException($"{name} is not a valid supported logic input type ({val.GetType().Name})");
						}
					}
				}

				Inputs = inputs;
				BetCategory = betCategory;
				Values = values;
				TotalBet = Credit.FromLong((long)((Credits)inputs.GetInput("TotalBet")).ToUInt64());
			}

			public override string ToString()
			{
				var sb = new StringBuilder();

				sb.Append($"Bet {TotalBet.Credits}:");
				foreach (var kvp in Values)
					sb.Append($" {kvp.Key}={kvp.Value}");

				return sb.ToString();
			}
		}

		private sealed class GleGaffSequence : IGaffSequence
		{
			public GaffSequence GaffSequence { get; }

			public string Name => GaffSequence.Title;
			public GaffType GaffType { get; }

			public GleGaffSequence(GaffSequence gaffSequence)
			{
				GaffSequence = gaffSequence;
				GaffType = GaffSequence.DeveloperOnly ? GaffType.Development : GaffType.Show;
			}

			public override string ToString() => $"{Name}{(GaffSequence.DeveloperOnly ? " (Dev)" : "")}";
		}

		private sealed class GleHistoryGaffSequence : IGaffSequence
		{
			private class DecisionGen : AlternateDecisionGenerator
			{
				public DecisionGen(GameCycleData cycleData) : base(new ReplayDecisionGen(cycleData.Decisions)) { }
				protected override Decision GetDecision<T>(string context, Func<T> decisionDefinition, Func<object> result) => new Decision(decisionDefinition(), result());
			}

			private IReadOnlyList<GameCycleData> cycleData;
			public int SelectedStakeCombination { get; }
			public IReadOnlyDictionary<string, object> InterGameData { get; }

			public GleDialUpResults GetGaffResults(IRunner runner, Inputs inputs)
			{
				var rngs = new List<IReadOnlyList<ulong>>();
				var gcr = default(CycleResult);

				foreach (var cycle in cycleData)
				{
					var dg = new DecisionGen(cycle);
					gcr = runner.EvaluateCycle(dg, inputs, gcr);
					rngs.Add(dg.OrderedDecisions.SelectMany(DecisionHelper.ConvertToRng).ToList());
				}

				return new GleDialUpResults(rngs);
			}

			public string Name { get; }
			public GaffType GaffType => GaffType.History;

			public GleHistoryGaffSequence(GameState gameState, IStakeCombination stakeCombination)
			{
				Name = $"GLE Game, {gameState.CycleData.Count} cycle{(gameState.CycleData.Count == 1 ? "" : "s")}, {DateTime.Now}\n{stakeCombination}";
				SelectedStakeCombination = gameState.SelectedStakeCombination;
				InterGameData = gameState.InterGameData;
				cycleData = gameState.CycleData.ToArray();
			}
		}

		private sealed class GameCycleData
		{
			public IReadOnlyList<ulong> Decisions { get; }

			public GameCycleData(IReadOnlyList<ulong> decisions)
			{
				Decisions = decisions;
			}
		}

		private sealed class GameState
		{
			/// <summary>
			/// True if the game was reset, false otherwise.
			/// </summary>
			public bool GameReset;

			/// <summary>
			/// The current active game configuration id.
			/// </summary>
			public IReadOnlyDictionary<string, string> GameConfiguration;

			/// <summary>
			/// The stake combination selected for this game.
			/// </summary>
			public int SelectedStakeCombination;

			/// <summary>
			/// The data train that stores data that is used between games.
			/// </summary>
			public IReadOnlyDictionary<string, object> InterGameData;

			/// <summary>
			/// The current set of player decisions.
			/// </summary>
			public Dictionary<string, GleUserSelection> Selections;

			/// <summary>
			/// The associated list of random numbers to be used for generating results.
			/// </summary>
			public List<GameCycleData> CycleData;

			#region Serialization

			private sealed class CustomSerializer : ICustomSerializer
			{
				public bool SupportsType(Type t) => t == typeof(GameState) || t == typeof(GameCycleData);

				public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex, object o)
				{
					switch (o)
					{
						case GameState gs:
							writer.Write(gs.GameReset);
							serializeComplex(writer, gs.GameConfiguration);
							writer.Write(gs.SelectedStakeCombination);
							serializeComplex(writer, gs.InterGameData);
							serializeComplex(writer, gs.Selections);
							serializeComplex(writer, gs.CycleData);
							break;

						case GameCycleData gsd:
							serializeComplex(writer, gsd.Decisions);
							break;

						default:
							throw new InvalidOperationException($"Unable to serialize object of type {o.GetType()}");
					}
				}

				public object Deserialize(Type t, BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
				{
					if (t == typeof(GameState))
					{
						var gs = new GameState();
						gs.GameReset = reader.ReadBoolean();
						gs.GameConfiguration = (IReadOnlyDictionary<string, string>)deserializeComplex(reader);
						gs.SelectedStakeCombination = reader.ReadInt32();
						gs.InterGameData = (IReadOnlyDictionary<string, object>)deserializeComplex(reader);
						gs.Selections = (Dictionary<string, GleUserSelection>)deserializeComplex(reader);
						gs.CycleData = (List<GameCycleData>)deserializeComplex(reader);
						return gs;
					}

					if (t == typeof(GameCycleData))
					{
						var randomNumbers = (IReadOnlyList<ulong>)deserializeComplex(reader);
						return new GameCycleData(randomNumbers);
					}

					throw new InvalidOperationException($"Unable to deserialize object of type {t}");
				}
			}

			static GameState() => NvramSerializer.RegisterCustomSerializer(new CustomSerializer());

			#endregion
		}

		private sealed class NoTriggerPrizeResultCondition : ResultCondition
		{
			private readonly Func<Credit, bool> checkPrize;

			private NoTriggerPrizeResultCondition(Func<Credit, bool> checkPrize)
			{
				this.checkPrize = checkPrize;
			}

			public static GleGaffSequence CreateSequence(string title, Func<Credit, bool> checkPrize)
			{
				var step = new GaffStep("", Array.Empty<DecisionMaker>(), new[] { new NoTriggerPrizeResultCondition(checkPrize) });
				return new GleGaffSequence(new GaffSequence(title, true, new[] { step }));
			}

			public override bool CheckCondition(CycleResult result, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow)
			{
				return result.Cycles.IsFinished && checkPrize(result.AwardedPrize.ToMidasCredit());
			}

			public override IResult ToString(string format) => null;
		}
	}
}