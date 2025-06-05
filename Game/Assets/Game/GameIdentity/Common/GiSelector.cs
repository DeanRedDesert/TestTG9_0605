using System;
using Midas.Core;
using Midas.Presentation.Data;
using Midas.Presentation.GameIdentity;
using UnityEngine;

namespace Game.GameIdentity.Common
{
	public sealed class GiSelector : MonoBehaviour
	{
		[Serializable]
		private class GameIdentityObjectPair
		{
			public GameIdentityType GameIdentity;
			public GameObject GameObject;
		}

		[SerializeField]
		private GameIdentityObjectPair[] gameIdentityObjects;

		private void OnEnable()
		{
			var gi = StatusDatabase.ConfigurationStatus.GameIdentity!.Value;

			foreach (var o in gameIdentityObjects)
			{
				if (o.GameIdentity != gi)
					Destroy(o.GameObject);
				else
					o.GameObject.SetActive(true);
			}

			Destroy(this);
		}
	}
}