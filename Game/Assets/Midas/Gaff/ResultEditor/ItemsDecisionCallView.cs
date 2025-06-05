using System;
using System.Collections.Generic;
using Logic.Core.DecisionGenerator.Decisions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Midas.Gaff.ResultEditor
{
	public sealed class ItemsDecisionCallView : MonoBehaviour, IDecision, IEditableDecision
	{
		#region Private Fields

		private int selectedItemIndex;
		private IList<string> items;

		#endregion

		#region Public Fields

		[SerializeField]
		private TextMeshProUGUI callNumberText;

		[SerializeField]
		private Toggle holdToggle;

		[SerializeField]
		private Button editIndexButton;

		[SerializeField]
		private GameObject theMagicalPanelPrefab;

		#endregion

		#region Properties

		public string Context { get; private set; }

		public Decision Decision { get; private set; }

		public bool Hold
		{
			get => holdToggle.isOn;
			private set => holdToggle.isOn = value;
		}

		#endregion

		#region Public Functions

		public void Construct(Decision decision, string title, bool isHeld, int callNumber)
		{
			UIState = null;
			Decision = decision;
			Context = Decision.DecisionDefinition.Context;
			Hold = isHeld;
			callNumberText.text = $"{callNumber}: {title}";
		}

		public void Initialize()
		{
			items = new List<string>();
			switch (Decision.DecisionDefinition)
			{
				case WeightsIndexesDecision wsid:
					for (ulong i = 0; i < wsid.Weights.GetLength(); i++)
						items.Add(wsid.GetName(i));
					break;

				case WeightedIndexesDecision wid:
					for (ulong i = 0; i < wid.IndexCount; i++)
						items.Add(wid.GetName(i));
					break;

				case IndexesDecision id:
					for (ulong i = 0; i < id.IndexCount; i++)
						items.Add(id.GetName(i));
					break;
			}

			selectedItemIndex = (int)((IReadOnlyList<ulong>)Decision.Result)[0];

			editIndexButton.GetComponentInChildren<TextMeshProUGUI>().text = items[selectedItemIndex];
			editIndexButton.onClick.AddListener(() =>
			{
				var rootSearcher = transform.parent;

				while (rootSearcher != null && rootSearcher.name != "ResultEditorPrefab(Clone)")
					rootSearcher = rootSearcher.parent;

				if (rootSearcher == null)
					throw new Exception("Bad things");

				var newPanel = Instantiate(theMagicalPanelPrefab, rootSearcher, false);
				newPanel.GetComponent<MagicalPanelManager>().Initialize(this, selectedItemIndex, (IReadOnlyList<string>)items);
				newPanel.transform.SetAsLastSibling();
			});
		}

		public void SetSelectedItemIndex(int selectedItem)
		{
			selectedItemIndex = selectedItem;
			Decision = new Decision(Decision.DecisionDefinition, new[] { (ulong)selectedItem });
			editIndexButton.GetComponentInChildren<TextMeshProUGUI>().text = items[selectedItem];
			Hold = true;
			Changed = true;
		}

		#endregion

		#region IEditableDecision implementation

		public object UIState { get; private set; }

		public bool Changed { get; private set; }

		public void ResetChangedState() => Changed = false;

		#endregion
	}
}