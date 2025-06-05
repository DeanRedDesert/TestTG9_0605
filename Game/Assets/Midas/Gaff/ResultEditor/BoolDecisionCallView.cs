using System.Linq;
using Logic.Core.DecisionGenerator.Decisions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Midas.Gaff.ResultEditor
{
	public sealed class BoolDecisionCallView : MonoBehaviour, IDecision, IEditableDecision
	{
		#region Private Fields

		private Toggle overrideToggle;
		private Toggle resultToggle;

		#endregion

		#region Properties

		public string Context { get; private set; }

		public bool Hold
		{
			get => overrideToggle.isOn;
			private set => overrideToggle.isOn = value;
		}

		public object UIState { get; private set; }

		public Decision Decision { get; private set; }

		public bool Changed { get; private set; }

		[SerializeField]
		private TextMeshProUGUI callNumberText;

		[SerializeField]
		private TextMeshProUGUI resultText;

		#endregion

		#region Public Functions

		public void Construct(Decision decision, string title, bool isHeld, int callNumber)
		{
			UIState = null;
			var toggles = GetComponentsInChildren<Toggle>();

			Decision = decision;

			overrideToggle = toggles.Single(gc => gc.name == "Hold");
			resultToggle = toggles.Single(gc => gc.name == "Result");
			resultToggle.isOn = (bool)decision.Result;
			Hold = isHeld;

			Context = Decision.DecisionDefinition.Context;
			callNumberText.text = $"{callNumber}: {title}";
			resultText.text = $"{resultToggle.isOn}";
		}

		public void Initialize() => resultToggle.onValueChanged.AddListener(OnValueChanged);

		private void OnValueChanged(bool _)
		{
			Changed = true;
			Hold = true;
			Decision = new Decision(Decision.DecisionDefinition, resultToggle.isOn);
			resultText.text = $"{resultToggle.isOn}";
		}

		#endregion

		#region IEditableDecision implementations

		void IEditableDecision.ResetChangedState() => Changed = false;

		#endregion
	}
}