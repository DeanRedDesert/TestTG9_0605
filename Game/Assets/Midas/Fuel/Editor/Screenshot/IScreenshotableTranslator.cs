using System;
using System.Collections;
using IGT.Game.Fuel.Data.TranslationTable;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace Midas.Fuel.Editor.Screenshot
{
	public sealed class ScreenshotableTranslator : IScreenshotableTranslator
	{
		private readonly Func<Locale, string> getTranslation;
		private readonly string contentType;

		public ScreenshotableTranslator(GameObject gameObject, SharedTableData table, SharedTableData.SharedTableEntry tableEntry, string contentType, Func<Locale, string> getTranslation)
		{
			GameObject = gameObject;
			GroupId = table.TableCollectionName;
			TranslationId = $"{GroupId}{tableEntry.Key}_{tableEntry.Id:x}";
			this.contentType = contentType;
			this.getTranslation = getTranslation;
		}

		public bool IgnoreScreenshot { get; set; } = false;

		public Vector3 ScreenshotCameraRotation { get; set; } = Vector3.zero;

		public float ScreenshotDistance { get; set; } = ScreenshotUtilities.DefaultCameraDistance;

		public GameObject GameObject { get; }

		public string GroupId { get; }

		public string TranslationId { get; }

		public IEnumerator GetScreenShots(Locale locale)
		{
			yield return new TranslationInformation
			{
				Translation = getTranslation(locale).ToFuelTranslation(),
				Language = locale.Identifier.Code,
//				Tuid = 0,
//				Comments = new List<string>(),
				ResourceLocation = ScreenshotUtilities.RenderGameObject(TranslationId.Replace(" ", "")),
//				ApprovalStatus = ApprovalStatus.Unapproved,
//				ApprovalOverride = false,
				GameTag = GroupId,
				GameClientId = TranslationId,
				GroupContext = TranslationId,
				GroupContextIndex = 0,
				ContentType = contentType,
//				TranslationReference = null
			};
		}
	}

	/// <summary>
	/// Interface for a class that can take screenshots for the Fuel system.
	/// </summary>
	public interface IScreenshotableTranslator
	{
		#region Screenshot Settings

		/// <summary>
		/// Flag indicating if this should not be screenshot at all.
		/// </summary>
		bool IgnoreScreenshot { get; set; }

		/// <summary>
		/// Specifies the custom rotation for the screenshot camera so that it can correctly capture the translation.
		/// </summary>
		Vector3 ScreenshotCameraRotation { get; set; }

		/// <summary>
		/// Distance to move the camera back from the object during screenshot.
		/// Adjust this if the object is not being captured correctly.
		/// </summary>
		float ScreenshotDistance { get; set; }

		string GroupId { get; }

		/// <summary>
		/// Ids available for screenshots.
		/// </summary>
		string TranslationId { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Take a screen shots of this item for the Fuel system.
		/// </summary>
		IEnumerator GetScreenShots(Locale locale);

		#endregion

		#region Legacy Code Support

		/// <summary>
		/// <see cref="GameObject"/> of item to screenshot.
		/// </summary>
		/// <remarks>
		/// This is to support legacy screenshot controller code.
		/// </remarks>
		GameObject GameObject { get; }

		#endregion
	}
}