using System;
using System.Collections.Generic;
using System.Linq;
using IGT.Game.Core.Linq;
using Midas.Presentation.ButtonHandling;
using UnityEngine;

namespace Game.GameIdentity.Global.Dashboard
{
	public class GlobalDashboardButtonPres : ButtonPresentation
	{
		private HashSet<GameObject> allGameObjectsForStates;

		[SerializeField]
		private GameObject[] enabledObjects;

		[SerializeField]
		private GameObject[] disabledObjects;

		[SerializeField]
		private GameObject[] selectedObjects;

		protected virtual void Awake()
		{
			allGameObjectsForStates = new HashSet<GameObject>();
			allGameObjectsForStates.AddRange(enabledObjects);
			allGameObjectsForStates.AddRange(disabledObjects);
			allGameObjectsForStates.AddRange(selectedObjects);
		}

		public override bool OnSpecificButtonDataChanged(object oldSpecificData, object newSpecificData)
		{
			return false;
		}

		public virtual bool IsButtonSelected(Button button, ButtonStateData buttonStateData)
		{
			return button.IsMouseDown;
		}

		public override void RefreshVisualState(Button button, ButtonStateData buttonStateData)
		{
			if (buttonStateData.ButtonState == ButtonState.DisabledHide)
			{
				gameObject.SetActive(false);
				return;
			}

			if (!gameObject.activeSelf)
				gameObject.SetActive(true);

			IReadOnlyList<GameObject> objectsToEnable;

			if (IsButtonSelected(button, buttonStateData))
			{
				objectsToEnable = selectedObjects ?? Array.Empty<GameObject>();
			}
			else
			{
				objectsToEnable = buttonStateData.ButtonState switch
				{
					ButtonState.Enabled => enabledObjects ?? Array.Empty<GameObject>(),
					_ => disabledObjects ?? enabledObjects ?? Array.Empty<GameObject>()
				};
			}

			foreach (var o in allGameObjectsForStates)
				o.SetActive(objectsToEnable.Contains(o));
		}
	}
}