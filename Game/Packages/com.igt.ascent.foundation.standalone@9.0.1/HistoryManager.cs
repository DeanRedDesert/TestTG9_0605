//-----------------------------------------------------------------------
// <copyright file = "HistoryManager.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This class manages the history records and history browsing
    /// for the Game Lib.
    /// </summary>
    internal class HistoryManager
    {
        #region Constants

        private const string HistoryStorageIdPath = "HistoryStorageIdPath";
        private const string HistoryRecordListPath = "HistoryRecordListPath";

        /// <summary>
        /// Constant which represents the number of history records to maintain.
        /// </summary>
        private const int MaxHistoryRecordCount = 10;

        #endregion

        #region Properties

        /// <summary>
        /// Current location in the history record. The foundation stores a fixed
        /// number of history records. While a game is in cycle, it has access to
        /// its history scope critical data for that game cycle.  When the game
        /// cycle ends, the data in the history scope will be copied to this location.
        /// The next game archives its history scope data to a new record location.
        /// Once the number of records has been exceeded, old ones will be removed.
        /// </summary>
        private int historyStorageId;
        private int HistoryStorageId
        {
            get => historyStorageId;
            set
            {
                if (historyStorageId != value)
                {
                    gameLib.WriteFoundationData(FoundationDataScope.Theme,
                                                GameLib.GameLibPrefix + HistoryStorageIdPath,
                                                value);
                    historyStorageId = value;
                }
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// The game lib that owns this object.
        /// </summary>
        private readonly GameLib gameLib;

        /// <summary>
        /// The index of the history record currently being displayed.
        /// </summary>
        private int displayingHistoryRecord;

        /// <summary>
        /// List of history records.  Only valid when the history mode is entered.
        /// </summary>
        private List<HistoryRecord> historyRecordList;

        #endregion

        #region Methods

        /// <summary>
        /// Constructs a history manager, getting a reference to its owner for
        /// reading and writing foundation data.
        /// </summary>
        /// <param name="owner">Reference to the game lib that owns this object.</param>
        /// <param name="isColdStart">Flag indicating if it is a code start.</param>
        public HistoryManager(GameLib owner, bool isColdStart)
        {
            gameLib = owner;

            if (!isColdStart)
            {
                historyStorageId = gameLib.ReadFoundationData<int>(FoundationDataScope.Theme, 
                                                                   GameLib.GameLibPrefix + HistoryStorageIdPath);
            }
        }

        /// <summary>
        /// Copy the history of the current game cycle from the game owned history storage
        /// to the Foundation owned history storage of latest 10 game cycles,
        /// then clear the game owned history storage to get ready for the next game cycle.
        /// </summary>
        /// <param name="paytableVariant">
        /// The paytable variant used for the game cycle to be archived.
        /// </param>
        /// <param name="gameDenomination">
        /// The denomination used for the game cycle to be archived.
        /// </param>
        /// <remarks>
        /// The History scope in CriticalData section is owned by the game to store the history of the current game cycle.
        /// The History Record:[0-9] scopes in History section are owned by the Foundation to record the last 10 game cycles.
        /// </remarks>
        public void ArchiveHistoryRecord(PaytableVariant paytableVariant, long gameDenomination)
        {
            #if !WEB_MOBILE
                // Archive the history data of the current game cycle to the foundation owned history storage.
                gameLib.DiskStoreManager.SwapScopes(DiskStoreSection.CriticalData, (int)CriticalDataScope.History,
                                                    DiskStoreSection.History, HistoryStorageId);
            #endif

            // Clear the game owned history storage.
            gameLib.ClearCriticalDataScope(CriticalDataScope.History);

            #if !WEB_MOBILE
                // Retrieve the history record list from the safe store.
                var historyList = gameLib.ReadFoundationData<List<HistoryRecord>>(FoundationDataScope.Theme,
                                                                                  GameLib.GameLibPrefix + HistoryRecordListPath) ??
                                  new List<HistoryRecord>();

                // If the list is full, remove the first one.
                if (historyList.Count >= MaxHistoryRecordCount)
                {
                    historyList.RemoveAt(0);
                }

                // Add a new history record to the list.
                historyList.Add(new HistoryRecord(paytableVariant, gameDenomination, HistoryStorageId));

                // Write the history record list back to the safe store.
                gameLib.WriteFoundationData(FoundationDataScope.Theme, GameLib.GameLibPrefix + HistoryRecordListPath, historyList);

                // Increment history record index.
                HistoryStorageId++;
                if (HistoryStorageId >= MaxHistoryRecordCount)
                {
                    HistoryStorageId = 0;
                }
            #endif
        }

        /// <summary>
        /// Enter the game history.
        /// </summary>
        /// <returns>
        /// The history record to be displayed.  If no history to display,
        /// the theme name would be empty string, and the denomination 0..
        /// </returns>
        /// <exception cref="InvalidFoundationHistoryStorageException">
        /// Thrown when there are errors in the denomination history list.
        /// </exception>
        public HistoryRecord EnterHistory()
        {
            var result = new HistoryRecord();

            // Get the history record list and verify its validity.
            historyRecordList = gameLib.ReadFoundationData<List<HistoryRecord>>(FoundationDataScope.Theme,
                                                                                GameLib.GameLibPrefix + HistoryRecordListPath);

            // The history record list is null if there is no game cycle played.
            if (historyRecordList != null)
            {
                if (historyRecordList.Count == 0 || historyRecordList.Count > MaxHistoryRecordCount)
                {
                    throw new InvalidFoundationHistoryStorageException("Invalid history record list size.");
                }

                if (historyRecordList.Any(record => record.Denomination <= 0))
                {
                    throw new InvalidFoundationHistoryStorageException("Invalid denomination in history record list.");
                }

                // Start displaying the last game cycle played.
                displayingHistoryRecord = historyRecordList.Count - 1;

                CopyHistoryRecordStorage(displayingHistoryRecord);

                result = historyRecordList[displayingHistoryRecord];
            }
            else
            {
                displayingHistoryRecord = 0;
            }

            return result;
        }

        /// <summary>
        /// Exit the game history.
        /// </summary>
        public void ExitHistory()
        {
            // Clear the game owned history storage.
            gameLib.ClearCriticalDataScope(CriticalDataScope.History);

            // Reset history display fields.
            historyRecordList = null;
            displayingHistoryRecord = 0;
        }

        /// <summary>
        /// Get the count of available history records.
        /// </summary>
        /// <returns>Count of available history records.</returns>
        public int GetHistoryRecordCount()
        {
            var historyList = gameLib.ReadFoundationData<List<HistoryRecord>>(FoundationDataScope.Theme,
                                                                              GameLib.GameLibPrefix + HistoryRecordListPath);

            var count = historyList?.Count ?? 0;

            return count;
        }

        /// <summary>
        /// Check if there are history records available after the current one.
        /// </summary>
        /// <returns>True if there are history records available after the current one.</returns>
        public bool IsNextAvailable()
        {
            return historyRecordList != null &&
                   displayingHistoryRecord < historyRecordList.Count - 1;
        }

        /// <summary>
        /// Check if there are history records available before the current one.
        /// </summary>
        /// <returns>True if there are history records available before the current one.</returns>
        public bool IsPreviousAvailable()
        {
            return historyRecordList != null &&
                   displayingHistoryRecord > 0;
        }

        /// <summary>
        /// Move forward to the next game cycle in history.
        /// </summary>
        /// <param name="isNewTheme">
        /// Return true if the new history record is of a different theme.  False otherwise.
        /// </param>
        /// <returns>
        /// The new history record to be displayed.
        /// </returns>
        public HistoryRecord NextHistoryRecord(out bool isNewTheme)
        {
            var nextRecord = new HistoryRecord();

            isNewTheme = false;

            if (IsNextAvailable())
            {
                nextRecord = UpdateHistoryDisplay(displayingHistoryRecord, ++displayingHistoryRecord,
                                                  out isNewTheme);
            }

            return nextRecord;
        }

        /// <summary>
        /// Move backward to the previous game cycle in history.
        /// </summary>
        /// <param name="isNewTheme">
        /// Return true if the new history record is of a different theme.  False otherwise.
        /// </param>
        /// <returns>
        /// The new history record to be displayed.
        /// </returns>
        public HistoryRecord PreviousHistoryRecord(out bool isNewTheme)
        {
            var nextRecord = new HistoryRecord();

            isNewTheme = false;

            if(IsPreviousAvailable())
            {
                nextRecord = UpdateHistoryDisplay(displayingHistoryRecord, --displayingHistoryRecord,
                                                  out isNewTheme);
            }

            return nextRecord;
        }

        /// <summary>
        /// Deep copy the requested history record in the foundation owned history storage
        /// to the game owned history storage.
        /// </summary>
        /// <param name="targetHistoryRecord">The history record to be deep copied.</param>
        /// <remarks>
        /// The History scope in CriticalData section is owned by the game to store the history of the current game cycle.
        /// The History Record:[0-9] scopes in History section are owned by the Foundation to record the last 10 game cycles.
        /// </remarks>
        private void CopyHistoryRecordStorage(int targetHistoryRecord)
        {
            // Deep copy the history data from the foundation owned history storage
            // to the game owned history storage.
            gameLib.DiskStoreManager.CopyScope(DiskStoreSection.History, historyRecordList[targetHistoryRecord].StorageId,
                                               DiskStoreSection.CriticalData, (int)CriticalDataScope.History);
        }

        /// <summary>
        /// Make the game owned history scope storage ready for the next game cycle to be
        /// displayed.  Check if there is any change in the theme and denomination.
        /// </summary>
        /// <param name="displayingRecord">
        /// History record currently being displayed.
        /// </param>
        /// <param name="newRecord">
        /// History record to be displayed.
        /// </param>
        /// <param name="isNewTheme">
        /// Return true if the new history record is of a different theme.  False otherwise.
        /// </param>
        /// <returns>
        /// The new history record to be displayed.
        /// </returns>
        private HistoryRecord UpdateHistoryDisplay(int displayingRecord, int newRecord,
                                                   out bool isNewTheme)
        {
            // Get the new record.
            CopyHistoryRecordStorage(newRecord);

            var newContext = historyRecordList[newRecord];
            var displayingContext = historyRecordList[displayingRecord];

            isNewTheme = false;

            // Check if the theme or the denomination is different.
            if(newContext.PaytableVariant.ThemeIdentifier != displayingContext.PaytableVariant.ThemeIdentifier)
            {
                isNewTheme = true;
            }

            return newContext;
        }

        #endregion
    }
}
