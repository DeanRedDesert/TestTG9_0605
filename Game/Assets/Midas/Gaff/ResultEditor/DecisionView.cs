using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Logic.Core.DecisionGenerator.Decisions;
using UnityEngine;
using UnityEngine.UI;

namespace Midas.Gaff.ResultEditor
{
	public sealed class DecisionView : MonoBehaviour
	{
		#region Private Fields

		private Text labelText;
		private List<IDecision> calls;
		private PropertyInfo propInfoBool;
		private PropertyInfo propInfoSingleItem;
		private PropertyInfo propMultiItem;

		#endregion

		#region Public Fields

		public string CallerName => calls != null && calls.Any() ? calls.First().Context : "";

		public bool Hold
		{
			get => calls.All(c => c.Hold);
			set
			{
				foreach (var decisionCallViewModel in calls)
					SetHold(decisionCallViewModel, value);
			}
		}

		public bool LastDecision { get; set; }

		[SerializeField]
		public DecisionSettings decisionSettings;

		#endregion

		#region Public Functions

		public void Construct(IEnumerable<IDecision> decisions)
		{
			calls = decisions.ToList();
			propInfoBool = typeof(BoolDecisionCallView).GetProperty("Hold");
			propInfoSingleItem = typeof(ItemsDecisionCallView).GetProperty("Hold");
			propMultiItem = typeof(MultiItemsDecisionCallView).GetProperty("Hold");
		}

		public void Initialize(string title)
		{
			labelText = GetComponentInChildren<Text>();
			labelText.text = title;
			var targetPanel = gameObject.transform.Find("Calls").transform;
			var layoutElement = GetComponent<LayoutElement>();
			var minWidth = 20;
			var minHeight = 50f;

			foreach (var decisionCall in calls)
			{
				switch (decisionCall)
				{
					case BoolDecisionCallView boolDecisionView:
					{
						var decisionGameObject = boolDecisionView.gameObject;
						var callLayoutElement = decisionGameObject.GetComponent<LayoutElement>();
						decisionGameObject.transform.SetParent(targetPanel, false);

						callLayoutElement.minWidth = decisionSettings.BaseDecisionWidth;
						minWidth += decisionSettings.BaseDecisionWidth;
						minHeight = Math.Max(callLayoutElement.minHeight, minHeight);
						break;
					}
					case ItemsDecisionCallView itemsDecisionView:
					{
						var calculatedRequiredWidth = MaxNameLength(itemsDecisionView.Decision.DecisionDefinition) * decisionSettings.ItemDecisionHCharacterWidth + 100;
						var decisionGameObject = itemsDecisionView.gameObject;
						var callLayoutElement = decisionGameObject.GetComponent<LayoutElement>();

						if (calculatedRequiredWidth < decisionSettings.MinimumItemDecisionWidth)
							calculatedRequiredWidth = decisionSettings.MinimumItemDecisionWidth;

						callLayoutElement.minWidth = calculatedRequiredWidth;
						minWidth += calculatedRequiredWidth;
						minHeight = Math.Max(callLayoutElement.minHeight, minHeight);
						decisionGameObject.transform.SetParent(targetPanel, false);
						break;
					}
					case MultiItemsDecisionCallView multiItemDecisionView:
					{
						var decisionGameObject = multiItemDecisionView.gameObject;
						var callLayoutElement = decisionGameObject.GetComponent<LayoutElement>();

						decisionGameObject.transform.SetParent(targetPanel, false);
						minWidth += (int)callLayoutElement.minWidth;
						minHeight = Math.Max(callLayoutElement.minHeight, minHeight);
						break;
					}
				}
			}

			minHeight += 30;
			layoutElement.minHeight = minHeight;
			layoutElement.minWidth = minWidth;

			if (LastDecision)
				layoutElement.minHeight += decisionSettings.LastItemExtraSpace;
		}

		#endregion

		#region Private Functions

		private static int MaxNameLength(DecisionDefinition sourceData)
		{
			var maxNameLength = 0;

			switch (sourceData)
			{
				case WeightsIndexesDecision wsid:
					for (ulong i = 0; i < wsid.Weights.GetLength(); i++)
						maxNameLength = Math.Max(maxNameLength, wsid.GetName(i).Length);
					break;

				case WeightedIndexesDecision wid:
					for (ulong i = 0; i < wid.IndexCount; i++)
						maxNameLength = Math.Max(maxNameLength, wid.GetName(i).Length);
					break;

				case IndexesDecision id:
					for (ulong i = 0; i < id.IndexCount; i++)
						maxNameLength = Math.Max(maxNameLength, id.GetName(i).Length);
					break;
			}

			return maxNameLength;
		}

		private void SetHold(IDecision decision, bool value)
		{
			switch (decision)
			{
				case BoolDecisionCallView _:
					propInfoBool.GetSetMethod().Invoke(decision, new object[] { value });
					break;
				case ItemsDecisionCallView _:
					propInfoSingleItem.GetSetMethod().Invoke(decision, new object[] { value });
					break;
				case MultiItemsDecisionCallView _:
					propMultiItem.GetSetMethod().Invoke(decision, new object[] { value });
					break;
				default: throw new Exception("Failed to get hold property getter.");
			}
		}

		#endregion
	}
}