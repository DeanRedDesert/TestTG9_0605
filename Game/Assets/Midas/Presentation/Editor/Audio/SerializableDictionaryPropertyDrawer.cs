using Midas.Presentation.Audio;
using UnityEditor;

namespace Midas.Presentation.Editor.Audio
{
	[CustomPropertyDrawer(typeof(SoundDefinitionsDictionary), true)]
	public sealed class SerializableDictionaryPropertyDrawer : Midas.Presentation.Editor.General.SerializableDictionaryPropertyDrawer
	{
	}
}