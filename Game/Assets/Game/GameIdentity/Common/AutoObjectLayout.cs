using UnityEngine;

namespace Game.GameIdentity.Common
{
	public sealed class AutoObjectLayout : MonoBehaviour
	{
		[SerializeField]
		private Vector2 anchorPosition;

		[SerializeField]
		private Vector2 offset;

		private void OnEnable()
		{
			Update();
		}

		private void Update()
		{
			Vector3 p = anchorPosition;
			var count = transform.childCount;

			for (var i = 0; i < count; i++)
			{
				var child = transform.GetChild(i);

				if (!child.gameObject.activeInHierarchy)
					continue;

				if (child.transform.localPosition != p)
					child.transform.localPosition = p;

				p += (Vector3)offset;
			}
		}
	}
}