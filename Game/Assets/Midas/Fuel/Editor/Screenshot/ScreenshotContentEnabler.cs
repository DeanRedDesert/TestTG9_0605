using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable UnusedType.Global - These classes are instantiated via reflection.

namespace Midas.Fuel.Editor.Screenshot
{
	/// <summary>
	/// Interface for enabling/disabling content that should be show for screenshots.
	/// </summary>
	public interface IScreenshotContentEnabler
	{
		void Enable(GameObject contentObj, bool enable);
	}

	/// <summary>
	/// Class for disabling/enabling <see cref="Renderer"/> components.
	/// </summary>
	public class RendererScreenshotContentEnabler : IScreenshotContentEnabler
	{
		public virtual void Enable(GameObject contentObj, bool enable)
		{
			var renderer = contentObj.GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.enabled = enable;
			}
		}
	}

	/// <summary>
	/// Class for disabling/enabling <see cref="Image"/> components.
	/// </summary>
	public class ImageScreenshotContentEnabler : IScreenshotContentEnabler
	{
		public virtual void Enable(GameObject contentObj, bool enable)
		{
			if (!enable)
			{
				return;
			}

			var image = contentObj.GetComponent<Image>();
			if (image != null)
			{
				image.enabled = true;
			}
		}
	}

	/// <summary>
	/// Class for disabling/enabling <see cref="TMP_SubMeshUI"/> components.
	/// </summary>
	public class TextMeshProSubMeshUIEnabler : IScreenshotContentEnabler
	{
		public virtual void Enable(GameObject contentObj, bool enable)
		{
			if (!enable)
			{
				return;
			}

			var textMeshPro = contentObj.GetComponent<TMP_Text>();
			if (textMeshPro == null)
			{
				return;
			}

			// TMP_SubMesh and TMP_SubMeshUI have different inheritance path get all components of both types
			// to enable embedded images in either a TextMeshPro or TextMeshProUI component.

			// These object is created on the fly and will be set to inactive so we need to grab it even if it's inactive.
			var textMeshProSubMeshes = contentObj.GetComponentsInChildren<TMP_SubMesh>(true);
			foreach (var subMesh in textMeshProSubMeshes)
			{
				subMesh.enabled = true;
				// We have to make sure we active object that has the attached component or it
				// will still be inactive in the screen shot because this object isn't part of
				// ScreenShot Context component.
				subMesh.gameObject.SetActive(true);
			}

			var textMeshProSubMeshesUI = contentObj.GetComponentsInChildren<TMP_SubMeshUI>(true);
			foreach (var subMeshUI in textMeshProSubMeshesUI)
			{
				subMeshUI.enabled = true;
				subMeshUI.gameObject.SetActive(true);
			}
		}
	}
}