using System;
using System.Collections.Generic;
using System.Linq;
using Midas;
using Midas.Presentation.Data.PropertyReference;
using UnityEngine;

namespace Game
{
	public interface IAutoLayoutHandler
	{
		void OnAutoLayout(int layoutIndex);
	}

	public sealed class AutoConstructAndLayout : MonoBehaviour
	{
		private enum Orientation
		{
			Horizontal,
			Vertical
		}

		public enum BlankPosition
		{
			Beginning,
			End
		}

		[Serializable]
		public class PrefabDetails
		{
			public int MinCount;
			public int MaxCount;
			public GameObject Prefab;
			public float Spacing;
			public int BankCount;
			public float BankSpacing;
			public GameObject BlankPrefab;
			public BlankPosition BlankPosition;
		}

		private int? actualObjectCount;
		private List<GameObject> currentObjects = new List<GameObject>();

		[SerializeField]
		private PropertyReferenceValueType<int> itemCountReference;

		[SerializeField]
		private PrefabDetails[] objectPrefabs;

		[SerializeField]
		private Orientation orientation;

		private void OnEnable()
		{
			if (itemCountReference == null || objectPrefabs == null || objectPrefabs.Length == 0)
			{
				Log.Instance.Warn($"{nameof(AutoConstructAndLayout)} for game object {gameObject.name} has nothing to lay out.");
				return;
			}

			itemCountReference.ValueChanged += OnValueChanged;
			UpdateObjects();
		}

		private void OnValueChanged(PropertyReference propertyRef, string path)
		{
			UpdateObjects();
		}

		private void UpdateObjects()
		{
			if (itemCountReference.Value > 0)
				ConstructObjects(itemCountReference.Value.Value);
			else
				DestroyObjects();
		}

		private void OnDisable()
		{
			itemCountReference.ValueChanged -= OnValueChanged;
			itemCountReference.DeInit();
		}

		private void ConstructObjects(int count)
		{
			if (actualObjectCount == count)
				return;

			DestroyObjects();

			var prefabDetails = objectPrefabs.Single(o => count >= o.MinCount && count <= o.MaxCount);
			bool useBlank = prefabDetails.BlankPrefab;
			var objectCount = useBlank ? prefabDetails.MaxCount : count;
			var buttonsPerRow = objectCount / prefabDetails.BankCount;
			var currentObject = 0;
			var leadingBlankCount = useBlank && prefabDetails.BlankPosition == BlankPosition.Beginning ? objectCount - count : 0;

			var yPos = prefabDetails.BankSpacing * (prefabDetails.BankCount - 1) * 0.5f;

			for (var row = 0; row < prefabDetails.BankCount; row++)
			{
				if (row == prefabDetails.BankCount - 1)
					buttonsPerRow = objectCount - currentObject;

				var xPos = -prefabDetails.Spacing * (buttonsPerRow - 1) * 0.5f;

				for (var i = 0; i < buttonsPerRow; i++)
				{
					var o = CreatePrefab();
					o.transform.localPosition = orientation == Orientation.Horizontal ? new Vector3(xPos, yPos, 0) : new Vector3(-yPos, -xPos, 0);

					xPos += prefabDetails.Spacing;

					var objIndex = currentObject - leadingBlankCount;
					if (objIndex >= 0 && objIndex < count)
						foreach (var layoutHandler in o.GetComponentsInChildren<IAutoLayoutHandler>())
							layoutHandler.OnAutoLayout(objIndex);

					currentObjects.Add(o);
					currentObject++;
				}

				yPos -= prefabDetails.BankSpacing;
			}

			actualObjectCount = count;
			return;

			GameObject CreatePrefab()
			{
				if (useBlank)
				{
					return prefabDetails.BlankPosition == BlankPosition.Beginning
						? Instantiate(currentObject < leadingBlankCount ? prefabDetails.BlankPrefab : prefabDetails.Prefab, transform)
						: Instantiate(currentObject >= count ? prefabDetails.BlankPrefab : prefabDetails.Prefab, transform);
				}

				return Instantiate(prefabDetails.Prefab, transform);
			}
		}

		private void DestroyObjects()
		{
			foreach (var o in currentObjects)
				Destroy(o);

			currentObjects.Clear();
		}
	}
}