using System.Linq;
using UnityEngine;

namespace Midas.Presentation.Info
{
	[CreateAssetMenu(menuName = "Midas/Info Page Controller Settings", order = 100)]
	public sealed class PageControllerSettings : ScriptableObject
	{
		[SerializeField]
		private string folderName;

		[SerializeField]
		private GameObject progressiveCeilingText;

		[SerializeField]
		private Sprite[] progressivePages;

		[SerializeField]
		private Vector3 progressiveCeilingTextOffset;

		public string FolderName => folderName;

		public Vector3 ProgressiveCeilingOffset => progressiveCeilingTextOffset;

		public bool IsProgressivePage(string pageName) => progressivePages != null && progressivePages.Any(pp => pp.name == pageName);

		public GameObject CreateProgressiveCeiling(Transform parent)
		{
			var pc = Instantiate(progressiveCeilingText, parent);
			pc.transform.localPosition = progressiveCeilingTextOffset;
			return pc;
		}
	}
}