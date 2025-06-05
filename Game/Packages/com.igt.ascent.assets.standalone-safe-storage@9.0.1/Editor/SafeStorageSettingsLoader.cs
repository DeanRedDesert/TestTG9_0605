// -----------------------------------------------------------------------
// <copyright file = "SafeStorageSettingsLoader.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Assets.StandaloneSafeStorage.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// The utility class to load and save <see cref="SafeStorageSettings"/> from/to a binary asset.
    /// </summary>
    public static class SafeStorageSettingsLoader
    {
        #region Constants

        /// <summary>
        /// The path where the settings are saved as an asset.
        /// </summary>
        private const string AssetDataPath = @"Assets\Resources\Configurations\StandaloneSafeStorageSettings.asset";

        #endregion

        #region Private Fields

        /// <summary>
        /// Whether the settings has been loaded from asset before.
        /// </summary>
        private static bool isLoaded;

        /// <summary>
        /// The data object of the settings.
        /// </summary>
        private static SafeStorageSettings dataObject;

        #endregion

        #region Static Members For Editor Use Only

        /// <summary>
        /// Gets the IsShell flag in the settings.
        /// </summary>
        /// <returns>
        /// Flag indicating whether it is for a shell application.
        /// </returns>
        public static bool GetIsShellFlag()
        {
            LoadOnce();

            return dataObject != null && dataObject.IsShell;
        }

        /// <summary>
        /// Sets the IsShell flag in the settings.
        /// </summary>
        /// <param name="isShell">
        /// Flag indicating whether it is for a shell application.
        /// </param>
        public static void SetIsShellFlag(bool isShell)
        {
            if(Application.isPlaying)
                return;

            LoadOnce();

            // Only create the object if the flag is true.
            if(dataObject == null && isShell)
            {
                dataObject = CreateAsset();
            }

            if(dataObject != null &&  dataObject.IsShell != isShell)
            {
                dataObject.IsShell = isShell;
                EditorUtility.SetDirty(dataObject);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Gets the current save slot value from the saved settings.
        /// </summary>
        /// <returns>
        /// The current save slot.
        /// </returns>
        public static SaveSlot GetCurrentSaveSlot()
        {
            LoadOnce();

            return dataObject == null ? SaveSlot.Default : dataObject.CurrentSaveSlot;
        }

        /// <summary>
        /// Saves a new save slot value to the settings.
        /// </summary>
        /// <param name="saveSlot">
        /// The save slot to save.
        /// </param>
        public static void SaveCurrentSaveSlot(SaveSlot saveSlot)
        {
            if(Application.isPlaying)
                return;

            LoadOnce();

            // Only create the object if the slot is not default.
            if(dataObject == null && saveSlot != SaveSlot.Default)
            {
                dataObject = CreateAsset();
            }

            if(dataObject != null && dataObject.CurrentSaveSlot != saveSlot)
            {
                dataObject.CurrentSaveSlot = saveSlot;
                EditorUtility.SetDirty(dataObject);
                AssetDatabase.SaveAssets();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads the data object from Assets only once.
        /// </summary>
        private static void LoadOnce()
        {
            if(!isLoaded)
            {
                dataObject = AssetDatabase.LoadAssetAtPath<SafeStorageSettings>(AssetDataPath);
                isLoaded = true;
            }
        }

        /// <summary>
        /// Creates an instance of the settings and a binary asset to save it.
        /// </summary>
        /// <returns>
        /// The settings created.
        /// </returns>
        private static SafeStorageSettings CreateAsset()
        {
            var result = ScriptableObject.CreateInstance<SafeStorageSettings>();
            result.CurrentSaveSlot = SaveSlot.Default;

            CreateAssetDataFolder();

            // Create the binary asset
            AssetDatabase.CreateAsset(result, AssetDataPath);
            AssetDatabase.ImportAsset(AssetDataPath);

            return result;
        }

        /// <summary>
        /// Makes sure the folder for <see cref="AssetDataPath"/> exists,
        /// creating the folder(s) as needed.
        /// </summary>
        private static void CreateAssetDataFolder()
        {
            if(!AssetDatabase.IsValidFolder(@"Assets\Resources\Configurations"))
            {
                if(!AssetDatabase.IsValidFolder(@"Assets\Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                AssetDatabase.CreateFolder(@"Assets\Resources", "Configurations");
            }
        }

        #endregion
    }
}