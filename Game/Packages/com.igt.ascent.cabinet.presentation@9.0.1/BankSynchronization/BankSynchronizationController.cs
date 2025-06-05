//-----------------------------------------------------------------------
// <copyright file = "BankSynchronizationController.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using CabinetServices;
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas;
    using GameEvents;
    using GameEvents.GameMessages;
    using PeripheralLights;
    using PeripheralLights.Devices;

    /// <summary>
    /// Controls the synchronized bank attracts.
    /// </summary>
    public static class BankSynchronizationController
    {
        /// <summary>
        /// Data structure containing a message type and a container of callbacks.
        /// </summary>
        private class GameEventMessages
        {
            /// <summary>
            /// Construct an instance of the <see cref="GameEventMessages"/> container.
            /// </summary>
            /// <param name="messageType">The Type of the message.</param>
            public GameEventMessages(Type messageType)
            {
                MessageType = messageType;
                MessageCallbacks = new Dictionary<GameEventCallback, GameEventKeyMatch>();
            }

            /// <summary>
            /// The Type for the Game Message.
            /// </summary>
            public Type MessageType { get; }

            /// <summary>
            /// Gets the message callbacks.
            /// </summary>
            /// <remarks>
            /// Key is the <see cref="GameEventCallback"/> function (delegate to be called when the message is received).
            /// Value is a <see cref="GameEventKeyMatch"/> function defining the sender key requirements.
            /// </remarks>
            public Dictionary<GameEventCallback, GameEventKeyMatch> MessageCallbacks { get; }
        }

        #region Public Properties

        /// <summary>
        /// Gets the flag indicating if <see cref="AsyncConnect"/> is complete.
        /// </summary>
        public static bool AsyncConnectComplete { get; private set; }

        /// <summary>
        /// Gets the flag indicating if <see cref="PostConnect"/> is complete.
        /// </summary>
        public static bool PostConnectComplete { get; private set; }

        #endregion

        #region Fields

        /// <summary>
        /// The amount of time in milliseconds that the target time can be off from the
        /// current time and still play.
        /// </summary>
        public const long DefaultTimeTolerance = 30;

        private static IBankSynchronization synchronizationInterface;
        private static string gameMountPoint;
        private static Playlists playlists;
        private static readonly Dictionary<StreamingLightHardware, UsbStreamingLight> LightDevices =
            new Dictionary<StreamingLightHardware, UsbStreamingLight>();
        private static NextEntryInfo? nextEntry;
        private static long timeTolerance;
        private static readonly Dictionary<string, GameEventMessages> KnownMessages = new Dictionary<string, GameEventMessages>();

        private static IGameMessageHeaderSerializer gameMessageHeaderSerializer;
        private static GameMessageSerializer serializer;
        private const int MaxGameMessageSizeInBytes = 2048;
        private static bool gameEventRegistrationRequested;
        private static int headerVersion;

        #endregion

        /// <summary>
        /// Construct the static instance.
        /// </summary>
        static BankSynchronizationController()
        {
            // Set the default algorithm.
            SynchronizationAlgorithm = new RollingSynchronizationAlgorithm();
            BankPosition = 1;
            TotalMachinesInBank = 1;
            TimeTolerance = DefaultTimeTolerance;

            foreach(var type in
                Assembly.GetAssembly(typeof(IGameMessage))
                    .GetTypes()
                    .Where(
                        myType =>
                            myType.IsClass && !myType.IsAbstract &&
                            myType.GetInterfaces().Contains(typeof(IGameMessage))))
            {
                KnownMessages.Add(type.ToString(), new GameEventMessages(type));
            }

            // Set the header version to compress game messages.
            SetHeaderVersion(2);
        }

        #region Bank Synchronization Properties

        /// <summary>
        /// Gets if bank synchronization is enabled or not.
        /// </summary>
        public static bool IsBankSynchronizationEnabled
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current bank synchronization precision level.
        /// </summary>
        public static TimeFramePrecisionLevel PrecisionLevel
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current playlist being used.
        /// </summary>
        public static Playlist CurrentPlaylist
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the algorithm to use when doing bank synchronization.
        /// </summary>
        public static ISynchronizationAlgorithm SynchronizationAlgorithm
        {
            get;
            set;
        }

        /// <summary>
        /// Gets if the synchronized bank attracts can possibly start.
        /// (Irregardless of if the feature is enabled or not.)
        /// </summary>
        public static bool CanStart =>
            playlists != null &&
            SynchronizationAlgorithm != null &&
            synchronizationInterface != null;

        /// <summary>
        /// Gets if a playlist file has been loaded or not.
        /// </summary>
        public static bool IsPlaylistFileLoaded => playlists != null;

        /// <summary>
        /// Gets the position of the EGM within the bank. This value is 1-based.
        /// </summary>
        public static uint BankPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the total number of machines in the bank.
        /// </summary>
        public static uint TotalMachinesInBank
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the time tolerance required before starting a playlist entry.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the value is less than the default tolerance.
        /// </exception>
        public static long TimeTolerance
        {
            get => timeTolerance;
            set
            {
                if(value < DefaultTimeTolerance)
                {
                    throw new ArgumentException(
                        $"The time tolerance cannot be less than the default tolerance. ({DefaultTimeTolerance})");
                }
                timeTolerance = value;
            }
        }

        #endregion

        #region Game Events Properties

        /// <summary>
        /// Gets if the game events system can be used, regardless of the feature being enabled.
        /// </summary>
        public static bool GameEventsReady => synchronizationInterface != null;

        /// <summary>
        /// Gets the current game's theme ID.
        /// </summary>
        public static string ThemeId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets if the game events are currently registered.
        /// </summary>
        public static bool GameEventsRegistered => synchronizationInterface?.GameEventsEnabled == true;

        /// <summary>
        /// Delegate defining a function that will be called upon receiving a game message.
        /// </summary>
        /// <param name="message">The received game message.</param>
        public delegate void GameEventCallback(IGameMessage message);

        /// <summary>
        /// Delegate to determine if a received game message was sent from an acceptable sender.
        /// </summary>
        /// <param name="senderKey">The message sender.</param>
        /// <returns>A value indicating if the message is from a valid sender.</returns>
        /// <example>
        /// bool OnlyBlueGames(string senderKey)
        /// {
        ///     return senderKey.Contains("Blue");
        /// }
        /// 
        /// bool AnyTheme(string senderKey)
        /// {
        ///     return true;
        /// }
        /// 
        /// bool SameTheme(string senderKey)
        /// {
        ///     return string.Equals(senderKey, BankSynchronizationController.ThemeId);
        /// }
        /// </example>
        public delegate bool GameEventKeyMatch(string senderKey);

        /// <summary>
        /// Event for a game message being received but failing to raise a callback.
        /// </summary>
        public static event EventHandler<MessageReceiveFailureEventArgs> GameMessageReceiveFailure;

        #endregion

        #region Bank Synchronization Methods
        /// <summary>
        /// Advance to the next attract sequence.
        /// </summary>
        /// <returns>
        /// Returns the current attract status that indicates the type of attract the game should do.
        /// The amount of time until the next attract starts is also returned.
        /// </returns>
        public static AttractStatus AdvanceAttract()
        {
            var status = new AttractStatus
            {
                AttractMode = AttractMode.Standalone
            };

            var currentTime = UpdateSynchronizationStatus();
            if(CanStart && IsBankSynchronizationEnabled && CurrentPlaylist != null)
            {
                var currentIndex = SynchronizationAlgorithm.GetPlaylistEntryIndex(CurrentPlaylist, currentTime,
                    BankPosition, TotalMachinesInBank);
                var currentEntry = SynchronizationAlgorithm.GetStartTimeForCurrentPlaylistEntry(CurrentPlaylist,
                    currentIndex, currentTime, BankPosition, TotalMachinesInBank);

                nextEntry = SynchronizationAlgorithm.GetStartTimeForNextPlaylistEntry(CurrentPlaylist,
                            currentIndex, currentTime, BankPosition, TotalMachinesInBank);

                // The entry to play assuming we are within tolerance either slightly early or slightly late.
                NextEntryInfo? playEntry = null;

                if(IsTimeWithinTolerance(currentTime, currentEntry.StartTime))
                {
                    // Within tolerance but slightly late, so play the current entry.
                    playEntry = currentEntry;
                }
                else if(IsTimeWithinTolerance(currentTime, nextEntry.Value.StartTime))
                {
                    // Within tolerance but slightly early, so play the next entry.
                    playEntry = nextEntry;
                }

                // If we are in tolerance, play the current or next entry.
                // If we're out of tolerance play standalone and wait for the next entry.
                if(playEntry.HasValue)
                {
                    // Play the attract entry in sync mode
                    var playlistEntry = CurrentPlaylist.Items[playEntry.Value.NextEntryIndex];
                    Play(playlistEntry);
                    if(playlistEntry is SynchronizedAttractEntry)
                    {
                        status.AttractMode = PrecisionLevel == TimeFramePrecisionLevel.High ?
                            AttractMode.HighPrecisionSynchronized : AttractMode.LowPrecisionSynchronized;
                    }
                    else
                    {
                        // For now assume that it if isn't a synchronized entry it must be a standalone
                        // attract entry/command.
                        status.AttractMode = AttractMode.Standalone;
                    }
                    status.EntryIndexPlaying = playEntry.Value.NextEntryIndex;

                    // Now wait for this sequence to play.
                    // This time offset is required so that the current time isn't returned
                    // again if there is only one entry in the list. This only applies
                    // if the current time matches the start time exactly.
                    var timeOffset = currentTime == playEntry.Value.StartTime ? 1 : 0;
                    nextEntry = SynchronizationAlgorithm.GetStartTimeForNextPlaylistEntry(CurrentPlaylist,
                            playEntry.Value.NextEntryIndex, currentTime + timeOffset, BankPosition, TotalMachinesInBank);
                    status.WaitTime = nextEntry.Value.StartTime - currentTime;
                }
                else
                {
                    if(currentEntry.StartTime < currentTime)
                    {
                        // This index has been missed so wait for the next one.
                        nextEntry = SynchronizationAlgorithm.GetStartTimeForNextPlaylistEntry(CurrentPlaylist,
                            currentEntry.NextEntryIndex, currentTime, BankPosition, TotalMachinesInBank);
                    }

                    // Wait for the time to be right and run the sync attract.
                    status.WaitTime = nextEntry.Value.StartTime - currentTime;
                    // Play the standalone attract.
                }
            }

            return status;
        }

        /// <summary>
        /// Do required clean up after the attracts have stopped.
        /// </summary>
        public static void AttractsHaveStopped()
        {
            BankAttractCleanup();
        }

        /// <summary>
        /// Loads a file containing playlists.
        /// </summary>
        /// <param name="filename">The file to load.</param>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the game mount point has not been set yet.
        /// </exception>
        public static void LoadPlaylists(string filename)
        {
            if(gameMountPoint == null)
            {
                throw new InvalidOperationException("The playlist cannot be loaded until the game mount point has been set.");
            }

            playlists = Playlists.Load(gameMountPoint, filename);
        }

        /// <summary>
        /// Initializes on a separate thread to speed up performance.
        /// </summary>
        /// <remarks>
        /// This method follows the pattern defined by <see cref="IAsyncConnect.AsyncConnect"/>
        /// </remarks>
        /// <param name="cabinet">The cabinet library instance.</param>
        /// <param name="gameMountPoint">The game's mount point.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="cabinet"/> or <paramref name="gameMountPoint"/> is null.
        /// </exception>
        // ReSharper disable once ParameterHidesMember
        public static void AsyncConnect(ICabinetLib cabinet, string gameMountPoint)
        {
            if(cabinet == null)
            {
                throw new ArgumentNullException(nameof(cabinet));
            }

            synchronizationInterface = cabinet.GetInterface<IBankSynchronization>();
            BankSynchronizationController.gameMountPoint = gameMountPoint ?? throw new ArgumentNullException(nameof(gameMountPoint));

            if(gameEventRegistrationRequested)
            {
                RegisterForGameEvents(ThemeId);
            }

            AsyncConnectComplete = true;
        }

        /// <summary>
        /// Initializes on the main thread.
        /// This will be called after <see cref="AsyncConnect"/>.
        /// </summary>
        /// <remarks>
        /// This method follows the pattern defined by <see cref="IAsyncConnect.PostConnect"/>
        /// </remarks>
        /// <exception cref="AsyncConnectException">Thrown if <see cref="PostConnect"/> failed.</exception>
        public static void PostConnect()
        {
            if(!AsyncConnectComplete)
            {
                throw new AsyncConnectException("Post Connect cannot be called before Async Connect completes.");
            }
            PostConnectComplete = true;
        }

        /// <summary>
        /// Releases the references to the cabinet library.
        /// </summary>
        public static void ReleaseCabinetLibrary()
        {
            if(synchronizationInterface != null)
            {
                synchronizationInterface.CleanUpGameEvents();
                synchronizationInterface.GameMessageReceivedEvent -= OnGameMessageReceivedEvent;
                synchronizationInterface = null;
            }
            IsBankSynchronizationEnabled = false;
            LightDevices.Clear();
            gameMountPoint = null;
            BankAttractCleanup();
        }

        /// <summary>
        /// Get the synchronization status' current time.
        /// </summary>
        /// <returns>
        /// The current time in milliseconds. Will return -1 if the synchronizationInterface is null, or if the bank 
        /// synchronization feature is disabled.
        /// </returns>
        public static long GetCurrentTime()
        {
            if(synchronizationInterface != null)
            {
                var status = synchronizationInterface.GetSynchronizationStatus();
                if(status.BankSynchronizationEnabled)
                {
                    return status.CurrentTime;
                }
            }

            return -1;
        }

        /// <summary>
        /// Unloads any currently loaded playlist.
        /// </summary>
        public static void UnloadPlaylist()
        {
            playlists = null;
            BankAttractCleanup();
        }

        /// <summary>
        /// Plays the light sequences from a playlist entry.
        /// </summary>
        /// <param name="entry">The entry to play.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="entry"/> is null.
        /// </exception>
        internal static void Play(PlaylistEntry entry)
        {
            switch(entry)
            {
                case null:
                    throw new ArgumentNullException(nameof(entry));
                case SynchronizedAttractEntry synchronizedEntry:
                {
                    foreach(var file in synchronizedEntry.File)
                    {
                        if(!LightDevices.TryGetValue(file.LightDevice, out var device))
                        {
                            device = CabinetServiceLocator.Instance
                                                          .GetService<IPeripheralLightService>()
                                                          ?.GetPeripheralLight(file.LightDevice);

                            if(device != null)
                            {
                                LightDevices.Add(file.LightDevice, device);
                            }
                        }

                        device?.PlayLightSequence(0, file.Value);
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Provide a mechanism to update the status properties such as bank position or total positions in the bank,
        /// and allow it to update this information without requiring the game to have a playlist since
        /// a playlist is not always needed for BankSynchronization.
        /// </summary>
        /// <param name="bypassPlaylist">The game's theme id.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="bypassPlaylist"/> is null.</exception>
        /// <returns>The current time in milliseconds.</returns>
        public static long UpdateSynchronizationStatus(bool bypassPlaylist = false)
        {
            if(!CanStart && !bypassPlaylist)
            {
                BankAttractCleanup();
                return 0;
            }

            var info = synchronizationInterface.GetSynchronizationStatus();
            IsBankSynchronizationEnabled = info.BankSynchronizationEnabled;

            if(PrecisionLevel != info.PrecisionLevel || !IsBankSynchronizationEnabled)
            {
                // Do the clean up now because the playlists will be changing.
                BankAttractCleanup();
            }

            PrecisionLevel = info.PrecisionLevel;
            BankPosition = info.BankPosition;
            TotalMachinesInBank = info.TotalMachinesInBank;

            if(PrecisionLevel != TimeFramePrecisionLevel.None && !bypassPlaylist)
            {
                CurrentPlaylist = playlists.Playlist.FirstOrDefault(playlist =>
                    playlist.Precision == ConvertPrecisionEnum(PrecisionLevel));
            }

            return info.CurrentTime;
        }

        /// <summary>
        /// Determines if the current time and the target time are within
        /// the allow tolerance of each other.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        /// <param name="targetTime">The target time.</param>
        /// <returns>True if they are within tolerance.</returns>
        private static bool IsTimeWithinTolerance(long currentTime, long targetTime)
        {
            return Math.Abs(targetTime - currentTime) < TimeTolerance;
        }

        /// <summary>
        /// Converts the CSI time frame precision enum to the enum in the playlist schema.
        /// </summary>
        /// <param name="precision">The value to convert.</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="precision"/> cannot be converted.
        /// </exception>
        private static TimePrecision ConvertPrecisionEnum(TimeFramePrecisionLevel precision)
        {
            switch(precision)
            {
                case TimeFramePrecisionLevel.High:
                    return TimePrecision.High;
                case TimeFramePrecisionLevel.Low:
                    return TimePrecision.Low;
                default:
                    throw new ArgumentException("Unsupported value: " + precision);
            }
        }

        /// <summary>
        /// Do any required clean up after a bank attract.
        /// </summary>
        private static void BankAttractCleanup()
        {
            nextEntry = null;
            CurrentPlaylist = null;
        }

        #endregion

        #region Game Events Methods

        /// <summary>
        /// Registers the game with the game events system.
        /// </summary>
        /// <param name="themeId">The game's theme id.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="themeId"/> is null.</exception>
        /// <returns>Returns if the registration was successful or not.</returns>
        public static bool RegisterForGameEvents(string themeId)
        {
            if(themeId == null)
            {
                throw new ArgumentNullException(nameof(themeId));
            }

            if(!GameEventsReady)
            {
                return false;
            }

            var info = synchronizationInterface.GetSynchronizationStatus();
            BankPosition = info.BankPosition;
            TotalMachinesInBank = info.TotalMachinesInBank;

            if(!synchronizationInterface.GameEventsEnabled)
            {
                ThemeId = themeId;

                if(synchronizationInterface.RegisterForGameEvents())
                {
                    synchronizationInterface.GameMessageReceivedEvent += OnGameMessageReceivedEvent;
                    // Make sure the bank parameters are updated after a theme id has been registered
                    // Bypass playlist the first time registering for game events to ensure parameters such as bank
                    // position and total positions in bank are updated even if after a power hit.
                    UpdateSynchronizationStatus(true);
                }
            }

            gameEventRegistrationRequested = true;
            return synchronizationInterface.GameEventsEnabled;
        }

        /// <summary>
        /// Unregisters the game from the game events system.
        /// </summary>
        public static void UnregisterForGameEvents()
        {
            gameEventRegistrationRequested = false;
            if(synchronizationInterface != null)
            {
                synchronizationInterface.UnregisterForGameEvents();
                synchronizationInterface.GameMessageReceivedEvent -= OnGameMessageReceivedEvent;
            }
        }

        /// <summary>
        /// Register a callback event for a game message type.
        /// </summary>
        /// <param name="messageType">The type of message the callback is for.</param>
        /// <param name="callback">The delegate to callback when a message is received.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> or <paramref name="messageType"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="messageType"/> doesn't implement IGameMessage. </exception>
        public static void RegisterGameEventCallback(Type messageType, GameEventCallback callback)
        {
            RegisterGameEventCallback(messageType, callback, KeyMatch.SameKey);
        }


        /// <summary>
        /// Register a callback event for a game message type.
        /// </summary>
        /// <param name="messageType">The type of message the callback is for.</param>
        /// <param name="callback">The delegate to callback when a message is received.</param>
        /// <param name="keyMatch">The delegate to determine if the message is from an allowed sender.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> or <paramref name="messageType"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="messageType"/> doesn't implement IGameMessage or if the given callback/type pair is already registered. </exception>
        public static void RegisterGameEventCallback(Type messageType, GameEventCallback callback,
            GameEventKeyMatch keyMatch)
        {
            if(messageType == null)
            {
                throw new ArgumentNullException(nameof(messageType));
            }
            if(callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            if(!messageType.GetInterfaces().Contains(typeof(IGameMessage)))
            {
                throw new ArgumentException("messageType does not implement IGameMessage", nameof(messageType));
            }

            var typeString = messageType.ToString();

            // If the game is registering for a custom (unknown) message type, instantiate it in the KnownMessages list.
            if(!KnownMessages.ContainsKey(typeString))
            {
                KnownMessages.Add(typeString, new GameEventMessages(messageType));
            }

            // Cannot register the same callback twice for a single message type.
            if(KnownMessages[typeString].MessageCallbacks.ContainsKey(callback))
            {
                throw new ArgumentException("This callback is already registered.", nameof(callback));
            }

            KnownMessages[typeString].MessageCallbacks.Add(callback, keyMatch);
        }

        /// <summary>
        /// Unregister a game event callback for a game message type.
        /// </summary>
        /// <param name="messageType">The type of message the callback is for.</param>
        /// <param name="callback">The delegate to unregister.</param>
        /// <exception cref="ArgumentNullException">Thrown if callback is null. </exception>
        /// <exception cref="ArgumentException">Thrown if message type is null or doesn't implement IGameMessage. </exception>
        public static void UnregisterGameEventCallback(Type messageType, GameEventCallback callback)
        {
            if(messageType?.GetInterfaces().Contains(typeof(IGameMessage)) != true)
            {
                throw new ArgumentException("messageType is null or does not implement IGameMessage", nameof(messageType));
            }

            if(callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var typeString = messageType.ToString();

            if(KnownMessages.ContainsKey(typeString))
            {
                KnownMessages[typeString].MessageCallbacks.Remove(callback);
            }
        }

        /// <summary>
        /// Sends a game message with the ThemeId as the sender key.
        /// </summary>
        /// <param name="message">The type of message being sent.</param>
        /// <exception cref="ArgumentNullException">Thrown if message is null. </exception>
        /// <exception cref="InvalidOperationException">Thrown if messagePayload exceeds the allowed size in bytes.</exception>
        public static void SendMessage(IGameMessage message)
        {
            SendMessage(message, ThemeId);
        }

        /// <summary>
        /// Send a message with a custom sender key.
        /// </summary>
        /// <param name="message">The type of message being sent.</param>
        /// <param name="senderKey">
        /// The custom sender key. Games receiving this message will see the message as sent from this key.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null. </exception>
        /// <exception cref="GameMessageSizeException">Thrown if messagePayload exceeds the allowed size in bytes.</exception>
        public static void SendMessage(IGameMessage message, string senderKey)
        {
            if(GameEventsReady && synchronizationInterface.GameEventsEnabled)
            {
                if(message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }

                byte[] serializedGameMessage;
                var uncompressedLength = 0;

                // Serialize the message with the appropriate serializer.
                switch (headerVersion)
                {
                    // Non-compressed game message.
                    case 0:
                        serializedGameMessage = serializer.SerializeGameMessage(message);
                        break;
                    // Default to compressed game message.
                    default:
                        serializedGameMessage = ((CompressedGameMessageSerializer)serializer).SerializeGameMessage(message, out uncompressedLength);
                        break;
                }

                // Create the message header.
                var header = new GameMessageHeader(senderKey,
                    serializedGameMessage,
                    message.GetType().ToString(),
                    message.MessageVersion,
                    headerVersion,
                    uncompressedLength);

                // Serialize and encode the header, check message size, and send the message.
                var serializedHeader = gameMessageHeaderSerializer.SerializeGameMessageHeader(header);
                var count = Encoding.UTF8.GetByteCount(serializedHeader);
                if(count >= MaxGameMessageSizeInBytes)
                {
                    throw new GameMessageSizeException(count, MaxGameMessageSizeInBytes);
                }
                synchronizationInterface.SendGameEvent(serializedHeader);
            }
        }

        /// <summary>
        /// Event raised when a game message is received. Calls all registered game events for the message if any exist,
        /// otherwise raises a failure message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="gameMessageReceivedEventArgs">The game message event args.</param>
        private static void OnGameMessageReceivedEvent(object sender, GameMessageReceivedEventArgs gameMessageReceivedEventArgs)
        {
            var message = ValidateMessageHeader(gameMessageReceivedEventArgs.GameMessageHeader, out var validationData);
            if(validationData.FailureReason == FailureReason.None)
            {
                foreach(var callbackPair in KnownMessages[validationData.ReceivedHeader.MessageType].MessageCallbacks)
                {
                    if(callbackPair.Value(validationData.ReceivedHeader.SenderThemeId))
                    {
                        callbackPair.Key(message);
                    }
                }
            }
            else
            {
                GameMessageReceiveFailure?.Invoke(null, new MessageReceiveFailureEventArgs(validationData, gameMessageReceivedEventArgs.GameMessageHeader));
            }
        }

        /// <summary>
        /// Validate received game message header data by deserializing, checking for registered listeners, comparing
        /// theme ID requirements, checking message type, deserializing message, and checking message version.
        /// </summary>
        /// <param name="messagePayload">The message data received from the foundation.</param>
        /// <param name="validationData">Exception information from a failed header.</param>
        /// <returns>The final game message, or null if validation failed.</returns>
        private static IGameMessage ValidateMessageHeader(string messagePayload, out GameMessageValidationData validationData)
        {
            var messageHeaderSerializer = new CompactGameMessageHeaderSerializer();
            var header = messageHeaderSerializer.DeserializeGameMessageHeader(messagePayload);
            if(header == null)
            {
                validationData = new GameMessageValidationData(FailureReason.ErrorDeserializingHeader, null);
                return null;
            }

            var messageTypeData = KnownMessages.ContainsKey(header.MessageType) ?
                KnownMessages[header.MessageType] : null;

            if(messageTypeData == null)
            {
                validationData = new GameMessageValidationData(FailureReason.NotAKnownType, header);
                return null;
            }

            if(!messageTypeData.MessageCallbacks.Any())
            {
                validationData = new GameMessageValidationData(FailureReason.NoRegisteredListeners, header);
                return null;
            }

            if(!messageTypeData.MessageCallbacks.Any(callbackPair => callbackPair.Value(header.SenderThemeId)))
            {
                validationData = new GameMessageValidationData(FailureReason.SenderKeyMismatch, header);
                return null;
            }

            IGameMessage localMessage;

            // Deserialize the message with the appropriate serializer.
            switch (header.HeaderVersion)
            {
                case 0:
                    var messageSerializer = new GameMessageSerializer();
                    localMessage = messageSerializer.DeserializeGameMessage(header.GameMessage, messageTypeData.MessageType);
                    break;
                case 1:
                case 2:
                    var compressedSerializer = new CompressedGameMessageSerializer();
                    localMessage = compressedSerializer.DeserializeGameMessage(header.GameMessage, messageTypeData.MessageType, 
                        header.UncompressedMessageLength);
                    break;
                default:
                    // This is a newer message that cannot be handled.
                    localMessage = null;
                    break;
            }

            if(localMessage == null)
            {
                validationData = new GameMessageValidationData(FailureReason.ErrorDeserializingMessage, header);
                return null;
            }

            // MessageVersion property is the version existing in this build
            // header.MessageVersion is filled out by the sending game's version
            // Newer messages should not be processed.
            if(localMessage.MessageVersion < header.MessageVersion)
            {
                validationData = new GameMessageValidationData(FailureReason.MessageVersionIncompatible, header);
                return null;
            }

            validationData = new GameMessageValidationData(FailureReason.None, header);
            return localMessage;
        }

        /// <summary>
        /// Set the serializer header version.
        /// </summary>
        /// <param name="version">The header version being set.</param>
        /// <remarks>
        /// Version 0: Non-compressed game message. 
        /// Version 1: Compressed game message.
        /// Version 2: Compact game message.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if the version is invalid. </exception>
        public static void SetHeaderVersion(int version)
        {
            switch (version)
            {
                case 0:
                    serializer = new GameMessageSerializer();
                    gameMessageHeaderSerializer = new GameMessageHeaderSerializer();
                    headerVersion = 0;
                    break;
                case 1:
                    serializer = new CompressedGameMessageSerializer();
                    gameMessageHeaderSerializer = new GameMessageHeaderSerializer();
                    headerVersion = 1;
                    break;
                case 2:
                    serializer = new CompressedGameMessageSerializer();
                    gameMessageHeaderSerializer = new CompactGameMessageHeaderSerializer();
                    headerVersion = 2;
                    break;
                default:
                    throw new ArgumentException($"Invalid header version ({version}).");
            }
        }

        #endregion
    }
}