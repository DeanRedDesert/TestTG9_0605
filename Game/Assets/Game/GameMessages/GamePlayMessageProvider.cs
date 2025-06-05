using System.Collections.Generic;
using Midas.Presentation.Data.PropertyReference;
using UnityEngine;

namespace Game.GameMessages
{
	public sealed class GamePlayMessageProvider : MonoBehaviour
	{
		#region Private Fields

		private GameObject lastGameObject;
		private int currentMessageIndex;

		#endregion

		#region Unity Fields

		[SerializeField]
		private List<GameObject> messages;

		[SerializeField]
		private PropertyReferenceValueType<int> currentIndex;

		#endregion

		private void Awake()
		{
			foreach (var m in messages)
				m.SetActive(false);
		}

		private void OnEnable()
		{
			if (lastGameObject != default)
				lastGameObject.SetActive(false);
			lastGameObject = null;

			if (messages.Count == 0)
				return;

			var ci = currentIndex.Value;
			if (ci >= messages.Count)
			{
				currentIndex.Value = 0;
				ci = 0;
			}

			if (ci != null)
			{
				lastGameObject = messages[ci.Value];
				lastGameObject.SetActive(true);
			}
		}

		private void OnDisable()
		{
			if (lastGameObject != default)
				lastGameObject.SetActive(false);

			currentIndex?.DeInit();
		}

#if UNITY_EDITOR
		public void ConfigureForMakeGame(string path)
		{
			currentIndex.ConfigureForMakeGame(path);
		}
#endif
	}
}