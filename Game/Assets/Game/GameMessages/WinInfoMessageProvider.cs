using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Data.PropertyReference;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.WinPresentation;
using UnityEngine;

namespace Game.GameMessages
{
	public sealed class WinInfoMessageProvider : MonoBehaviour
	{
		#region Private Fields

		private GameObject lastGameObject;

		#endregion

		#region Unity Fields

		[SerializeField]
		private List<PrizeSpriteMapping> mappings;

		[SerializeField]
		private List<PrizeObjectMapping> objectMappings;

		[SerializeField]
		private GameObject sprite;

		[SerializeField]
		private GameObject message;

		[SerializeField]
		private PropertyReference<IWinInfo> currentWin;

		[SerializeField]
		private PropertyReferenceValueType<VisibilityType> currentWinVisibilityType;

		#endregion

		#region Private Methods

		private void Awake()
		{
			sprite.SetActive(false);
			foreach (var om in objectMappings)
				om.PrizeObject.SetActive(false);
		}

		private void OnEnable()
		{
			if (currentWin == null)
				return;

			currentWin.ValueChanged += OnCurrentWinValueChanged;
			currentWinVisibilityType.ValueChanged += OnCurrentWinValueChanged;
			UpdateCurrentWin();
		}

		private void OnCurrentWinValueChanged(PropertyReference arg1, string arg2) => UpdateCurrentWin();

		private void UpdateCurrentWin()
		{
			if (currentWin.Value == null)
				return;

			if (lastGameObject != default)
				lastGameObject.SetActive(false);

			if (currentWinVisibilityType.Value != VisibilityType.Visible && currentWinVisibilityType.Value != VisibilityType.HiddenBecauseFlashing)
			{
				lastGameObject = null;
				message.SetActive(false);
				return;
			}

			var temp = GetPrizeObject(currentWin.Value.PrizeName);
			if (temp != null)
			{
				lastGameObject = temp;
				lastGameObject.SetActive(true);
				message.SetActive(true);
			}
		}

		private void OnDisable()
		{
			if (lastGameObject != default)
				lastGameObject.SetActive(false);
			lastGameObject = null;

			if (currentWin == null)
				return;

			currentWin.ValueChanged -= OnCurrentWinValueChanged;
			currentWin.DeInit();

			currentWinVisibilityType.ValueChanged -= OnCurrentWinValueChanged;
			currentWinVisibilityType.DeInit();
		}

		private GameObject GetPrizeObject(string prizeName)
		{
			var objectMapping = objectMappings.FirstOrDefault(m => m.PrizeName == prizeName);
			if (objectMapping != null)
				return objectMapping.PrizeObject;

			var mapping = mappings.FirstOrDefault(m => m.PrizeName == prizeName);
			if (mapping != null)
			{
				sprite.GetComponent<SpriteRenderer>().sprite = mapping.Sprite;
				return sprite;
			}

			return null;
		}

#if UNITY_EDITOR
		public void ConfigureForMakeGame(IReadOnlyList<(string, Sprite)> data)
		{
			mappings = data.Select(d => new PrizeSpriteMapping(d.Item1, d.Item2)).ToList();
		}
#endif

		#endregion

		#region Types

		[Serializable]
		public class PrizeSpriteMapping
		{
			#region Properties

			public string PrizeName => prizeName;

			public Sprite Sprite => sprite;

			#endregion

			#region Inspector Fields

			[SerializeField]
			private string prizeName;

			[SerializeField]
			private Sprite sprite;

			#endregion

			public PrizeSpriteMapping(string prizeName, Sprite sprite)
			{
				this.prizeName = prizeName;
				this.sprite = sprite;
			}
		}

		[Serializable]
		private class PrizeObjectMapping
		{
			#region Properties

			public string PrizeName => prizeName;

			public GameObject PrizeObject => prizeObject;

			#endregion

			#region Inspector Fields

			[SerializeField]
			private string prizeName;

			[SerializeField]
			private GameObject prizeObject;

			#endregion
		}

		#endregion
	}
}