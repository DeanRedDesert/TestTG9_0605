//-----------------------------------------------------------------------
// <copyright file = "SafeStorageMenu.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Assets.StandaloneSafeStorage.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using StandaloneSafeStorage;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// This class adds menu items for handling the standalone safe storage.
    /// </summary>
    public class SafeStorageMenu : ScriptableObject
    {
        /// <summary>
        /// Available slots as defined by the enum of <see cref="SaveSlot"/>.
        /// </summary>
        private static readonly List<SaveSlot> AvailableSlots = Enum.GetValues(typeof(SaveSlot)).Cast<SaveSlot>()
                                                                    .Where(slot  => slot != SaveSlot.Default)
                                                                    .ToList();

        private static readonly string[] DefaultFiles = {
                                                            SafeStorageSettings.CommittedSafeStoreFileName,
                                                            SafeStorageSettings.ModifiedSafeStoreFileName,
                                                            SafeStorageSettings.SimulateDbSafeStorageFileName
                                                        };

        /// <summary>
        /// List of slots that have been saved into files.
        /// </summary>
        private static List<SaveSlot> savedSlots;

        #region Clear Safe Storage

        /// <summary>
        /// Clears the standalone safe storage.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Clear %`", priority = 1)]
        public static void Clear()
        {
            foreach(var fileName in DefaultFiles)
            {
                if(File.Exists(fileName))
                {
                    File.Delete(fileName);
                    Debug.Log($"{fileName} deleted.");
                }
            }
        }

        /// <summary>
        /// Validates the clear menu option.
        /// </summary>
        /// <returns>True if the clear menu option can be enabled.</returns>
        [MenuItem("Tools/Standalone Safe Storage/Clear %`", validate = true)]
        public static bool ValidateClear()
        {
            return !Application.isPlaying;
        }

        /// <summary>
        /// Clears the standalone safe storage and resets the auto load property to Default.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Clear and Reset Auto Load Property", priority = 1)]
        public static void ClearAndReset()
        {
            Clear();
            SafeStorageSettingsLoader.SaveCurrentSaveSlot(SaveSlot.Default);
        }

        /// <summary>
        /// Validates the clear and reset menu option.
        /// </summary>
        /// <returns>True if the clear menu option can be enabled.</returns>
        [MenuItem("Tools/Standalone Safe Storage/Clear and Reset Auto Load Property", validate = true)]
        public static bool ValidateClearAndReset()
        {
            return !Application.isPlaying;
        }

        #endregion

        #region Save and Load From File

        /// <summary>
        /// Saves the safe storage to a file of the user's choice.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Save To File...", priority = 31)]
        public static void SaveToFile()
        {
            var saveFile = EditorUtility.SaveFilePanel("Save Safe Storage",
                                                       "",
                                                       "Safe Storage Copy.xml",
                                                       "xml");

            if(!string.IsNullOrEmpty(saveFile))
            {
                File.Copy(GetDefaultFileName(), saveFile, true);
                Debug.Log("Safe storage saved to " + saveFile);
            }
        }

        /// <summary>
        /// Validates the save to file menu option.
        /// </summary>
        /// <returns>True if the save to file menu option can be enabled.</returns>
        [MenuItem("Tools/Standalone Safe Storage/Save To File...", validate = true)]
        public static bool ValidateSaveToFile()
        {
            return File.Exists(GetDefaultFileName());
        }

        /// <summary>
        /// Loads the safe storage from a file picked by the user.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Load From File...", priority = 30)]
        public static void LoadFromFile()
        {
            var fileToLoad = EditorUtility.OpenFilePanel("Select a file to load.", "", "xml");
            if(!string.IsNullOrEmpty(fileToLoad))
            {
                File.Copy(fileToLoad, GetDefaultFileName(), true);
                Debug.Log("Safe storage loaded from " + fileToLoad);
            }
        }

        /// <summary>
        /// Validates the load from file menu option.
        /// </summary>
        /// <returns>True if the load from file menu option can be enabled.</returns>
        [MenuItem("Tools/Standalone Safe Storage/Load From File...", validate = true)]
        public static bool ValidateLoadFromFile()
        {
            return !Application.isPlaying;
        }

        #endregion

        #region Quick Save Slots

        /// <summary>
        /// Saves a copy of safe storage into slot 1.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Save Slot 1 %1", priority = 51)]
        public static void SaveSlot1()
        {
            SaveToSlot(SaveSlot.Slot1);
        }

        /// <summary>
        /// Saves a copy of safe storage into slot 2.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Save Slot 2 %2", priority = 52)]
        public static void SaveSlot2()
        {
            SaveToSlot(SaveSlot.Slot2);
        }

        /// <summary>
        /// Saves a copy of safe storage into slot 3.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Save Slot 3 %3", priority = 53)]
        public static void SaveSlot3()
        {
            SaveToSlot(SaveSlot.Slot3);
        }

        /// <summary>
        /// Saves a copy of safe storage into slot 4.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Save Slot 4 %4", priority = 54)]
        public static void SaveSlot4()
        {
            SaveToSlot(SaveSlot.Slot4);
        }

        /// <summary>
        /// Saves a copy of safe storage into slot 5.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Save Slot 5 %5", priority = 55)]
        public static void SaveSlot5()
        {
            SaveToSlot(SaveSlot.Slot5);
        }

        #endregion

        #region Quick Load Slots

        /// <summary>
        /// Loads safe storage from the file slot 1.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Load Slot 1 &1", priority = 71)]
        public static void LoadSlot1()
        {
            LoadFromSlot(SaveSlot.Slot1);
        }

        /// <summary>
        /// Validates the load slot 1 menu item.
        /// </summary>
        /// <returns>True if the menu item can be enabled.</returns>
        [MenuItem("Tools/Standalone Safe Storage/Load Slot 1 &1", validate = true)]
        public static bool ValidateLoadSlot1()
        {
            return ValidateLoadFromSlot(SaveSlot.Slot1);
        }

        /// <summary>
        /// Loads safe storage from the file slot 2.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Load Slot 2 &2", priority = 72)]
        public static void LoadSlot2()
        {
            LoadFromSlot(SaveSlot.Slot2);
        }

        /// <summary>
        /// Validates the load slot 2 menu item.
        /// </summary>
        /// <returns>True if the menu item can be enabled.</returns>
        [MenuItem("Tools/Standalone Safe Storage/Load Slot 2 &2", validate = true)]
        public static bool ValidateLoadSlot2()
        {
            return ValidateLoadFromSlot(SaveSlot.Slot2);
        }

        /// <summary>
        /// Loads safe storage from the file slot 3.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Load Slot 3 &3", priority = 73)]
        public static void LoadSlot3()
        {
            LoadFromSlot(SaveSlot.Slot3);
        }

        /// <summary>
        /// Validates the load slot 3 menu item.
        /// </summary>
        /// <returns>True if the menu item can be enabled.</returns>
        [MenuItem("Tools/Standalone Safe Storage/Load Slot 3 &3", validate = true)]
        public static bool ValidateLoadSlot3()
        {
            return ValidateLoadFromSlot(SaveSlot.Slot3);
        }

        /// <summary>
        /// Loads safe storage from the file slot 4.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Load Slot 4 &4", priority = 74)]
        public static void LoadSlot4()
        {
            LoadFromSlot(SaveSlot.Slot4);
        }

        /// <summary>
        /// Validates the load slot 4 menu item.
        /// </summary>
        /// <returns>True if the menu item can be enabled.</returns>
        [MenuItem("Tools/Standalone Safe Storage/Load Slot 4 &4", validate = true)]
        public static bool ValidateLoadSlot4()
        {
            return ValidateLoadFromSlot(SaveSlot.Slot4);
        }

        /// <summary>
        /// Loads safe storage from the file slot 5.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Load Slot 5 &5", priority = 75)]
        public static void LoadSlot5()
        {
            LoadFromSlot(SaveSlot.Slot5);
        }

        /// <summary>
        /// Validates the load slot 5 menu item.
        /// </summary>
        /// <returns>True if the menu item can be enabled.</returns>
        [MenuItem("Tools/Standalone Safe Storage/Load Slot 5 &5", validate = true)]
        public static bool ValidateLoadSlot5()
        {
            return ValidateLoadFromSlot(SaveSlot.Slot5);
        }

        #endregion

        #region Quick Delete Slots

        /// <summary>
        /// Delete safe storage slot 1.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Delete Slot 1", priority = 91)]
        public static void DeleteSlot1()
        {
            DeleteSlot(SaveSlot.Slot1);
        }

        /// <summary>
        /// Delete safe storage slot 2.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Delete Slot 2", priority = 92)]
        public static void DeleteSlot2()
        {
            DeleteSlot(SaveSlot.Slot2);
        }

        /// <summary>
        /// Delete safe storage slot 3.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Delete Slot 3", priority = 93)]
        public static void DeleteSlot3()
        {
            DeleteSlot(SaveSlot.Slot3);
        }

        /// <summary>
        /// Delete safe storage slot 4.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Delete Slot 4", priority = 94)]
        public static void DeleteSlot4()
        {
            DeleteSlot(SaveSlot.Slot4);
        }

        /// <summary>
        /// Delete safe storage slot 5.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Delete Slot 5", priority = 95)]
        public static void DeleteSlot5()
        {
            DeleteSlot(SaveSlot.Slot5);
        }

        /// <summary>
        /// Delete all safe storage saved slots.
        /// </summary>
        [MenuItem("Tools/Standalone Safe Storage/Delete All Slots", priority = 96)]
        public static void DeleteAllSlots()
        {
            if(savedSlots == null)
            {
                savedSlots = FindSavedSlots();
            }

            foreach(var slot in savedSlots)
            {
                File.Delete(SafeStorageSettings.GetSlotFileName(slot));
            }

            savedSlots.Clear();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the default safe storage file name based on whether the game is a non-concurrent or a concurrent game.
        /// </summary>
        /// <returns>The default safe storage file name.</returns>
        private static string GetDefaultFileName()
        {
            return SafeStorageSettings.GetSlotFileName(SaveSlot.Default, SafeStorageSettingsLoader.GetIsShellFlag());
        }

        /// <summary>
        /// Saves the current safe storage data to the specified slot.
        /// </summary>
        /// <param name="slot">The slot to save to.</param>
        private static void SaveToSlot(SaveSlot slot)
        {
            if(slot == SaveSlot.Default)
            {
                return;
            }

            if(savedSlots == null)
            {
                savedSlots = FindSavedSlots();
            }

            File.Copy(GetDefaultFileName(), SafeStorageSettings.GetSlotFileName(slot), true);
            Debug.Log($"Safe storage saved to {slot}.");

            if(!savedSlots.Contains(slot))
            {
                savedSlots.Add(slot);
            }
        }

        /// <summary>
        /// Deletes the current safe storage data from the specified slot.
        /// </summary>
        /// <param name="slot">The slot to delete.</param>
        private static void DeleteSlot(SaveSlot slot)
        {
            if(slot == SaveSlot.Default)
            {
                return;
            }

            if(savedSlots == null)
            {
                savedSlots = FindSavedSlots();
            }

            if(savedSlots.Contains(slot))
            {
                savedSlots.Remove(slot);
                File.Delete(SafeStorageSettings.GetSlotFileName(slot));
                Debug.Log($"Safe storage {slot} deleted");
            }
        }

        /// <summary>
        /// Loads the safe storage from the contents of a slot.
        /// </summary>
        /// <param name="slot">The slot to load from.</param>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if this function is called while the application is playing.
        /// </exception>
        private static void LoadFromSlot(SaveSlot slot)
        {
            if(Application.isPlaying)
            {
                throw new InvalidOperationException("Safe storage cannot be loaded while the player is running.");
            }

            if(slot == SaveSlot.Default)
            {
                return;
            }

            var loadFile = SafeStorageSettings.GetSlotFileName(slot);
            if(File.Exists(loadFile))
            {
                File.Copy(loadFile, GetDefaultFileName(), true);
                Debug.Log($"Safe storage loaded from {slot}.");
            }
            else
            {
                Debug.LogError($"Unable to load from {slot}. File {loadFile} does not exist.");
            }
        }

        /// <summary>
        /// Validates a load from slot menu item.
        /// </summary>
        /// <param name="slot">The slot index to validate.</param>
        /// <returns>True if something can be loaded from the slot.</returns>
        private static bool ValidateLoadFromSlot(SaveSlot slot)
        {
            if(savedSlots == null)
            {
                savedSlots = FindSavedSlots();
            }

            return !Application.isPlaying && savedSlots.Contains(slot);
        }

        /// <summary>
        /// Checks for slotted save files on disk.
        /// </summary>
        private static List<SaveSlot> FindSavedSlots()
        {
            return AvailableSlots.Where(slot => File.Exists(SafeStorageSettings.GetSlotFileName(slot))).ToList();
        }

        #endregion
    }
}