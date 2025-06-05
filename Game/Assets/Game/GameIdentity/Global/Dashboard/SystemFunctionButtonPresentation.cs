using System;
using System.Collections.Generic;
using System.Linq;
using IGT.Game.Core.Linq;
using Midas.Presentation.ButtonHandling;
using UnityEngine;

namespace Game.GameIdentity.Global.Dashboard
{
	public sealed class SystemFunctionButtonPresentation : ButtonPresentation
	{
		private HashSet<GameObject> allGameObjectsForStates;
		private bool isSelected;

		[SerializeField]
		private GameObject[] enabledObjects;

		[SerializeField]
		private GameObject[] disabledObjects;

		[SerializeField]
		private GameObject[] selectedObjects;

		[SerializeField]
		[Tooltip("Used to indicated the status of the associated functionality.")]
		private GameObject[] activeObjects;

		private void Awake()
		{
			allGameObjectsForStates = new HashSet<GameObject>();
			allGameObjectsForStates.AddRange(enabledObjects);
			allGameObjectsForStates.AddRange(disabledObjects);
			allGameObjectsForStates.AddRange(selectedObjects);
		}

		public override bool OnSpecificButtonDataChanged(object oldSpecificData, object newSpecificData)
		{
			var newIsSelected = false;
			if (newSpecificData != null)
				newIsSelected = (bool)newSpecificData;

			if (newIsSelected != isSelected)
			{
				isSelected = newIsSelected;
				return true;
			}

			return false;
		}

		public override void RefreshVisualState(Button button, ButtonStateData buttonStateData)
		{
			isSelected = buttonStateData.SpecificData != null && (bool)buttonStateData.SpecificData;

			if (buttonStateData.ButtonState == ButtonState.DisabledHide)
			{
				gameObject.SetActive(false);
				return;
			}

			if (!gameObject.activeSelf)
				gameObject.SetActive(true);

			IReadOnlyList<GameObject> objectsToEnable;

			if (button.IsMouseDown)
				objectsToEnable = selectedObjects ?? Array.Empty<GameObject>();
			else
				objectsToEnable = buttonStateData.ButtonState switch
				{
					ButtonState.Enabled => enabledObjects ?? Array.Empty<GameObject>(),
					_ => disabledObjects ?? enabledObjects ?? Array.Empty<GameObject>()
				};

			foreach (var o in allGameObjectsForStates)
				o.SetActive(objectsToEnable.Contains(o));

			foreach (var ao in activeObjects)
				ao.SetActive(isSelected || objectsToEnable.Contains(ao));
		}
	}
}