// -----------------------------------------------------------------------
// <copyright file = "SafeStorageSettings.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Assets.StandaloneSafeStorage
{
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// The scriptable object that saves the settings of the standalone safe storage.
    /// </summary>
    public sealed class SafeStorageSettings : ScriptableObject
    {
        #region Constants

        /// <summary>
        /// The name of the committed safe storage file.
        /// </summary>
        public const string CommittedSafeStoreFileName = "Com.safestorage";

        /// <summary>
        /// The name of the modified safe storage file.
        /// </summary>
        public const string ModifiedSafeStoreFileName = "Mod.safestorage";

        /// <summary>
        /// The name of the simulate database file, used for concurrent games.
        /// </summary>
        public const string SimulateDbSafeStorageFileName = "Simulate.db.safestorage";

        /// <summary>
        /// The format of the file names of the save slots.
        /// </summary>
        private const string SlotFileNameFormat = "SafeStoreSave{0}.safestorage";

        /// <summary>
        /// The name of the asset data.
        /// </summary>
        private const string AssetDataName = @"Configurations\StandaloneSafeStorageSettings";

        #endregion

        #region Serialized Fields

        public bool IsShell;
        public SaveSlot CurrentSaveSlot;

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the safe storage file name for the given save slot.
        /// </summary>
        /// <param name="saveSlot">The save slot.</param>
        /// <param name="isShell">Flag indicating if it is for a shell application.</param>
        /// <returns>
        /// The safe storage file name for <paramref name="saveSlot"/>.
        /// </returns>
        public static string GetSlotFileName(SaveSlot saveSlot, bool isShell = false)
        {
            return saveSlot == SaveSlot.Default
                       ? (isShell ? SimulateDbSafeStorageFileName : CommittedSafeStoreFileName)
                       : string.Format(SlotFileNameFormat, (int)saveSlot);
        }

        /// <summary>
        /// Loads the safe storage from save slot currently selected for auto-loading.
        /// </summary>
        /// <remarks>
        /// The method must be called on Unity main thread.
        /// </remarks>
        public static void LoadCurrentSaveSlot()
        {
            var dataObject = Resources.Load<SafeStorageSettings>(AssetDataName);

            if(dataObject == null || dataObject.CurrentSaveSlot == SaveSlot.Default)
            {
                return;
            }

            // Copy the save slot file to the default committed file.
            var defaultFile = GetSlotFileName(SaveSlot.Default, dataObject.IsShell);
            var loadFile = GetSlotFileName(dataObject.CurrentSaveSlot);

            if(File.Exists(loadFile))
            {
                Debug.Log($"Loading safe storage from {dataObject.CurrentSaveSlot}");
                File.Copy(loadFile, defaultFile, true);
            }
            else
            {
                Debug.LogError($"Failed to load safe storage from {dataObject.CurrentSaveSlot}. File {loadFile} does not exist.");
            }
        }

        #endregion
    }
}