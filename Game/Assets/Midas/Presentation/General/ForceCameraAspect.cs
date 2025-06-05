using UnityEngine;
using UnityEngine.Serialization;

namespace Midas.Presentation.General
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public sealed class ForceCameraAspect : MonoBehaviour
	{
		[SerializeField]
		[FormerlySerializedAs("AspectRatio")]
		private float aspectRatio = 16f / 9f;

		// Use this for initialization
		private void Awake()
		{
			GetComponent<Camera>().aspect = aspectRatio;
		}

#if UNITY_EDITOR
		// Update is called once per frame
		private void Update()
		{
			if (!Application.isPlaying)
				GetComponent<Camera>().aspect = aspectRatio;
		}
#endif
	}
}