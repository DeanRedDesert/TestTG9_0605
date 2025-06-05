using IGT.Game.SDKAssets.SDKBuild.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;

namespace Localization.Editor
{
	[InitializeOnLoad]
	public static class LocalizationBuild
	{
		static LocalizationBuild()
		{
			Build.OnPreBuild += BuildLocalizationContent;
		}

		[MenuItem("Midas/Build/Localization Content")]
		private static void BuildLocalizationContent()
		{
			AddressableAssetSettings.BuildPlayerContent();
		}
	}
}