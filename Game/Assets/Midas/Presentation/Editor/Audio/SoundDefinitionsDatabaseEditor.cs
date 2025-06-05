// Copyright (c) 2022 IGT

#region Usings

using Midas.Presentation.Audio;
using UnityEditor;

#endregion

namespace Midas.Presentation.Editor.Audio
{
	[CustomEditor(typeof(SoundDefinitionsDatabase))]
	public sealed class SoundDefinitionsDatabaseEditor : UnityEditor.Editor
	{
		[MenuItem("Midas/Goto/Sound Definitions")]
		private static void FocusObject()
		{
			Selection.activeObject = SoundDefinitionsDatabase.Instance;
		}
	}
}