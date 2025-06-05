using UnityEngine;

namespace Midas.Presentation.ExtensionMethods
{
	public static class TransformExtensionMethods
	{
		public static void SetPosX(this Transform transform, float x)
		{
			var c = transform.position;
			c.x = x;
			transform.position = c;
		}

		public static void SetPosY(this Transform transform, float y)
		{
			var c = transform.position;
			c.y = y;
			transform.position = c;
		}

		public static void SetPosZ(this Transform transform, float z)
		{
			var c = transform.position;
			c.z = z;
			transform.position = c;
		}

		public static void SetLocalPosX(this Transform transform, float x)
		{
			var c = transform.localPosition;
			c.x = x;
			transform.localPosition = c;
		}

		public static void SetLocalPosY(this Transform transform, float y)
		{
			var c = transform.localPosition;
			c.y = y;
			transform.localPosition = c;
		}

		public static void SetLocalPosZ(this Transform transform, float z)
		{
			var c = transform.localPosition;
			c.z = z;
			transform.localPosition = c;
		}

		/// <summary>
		/// Destroy all children of a transform.
		/// </summary>
		/// <param name="transform">The transform to clean up.</param>
		public static void DestroyAllChildren(this Transform transform)
		{
			if (Application.isPlaying)
			{
				for (var i = transform.childCount - 1; i >= 0; --i)
				{
					Object.Destroy(transform.GetChild(i).gameObject);
				}

				transform.DetachChildren();
			}
			else
			{
				for (var i = transform.childCount - 1; i >= 0; --i)
					Object.DestroyImmediate(transform.GetChild(i).gameObject);
			}
		}
	}
}