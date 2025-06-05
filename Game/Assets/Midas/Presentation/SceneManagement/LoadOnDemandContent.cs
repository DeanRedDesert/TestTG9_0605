using Midas.Presentation.ExtensionMethods;
using UnityEngine;

namespace Midas.Presentation.SceneManagement
{
	public sealed class LoadOnDemandContent : MonoBehaviour
	{
		private bool isActive;
		private bool hasLoaded;

		[SerializeField]
		private GameObject[] content;

		public void SetContentActive(bool active)
		{
			if (active == isActive)
				return;

			if (hasLoaded)
			{
				foreach (var c in content)
					c.SetActive(active);
			}
			else if (active)
			{
				gameObject.InstantiateOrActivateChildContent(content);
				hasLoaded = true;
			}

			isActive = active;
		}
	}
}