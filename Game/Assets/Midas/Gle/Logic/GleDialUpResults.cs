using System.Collections.Generic;
using System.Linq;
using Logic.Core.Engine;
using Midas.Core;
using Midas.Gle.LogicToPresentation;

namespace Midas.Gle.Logic
{
	public sealed class GleDialUpResults : IDialUpResults
	{
		private readonly List<List<ulong>> gaffRngSequences;

		public GleDialUpResults(IReadOnlyList<IReadOnlyList<ulong>> gaffRngSequences) => this.gaffRngSequences = gaffRngSequences.Select(g => g.ToList()).ToList();

		public IReadOnlyList<ulong> PeekNext()
		{
			return gaffRngSequences[0];
		}

		public (IReadOnlyList<ulong> Numbers, bool Finished) GetNext()
		{
			var r = gaffRngSequences[0];
			gaffRngSequences.RemoveAt(0);
			return (r, gaffRngSequences.Count == 0);
		}
	}

	public sealed class GleDialUpData : IDialUpData
	{
		public Inputs Inputs { get; }

		public IReadOnlyDictionary<string, string> GameConfiguration { get; }

		public IReadOnlyDictionary<string, object> InterGameData { get; }

		public Dictionary<string, GleUserSelection> Selections { get; }

		public IReadOnlyList<IReadOnlyList<ulong>> CycleData { get; }

		public CycleResult PreviousResults { get; }

		public string InitialStageName { get; }

		public GleDialUpData(string initialStageName, Inputs inputs, IReadOnlyDictionary<string, string> gameConfiguration, IReadOnlyDictionary<string, object> interGameData, Dictionary<string, GleUserSelection> selections, IReadOnlyList<IReadOnlyList<ulong>> cycleData, CycleResult previousResults)
		{
			InitialStageName = initialStageName;
			Inputs = inputs;
			GameConfiguration = gameConfiguration;
			InterGameData = interGameData;
			Selections = selections;
			CycleData = cycleData;
			PreviousResults = previousResults;
		}
	}
}