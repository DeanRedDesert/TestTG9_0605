using System;
using System.Linq;
using UnityEngine;

namespace Midas.Presentation.General
{
	[RequireComponent(typeof(BoxCollider2D))]
	public sealed class TouchRegion : MonoBehaviour
	{
		private BoxCollider2D boxCollider2D;

		[SerializeField]
		private Camera[] touchRegionCameras;

		[SerializeField]
		private bool invertX;

		[SerializeField]
		private bool invertY;

		private BoxCollider2D BoxCollider2D => boxCollider2D == null ? boxCollider2D = GetComponent<BoxCollider2D>() : boxCollider2D;

		public event Action OnTouchDown;
		public event Action OnTouchUp;
		public event Action<Vector3> OnTouch;

		public Vector2 ValueToSliderPos(Vector2 value)
		{
			value = new Vector2(invertX ? 0.5f - value.x : value.x - 0.5f, invertY ? 0.5f - value.y : value.y - 0.5f);

			var bc = BoxCollider2D;
			return bc.size * value + bc.offset;
		}

		public Vector2 ValueToSliderPosHorizontal(float value)
		{
			return ValueToSliderPos(new Vector2(value, 0.5f));
		}

		public Vector2 ValueToSliderPosVertical(float value)
		{
			return ValueToSliderPos(new Vector2(0.5f, value));
		}

		private void OnMouseDown()
		{
			OnTouchDown?.Invoke();
		}

		private void OnMouseUp()
		{
			OnTouchUp?.Invoke();
		}

		private void OnMouseDrag()
		{
			if (OnTouch == null)
				return;

			var mousePos = Input.mousePosition;
			var cam = touchRegionCameras.FirstOrDefault(c => c.gameObject.activeInHierarchy && c.isActiveAndEnabled);
			if (!cam)
				Camera.allCameras.FirstOrDefault(c => c.targetDisplay == mousePos.z);

			if (cam)
			{
				var bc = BoxCollider2D;
				var curTouch = transform.InverseTransformPoint(cam.ScreenToWorldPoint(mousePos));
				var colliderPos = curTouch - (Vector3)bc.offset + bc.bounds.extents;
				var s = bc.size;

				var xPos = Mathf.Clamp01(colliderPos.x / s.x);
				if (invertX)
					xPos = 1 - xPos;

				var yPos = Mathf.Clamp01(colliderPos.y / s.y);
				if (invertY)
					yPos = 1 - yPos;

				OnTouch?.Invoke(new Vector3(xPos, yPos, 0));
			}
		}
	}
}