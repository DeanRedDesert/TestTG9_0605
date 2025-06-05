using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Audio;
using Midas.Tools.Editor;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Midas.Presentation.Editor.Find
{
	public sealed class FindSoundId : FindReferencesBase
	{
		private const string Unassigned = "<Unassigned>";
		private readonly List<string> nameList = new List<string>();

		protected override string[] PopupNameList => nameList.ToArray();
		protected override string HelpBoxMessage => "Find all Sound Ids that are in use.";
		protected override string SearchFieldLabel => "Sound Id";

		[MenuItem("Midas/Find/Find Sound Ids")]
		public static void Init()
		{
			GetWindow<FindSoundId>("Midas-Find Sound Id");
		}

		protected override IEnumerable<(Object Object, string Path)> Find()
		{
			return new ResourceFinderWithProgress<Object, (Object GameObject, string Path)>().Find(FindOnMonoBehaviour);
		}

		private void OnEnable()
		{
			var accessor = SoundDefinitionsDatabase.Instance;
			if (accessor == null)
			{
				EditorGUI.HelpBox(position, " There was no SoundDefinitionsDatabase found!", MessageType.Error);
				return;
			}

			nameList.Add(Unassigned);
			nameList.AddRange(accessor.GetAllSoundNames() ?? throw new InvalidOperationException());
		}

		private IEnumerable<(Object GameObject, string Path)> FindOnMonoBehaviour(Object obj)
		{
			return obj
				.GetAllSerializedFieldsRecursive<SoundId>()
				.Select(GetSoundIdName)
				.Where(fullName => fullName.IndexOf(SelectionName, StringComparison.OrdinalIgnoreCase) >= 0)
				.Select(s => (obj, s));

			string GetSoundIdName(SoundId id)
			{
				var idName = id?.ToString();
				return string.IsNullOrEmpty(idName) ? Unassigned : idName;
			}
		}
	}
}