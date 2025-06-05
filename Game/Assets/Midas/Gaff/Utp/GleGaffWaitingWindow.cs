using Midas.Presentation.Data;
using UnityEngine;

namespace Midas.Gaff.Utp
{
	public sealed class GleGaffWaitingWindow : MonoBehaviour
	{
		private GameObject waitingGameObject;

		public GameObject WaitingPrefab;

		private void Update()
		{
			var egs = StatusDatabase.QueryStatusBlock<GleGaffModule.UtpGaffStatus>(false);

			if (egs is { IsShowing: true })
			{
				if (waitingGameObject == null)
					waitingGameObject = Instantiate(WaitingPrefab, transform, false);

				if (egs.GaffResults != null)
					egs.IsShowing = false;
			}
			else
			{
				if (waitingGameObject != null)
				{
					Destroy(waitingGameObject);
					waitingGameObject = null;
				}
			}
		}
	}
}