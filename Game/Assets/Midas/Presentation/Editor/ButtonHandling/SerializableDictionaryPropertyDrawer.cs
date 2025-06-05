using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Editor.General;
using UnityEditor;

namespace Midas.Presentation.Editor.ButtonHandling
{
	[CustomPropertyDrawer(typeof(KeyCodeButtonFunctionDictionary), true)]
	[CustomPropertyDrawer(typeof(KeyCodeButtonNameDictionary), true)]
	public class KeyCodeButtonFunctionDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer
	{
	}
}