using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Midas.Presentation.Audio;
using Midas.Presentation.Data.PropertyReference;
using Midas.Tools.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Midas.Presentation.Editor.Find
{
	public sealed class FindInvalidReferences : FindReferencesBase
	{
		private enum ReferenceType
		{
			PropertyReference,
			SoundId
		}

		private const string Unassigned = "<Unassigned>";
		private ReferenceType currentRefType;
		private string searchFieldLabel;

		#region FindReferencesBase

		protected override string[] PopupNameList { get; } = Array.Empty<string>();

		protected override string HelpBoxMessage => "Find all Items that are invalid.";

		// ReSharper disable once ConvertToAutoPropertyWithPrivateSetter - can't add new accessor in an override
		protected override string SearchFieldLabel
		{
			get
			{
				return searchFieldLabel;
			}
		}

		#endregion

		#region Private Methods

		[MenuItem("Midas/Find/Find Invalid References")]
		public static void ShowWindow() => GetWindow<FindInvalidReferences>("Midas-Find Invalid References");

		protected override void DrawSearchField()
		{
			EditorGUILayout.HelpBox(HelpBoxMessage, MessageType.Info);
			currentRefType = (ReferenceType)EditorGUILayout.EnumPopup(currentRefType);
		}

		protected override IEnumerable<(Object Object, string Path)> Find()
		{
			var resourceFinder = new ResourceFinderWithProgress<MonoBehaviour, (Object GameObject, string Path)>();

			switch (currentRefType)
			{
				case ReferenceType.PropertyReference:
				{
					searchFieldLabel = "Property Path";
					var allProps = PropertyPathResolver.CollectProperties(typeof(object));
					return resourceFinder.Find(b => InspectPropRefs(b, allProps));
				}

				case ReferenceType.SoundId:
				{
					searchFieldLabel = "Sound Id";
					var allSounds = SoundDefinitionsDatabase.Instance.GetAllSoundNames();
					return resourceFinder.Find(b => InspectSoundIds(b, allSounds));
				}

				default:
					return Array.Empty<(Object GameObject, string Path)>();
			}
		}

		private static IEnumerable<(Object, string)> InspectPropRefs(Object obj, IReadOnlyList<(string Name, Type PropertyType)> allProps)
		{
			return obj
				.GetAllSerializedFieldsRecursive<PropertyReference>()
				.Where(r => !IsValidReference(r))
				.Select(r => (obj, r.Path == "" ? Unassigned : r.Path)).ToArray();

			bool IsValidReference(PropertyReference propRef)
			{
				if (string.IsNullOrEmpty(propRef.Path))
					return false;

				var prop = allProps.FirstOrDefault(p => p.Name == propRef.Path);

				if (prop != default)
				{
					var type = (Type)propRef.GetType().GetProperty("RequiredType", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(propRef);
					return type.IsAssignableFrom(prop.PropertyType);
				}

				return false;
			}
		}

		private static IEnumerable<(Object, string)> InspectSoundIds(Object obj, IReadOnlyList<string> allSounds)
		{
			return obj
				.GetAllSerializedFieldsRecursive<SoundId>()
				.Select(s => s.ToString())
				.Where(s => !IsValidSoundId(s))
				.Select(s => (obj, s == "" ? Unassigned : s)).ToArray();

			bool IsValidSoundId(string soundName)
			{
				return !string.IsNullOrEmpty(soundName) && allSounds.Contains(soundName);
			}
		}

		#endregion
	}
}