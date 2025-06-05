using System.Collections.Generic;
using System.Linq;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.Engine;
using Logic.Core.Types;
using Logic.Core.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Midas.Utility.ResultPicker
{
	/// <summary>
	/// Used to allow the selection of the symbol window for utility mode.
	/// </summary>
	public sealed class SymbolWindowPicker : MonoBehaviour
	{
		#region Private Fields

		private const float SymbolHeight = 70f;
		private const float SymbolWidth = 140f;
		private List<Decision> cachedDecisions;

		#endregion

		#region Public Fields

		[SerializeField]
		private GameObject symbolPrefab;

		#endregion

		public bool IsFinished { get; private set; }

		public IReadOnlyList<Decision> FinalDecisions { get { return cachedDecisions; } }

		#region Public Functions

		public void Initialize(IRunner runner, Inputs inputs, CycleResult previousResults, IReadOnlyList<Decision> currentDecisions, string context, string resultName)
		{
			IsFinished = false;
			var swDecisions = currentDecisions.Where(ud => ud.DecisionDefinition.Context.StartsWith($"{context}_Reel")).ToList();
			var (swr, lockedMask) = GenerateResult(runner, inputs, previousResults, currentDecisions, resultName);
			var continueButton = GetComponentsInChildren<Button>().Single(b => b.name == "ContinueButton");

			var swc = new GameObject("SymbolWindowContainer");
			var maxCol = swr.SymbolWindowStructure.Cells.Max(c => c.Column + 1);
			var maxRow = swr.SymbolWindowStructure.Cells.Max(c => c.Row + 1);
			swc.transform.SetParent(gameObject.transform);
			swc.transform.localPosition = new Vector3(-((maxCol - 1) * SymbolWidth) / 2f, (maxRow - 1) * SymbolHeight / 2f, 0);
			swc.transform.localScale = new Vector3(1, 1, 1);

			var i = 0;
			foreach (var population in swr.SymbolWindowStructure.PopulationsAsIndexes)
			{
				if (population.All(p => lockedMask[p]))
					continue;

				var symbols = new List<(TextMeshProUGUI Sym, TextMeshProUGUI Index)>();
				for (var popIndex = 0; popIndex < population.Count; popIndex++)
				{
					var reel = Instantiate(symbolPrefab, swc.transform);
					var cell = swr.SymbolWindowStructure.Cells[population[popIndex]];
					var contextName = swDecisions[i].DecisionDefinition.Context;
					reel.name = $"{contextName}:Sym{popIndex}";
					reel.transform.transform.localPosition = new Vector3(cell.Column * SymbolWidth, -cell.Row * SymbolHeight, 0);

					var sym = reel.GetComponentsInChildren<TextMeshProUGUI>().Single(b => b.name == "Sym");
					sym.text = swr.GetSymbolAt(population[popIndex]);
					sym.gameObject.name = $"Sym{popIndex}";

					var first = popIndex == 0;
					var last = popIndex == population.Count - 1;

					var symIndex = reel.GetComponentsInChildren<TextMeshProUGUI>().Single(b => b.name == "SymIndex");
					symIndex.text = swr.StripSelections[i].ToString();
					symIndex.gameObject.SetActive(first);
					symbols.Add((sym, symIndex));

					var swu = reel.GetComponentsInChildren<ButtonHold>().Single(b => b.name == "SymbolWindowUp");
					var swd = reel.GetComponentsInChildren<ButtonHold>().Single(b => b.name == "SymbolWindowDown");

					swu.gameObject.SetActive(first);
					swd.gameObject.SetActive(last);

					var index = i;
					swu.OnPress.AddListener(() => { ChangeStopIndex(runner, inputs, previousResults, true, 1, resultName, contextName, index, population, symbols); });
					swd.OnPress.AddListener(() => { ChangeStopIndex(runner, inputs, previousResults, false, 1, resultName, contextName, index, population, symbols); });
				}

				i++;
			}

			continueButton.onClick.AddListener(() =>
			{
				IsFinished = true;
				transform.SetParent(null);
				Destroy(gameObject);
			});
		}

		#endregion

		#region Private Functions

		private void ChangeStopIndex(IRunner runner, Inputs inputs, CycleResult previousResults, bool increment, ulong delta, string resultName, string contextName,
			int popIndex, IReadOnlyList<int> population, IReadOnlyList<(TextMeshProUGUI Sym, TextMeshProUGUI Index)> symbols)
		{
			var index = cachedDecisions.IndexOf(cd => cd.DecisionDefinition.Context == contextName);
			var dec = cachedDecisions[index];
			var stop = (ulong[])dec.Result;

			var newStop = new[]
			{
				dec.DecisionDefinition switch
				{
					WeightsIndexesDecision wsid => (stop[0] + (increment ? wsid.Weights.GetLength() + delta : wsid.Weights.GetLength() - delta)) % wsid.Weights.GetLength(),
					WeightedIndexesDecision wid => (stop[0] + (increment ? wid.IndexCount + delta : wid.IndexCount - delta)) % wid.IndexCount,
					IndexesDecision id => (stop[0] + (increment ? id.IndexCount + delta : id.IndexCount - delta)) % id.IndexCount,
					_ => stop[0]
				}
			};

			cachedDecisions[index] = new Decision(dec.DecisionDefinition, newStop);

			var swr = GenerateResult(runner, inputs, previousResults, cachedDecisions, resultName);
			for (var j = 0; j < population.Count; j++)
			{
				symbols[j].Sym.text = swr.SymbolWindow.GetSymbolAt(population[j]);
				symbols[j].Index.text = swr.SymbolWindow.StripSelections[popIndex].ToString();
			}
		}

		private (SymbolWindowResult SymbolWindow, ReadOnlyMask LockedMask) GenerateResult(IRunner runner, Inputs inputs, CycleResult previousResults, IReadOnlyList<Decision> decisions, string resultName)
		{
			var currentResults = UtilityEditorDisplay.GenerateResult(runner, inputs, previousResults, decisions, out var usedDecisions);
			cachedDecisions = usedDecisions.ToList();

			var value = currentResults.StageResults.FirstOrDefault(sr => sr.Name == resultName)?.Value;

			var swr = value switch
			{
				SymbolWindowResult symbolWindowResult => (symbolWindowResult, ReadOnlyMask.CreateAllFalse(symbolWindowResult.SymbolWindowStructure.Cells.Count)),
				LockedSymbolWindowResult lockedSymbolWindowResult => (lockedSymbolWindowResult.SymbolWindowResult, lockedSymbolWindowResult.LockMask),
				_ => default
			};

			return swr;
		}

		#endregion
	}
}