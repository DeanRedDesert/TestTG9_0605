using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Midas.Gaff.ResultEditor
{
	public sealed class MultiItemsDecisionCallView : MonoBehaviour, IDecision, IEditableDecision
	{
		#region Private Fields

		private readonly IList<GameObject> buttons = new List<GameObject>();
		private bool refreshComponents;
		private TextMeshProUGUI callNumberText;
		private TextMeshProUGUI choosenTitleText;
		private TextMeshProUGUI sourceTitleText;
		private TextMeshProUGUI duplicatesAllowedText;
		private Toggle holdToggle;
		private VerticalLayoutGroup selectableItemsGroup;
		private VerticalLayoutGroup selectedItemsGroup;
		private bool usingReplacement;
		private IReadOnlyList<ulong> result;
		private ulong requestedCount;
		private ulong selectableItemsCount;
		private ulong selectableItemsFirstIndex;
		private List<(ulong Index, object Object)> selectedItems;
		private ulong firstIndex;

		#endregion

		#region Public Fields

		[SerializeField]
		private GameObject itemText;

		[SerializeField]
		private GameObject itemChosen;

		[SerializeField]
		private Button upButton;

		[SerializeField]
		private Button downButton;

		[SerializeField]
		private GameObject panel;

		#endregion

		#region Properties

		public object UIState { get; private set; }

		public string Context { get; private set; }

		public Decision Decision { get; private set; }

		public bool Hold
		{
			get => holdToggle.isOn;
			set => holdToggle.isOn = value;
		}

		#endregion

		#region Public Functions

		public void Construct(Decision decision, string title, bool isHeld, int callNumber)
		{
			Decision = decision;
			callNumberText = GetComponentsInChildren<TextMeshProUGUI>().Single(b => b.name == "ContextTitle");
			callNumberText.text = $"{callNumber}: {title}";
			choosenTitleText = GetComponentsInChildren<TextMeshProUGUI>().Single(b => b.name == "ChosenTitle");
			sourceTitleText = GetComponentsInChildren<TextMeshProUGUI>().Single(b => b.name == "SourceTitle");
			holdToggle = GetComponentsInChildren<Toggle>().Single(b => b.name == "Toggle");
			selectableItemsGroup = GetComponentsInChildren<VerticalLayoutGroup>().Single(b => b.name == "SelectableItemsPanel");
			selectedItemsGroup = GetComponentsInChildren<VerticalLayoutGroup>().Single(b => b.name == "SelectedItemsPanel");
			duplicatesAllowedText = GetComponentsInChildren<TextMeshProUGUI>().Single(b => b.name == "Duplicates");
			Context = decision.DecisionDefinition.Context;
			selectedItems = new List<(ulong Index, object Object)>();
			Hold = isHeld;
			upButton.onClick.AddListener(OnUpClick);
			downButton.onClick.AddListener(OnDownClick);
		}

		private void OnDownClick()
		{
			if (firstIndex == selectableItemsCount - 1)
				firstIndex = 0;
			else
				firstIndex++;
			Refresh();
		}

		private void OnUpClick()
		{
			if (firstIndex == 0)
				firstIndex = selectableItemsCount - 1;
			else
				firstIndex--;
			Refresh();
		}

		public void Initialize(object firstSelectableIndex)
		{
			usingReplacement = AllowsDuplicates();
			selectableItemsCount = GetSelectableCount();
			var fsi = firstSelectableIndex is ulong index ? index : 0;
			firstIndex = fsi > selectableItemsCount ? 0 : fsi;
			requestedCount = GetRequestedCount();
			duplicatesAllowedText.text = usingReplacement ? "Duplicates Allowed" : "Duplicates Prohibited";

			selectedItems.Clear();
			foreach (var t in (IReadOnlyList<ulong>)Decision.Result)
				selectedItems.Add((t, GetItem(t)));

			Refresh();

			choosenTitleText.text = $"Chosen {requestedCount.ToString()}";
			sourceTitleText.text = $"Source {selectableItemsCount.ToString()}";
		}

		#endregion

		#region IEditableDecision implementation

		public bool Changed { get; private set; }

		public void ResetChangedState() => Changed = false;

		#endregion

		#region Private Functions

		private void Refresh()
		{
			foreach (var b in buttons)
			{
				b.transform.SetParent(null);
				Destroy(b);
			}

			buttons.Clear();

			var selectableIndex = GetSelectableIndexes();
			for (var i = 0; i < selectableIndex.Count; i++)
			{
				var item = GetItem(selectableIndex[i]);
				var newButton = Instantiate(itemText, selectableItemsGroup.transform, false);

				newButton.GetComponentsInChildren<TextMeshProUGUI>().Single(s => s.gameObject.name == "Index").text = selectableIndex[i].ToString();
				newButton.GetComponentsInChildren<TextMeshProUGUI>().Single(s => s.gameObject.name == "Name").text = item + (!usingReplacement && selectedItems.Any(si => si.Index == selectableIndex[i]) ? "*" : string.Empty);

				if (i == 0)
					newButton.GetComponentInChildren<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
				buttons.Add(newButton.gameObject);
			}

			var height = Math.Max(selectedItems.Count, 10) * 22;
			selectableItemsGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(75, height);
			selectableItemsGroup.GetComponent<LayoutElement>().minHeight = height;
			selectedItemsGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(100, height);
			selectedItemsGroup.GetComponent<LayoutElement>().minHeight = height;
			panel.GetComponent<LayoutElement>().minHeight = height + 15;
			GetComponent<LayoutElement>().minHeight = transform.Cast<Transform>().Sum(child => child.GetComponent<LayoutElement>().minHeight);

			for (var i = 0; i < selectedItems.Count; i++)
			{
				var newButton = Instantiate(itemChosen, selectedItemsGroup.transform, false);
				SetChosen(newButton, i);
				var itemIndex = i;
				newButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
				{
					var newItem = (Index: firstIndex, GetItem(firstIndex));
					if (!usingReplacement)
					{
						var dupIndex = selectedItems.IndexOf(si => si.Index == newItem.Index);
						if (dupIndex != -1)
							selectedItems[dupIndex] = selectedItems[itemIndex];

						selectedItems[itemIndex] = newItem;
					}
					else
					{
						selectedItems[itemIndex] = newItem;
					}

					UIState = firstIndex;
					Hold = true;
					Changed = true;
					Decision = new Decision(Decision.DecisionDefinition, selectedItems.Select(si => si.Index).ToList());
					SetChosen(newButton, itemIndex);
				});

				buttons.Add(newButton.gameObject);
			}

			void SetChosen(GameObject button, int itemIndex)
			{
				button.GetComponentsInChildren<TextMeshProUGUI>().Single(s => s.gameObject.name == "Index").text = selectedItems[itemIndex].Index.ToString();
				button.GetComponentsInChildren<TextMeshProUGUI>().Single(s => s.gameObject.name == "Name").text = (string)selectedItems[itemIndex].Object;
			}
		}

		private string GetItem(ulong index) =>
			Decision.DecisionDefinition switch
			{
				WeightsIndexesDecision wsid => wsid.GetName(index),
				WeightedIndexesDecision wid => wid.GetName(index),
				IndexesDecision id => id.GetName(index),
				_ => string.Empty
			};

		private ulong GetSelectableCount() =>
			Decision.DecisionDefinition switch
			{
				WeightsIndexesDecision wsid => wsid.Weights.GetLength(),
				WeightedIndexesDecision wid => wid.IndexCount,
				IndexesDecision id => id.IndexCount,
				_ => 0
			};

		private bool AllowsDuplicates() =>
			Decision.DecisionDefinition switch
			{
				WeightsIndexesDecision wsid => wsid.AllowDuplicates,
				WeightedIndexesDecision wid => wid.AllowDuplicates,
				IndexesDecision id => id.AllowDuplicates,
				_ => false
			};

		private ulong GetRequestedCount() =>
			Decision.DecisionDefinition switch
			{
				WeightsIndexesDecision wsid => wsid.Count,
				WeightedIndexesDecision wid => wid.Count,
				IndexesDecision id => id.Count,
				_ => 0
			};

		private IReadOnlyList<ulong> GetSelectableIndexes()
		{
			return Enumerable.Range(0, Math.Min(10, (int)selectableItemsCount)).Select(s => (firstIndex + (ulong)s) % selectableItemsCount).ToList();
		}

		#endregion
	}
}