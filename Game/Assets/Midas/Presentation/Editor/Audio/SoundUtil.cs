using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Audio
{
	public static class SoundUtil
	{
		#region Public

		public static void PlayClip(AudioClip clip)
		{
			StopAllClips();
			var unityEditorAssembly = typeof(AudioImporter).Assembly;
			var audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
			var method = audioUtilClass.GetMethod(
				"PlayPreviewClip",
				BindingFlags.Static | BindingFlags.Public,
				null,
				new[] { typeof(AudioClip), typeof(int), typeof(bool) },
				null
			);
			method?.Invoke(
				null,
				new object[]
				{
					clip,
					0,
					false
				}
			);
		}

		public static void StopAllClips()
		{
			var unityEditorAssembly = typeof(AudioImporter).Assembly;
			var audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
			var method = audioUtilClass.GetMethod(
				"StopAllPreviewClips",
				BindingFlags.Static | BindingFlags.Public,
				null,
				new Type[] { },
				null
			);
			method?.Invoke(
				null,
				Array.Empty<object>()
			);
		}

		#endregion
	}
}