//-----------------------------------------------------------------------
// <copyright file = "CabinetLib.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml;
    using System.Xml.Linq;
    using Communication.Standalone;
    using CSI.Schemas;
    using Devices;
    using Monitor = CSI.Schemas.Monitor;

    /// <summary>
    /// This is the cabinet communications library for the CSI communications
    /// link. Standalone implementation.
    /// </summary>
    public class CabinetLib : ICabinetLib, ICabinetLibDemo, IDisposable
    {
        #region Private Fields

        /// <summary>
        /// Flag which indicates if this object has been disposed or not.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Queue of incoming events that need to be processed in the presentation thread.
        /// </summary>
        private Queue<EventArgs> cabinetEventQueue = new Queue<EventArgs>();

        /// <summary>
        /// Table of event handlers to their associated event arguments.
        /// </summary>
        private readonly Dictionary<Type, Action<EventArgs>> eventTable = new Dictionary<Type, Action<EventArgs>>();

        /// <summary>
        /// List of interfaces that need to have their event queues dequeued.
        /// </summary>
        private readonly List<ICabinetUpdate> updateableCategories = new List<ICabinetUpdate>();

        /// <summary>
        /// The current window id. Used when creating windows.
        /// </summary>
        private ulong currentWindowId;

        /// <summary>
        /// Object responsible for posting the ActivityStatusEvent in standalone.
        /// </summary>
        public readonly MachineActivityManager MachineActivityManager;

        /// <summary>
        /// Object responsible for managing resources.
        /// </summary>
        private readonly ResourceManager resourceManager;

        /// <summary>
        /// Object responsible for managing hardware devices.
        /// </summary>
        private readonly DeviceManager deviceManager;

        /// <summary>
        /// Used by WaitForEvent and EnqueueEvent to wait for an event.
        /// </summary>
        private readonly AutoResetEvent eventEnqueuedEvent = new AutoResetEvent(true);

        /// <summary>
        /// The volume level set by the player.
        /// </summary>
        private float playerVolume = 0.5f;

        /// <summary>
        /// The player mute selected flag.
        /// </summary>
        private bool playerMuteSelected;

        /// <summary>
        /// CSI functionality not associated with a hardware resources.
        /// </summary>
        /// <remarks>
        /// Original base CSI functionality was implemented directly in the cabinet, however for extensibility and
        /// maintenance future categories should only be added as either resource or non-resource categories.
        /// </remarks>
        private readonly Dictionary<Type, object> nonResourceCategories = new Dictionary<Type, object>();

        /// <summary>
        /// The cached credit display type on the cabinet.
        /// </summary>
        private CabinetCreditDisplayType cabinetCachedCreditDisplayType = CabinetCreditDisplayType.NotSet;

        /// <summary>
        /// The cabinet setting parser.
        /// </summary>
        private readonly VolumeSettingsParser volumeSettingsParser;

        /// <summary>
        /// The monitor settings parser.
        /// </summary>
        private readonly MonitorSettingsParser monitorSettingsParser;

        /// <summary>
        /// The button panel settings parser.
        /// </summary>
        private readonly ButtonPanelSettingsParser buttonPanelSettingsParser;

        /// <summary>
        /// Flag indicating the game is in the process of disconnecting from the CSI.
        /// </summary>
        private volatile bool isDisconnecting;

        /// <summary>
        /// Locker for the primary cabinet event queue.
        /// </summary>
        private readonly object cabinetEventQueueLocker = new object();

        /// <summary>
        /// Cache the monitor list of monitor settings parser.
        /// </summary>
        private List<Monitor> monitorList;

        /// <summary>
        /// Cache the button panel configurations of button panel settings parser.
        /// </summary>
        private IList<IButtonPanelConfiguration> buttonPanelConfigurations;

        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// Construct a CabinetLib based on the given configuration
        /// which supports the given interfaces.
        /// </summary>
        /// <param name="systemConfigStream">
        /// A stream from which the system configuration is to be read.
        /// </param>
        /// <param name="additionalInterfaces">
        /// Additional interfaces to support. If there are base interfaces, then the values in this dictionary will
        /// override any existing values for conflicting interfaces.
        /// </param>
        public CabinetLib(Stream systemConfigStream = null, IDictionary<Type, object> additionalInterfaces = null)
            : this(systemConfigStream, additionalInterfaces, FoundationTarget.Next, "StandaloneToken", Priority.Game)
        {
        }

        /// <summary>
        /// Construct a CabinetLib based on the given configuration
        /// which supports the given interfaces.
        /// </summary>
        /// <param name="csiConfigStream">A stream from which the CSI configuration is to be read.</param>
        /// <param name="additionalInterfaces">
        /// Additional interfaces to support. If there are base interfaces, then the values in this dictionary will
        /// override any existing values for conflicting interfaces.
        /// </param>
        /// <param name="foundationTarget">
        /// The foundation target for the game.
        /// </param>
        /// <param name="token">Connection token for the client.</param>
        /// <param name="clientPriority">The client priority.</param>
        public CabinetLib(Stream csiConfigStream,
                          IDictionary<Type, object> additionalInterfaces,
                          // ReSharper disable once UnusedParameter.Local
                          FoundationTarget foundationTarget,
                          string token,
                          Priority clientPriority)
        {
            // Load the root element of the system configurations.
            var systemConfigRoot = csiConfigStream == null ? null : XElement.Load(XmlReader.Create(csiConfigStream));

            // Initialize machine activity manager.
            var sectionElement = systemConfigRoot?.Element("MachineActivity");

            MachineActivityManager = new MachineActivityManager(this, new MachineActivityParser(sectionElement));

            if(additionalInterfaces != null)
            {
                // Check and add each entity if it needs to be updated.
                updateableCategories = additionalInterfaces.Where(entry => entry.Value is ICabinetUpdate).Select(entry => (ICabinetUpdate)entry.Value).ToList();

                // An entry with a null value means to use the device hardware
                // rather than the virtual implementation.  In this case, we need
                // a device manager.
                if(additionalInterfaces.Any(entry => entry.Value == null))
                {
                    deviceManager = new DeviceManager();

                    // Do not subscribe to Device Manager events.
                    // Leave it to Resource Manager, who will filter the events
                    // based on whether the hardware is in use.
                }
            }

            var readinessImplementation = new StandaloneReadiness();
            readinessImplementation.SetConnectionInfo(token, clientPriority);
            nonResourceCategories.Add(typeof(IReadiness), readinessImplementation);
            updateableCategories.Add(readinessImplementation);

            //For now, the monitor interface is not externally configurable and is just added.
            if(additionalInterfaces == null)
            {
                additionalInterfaces = new Dictionary<Type, object>();
            }

            var monitorImplementation = new StandaloneMonitor();
            additionalInterfaces.Add(typeof(IMonitor), monitorImplementation);

            var touchScreenImplementation = new StandaloneTouchScreen();
            additionalInterfaces.Add(typeof(ITouchScreen), touchScreenImplementation);

            var depthCameraImplementation = new StandaloneDepthCamera();
            additionalInterfaces.Add(typeof(IDepthCamera), depthCameraImplementation);

            var hapticImplementation = new StandaloneHaptic();
            additionalInterfaces.Add(typeof(IHaptic), hapticImplementation);

            var mechanicalBellImplementation = new StandaloneMechanicalBell();
            additionalInterfaces.Add(typeof(IMechanicalBell), mechanicalBellImplementation);
            
            sectionElement = systemConfigRoot?.Element("ServiceSettings");
            var serviceSettingsParser = new ServiceSettingsParser(sectionElement);

            var serviceImplementation = new StandaloneService(this, serviceSettingsParser);
            additionalInterfaces.Add(typeof(IService), serviceImplementation);
            updateableCategories.Add(serviceImplementation);

            var candleImplementation = new StandaloneCandle();
            additionalInterfaces.Add(typeof(ICandle), candleImplementation);

            // Initialize cabinet settings parser.
            sectionElement = systemConfigRoot?.Element("VolumeSettings");
            volumeSettingsParser = new VolumeSettingsParser(sectionElement);

            sectionElement = systemConfigRoot?.Element("MonitorSettings");
            monitorSettingsParser = new MonitorSettingsParser(sectionElement);

            sectionElement = systemConfigRoot?.Element("ButtonPanelSettings");
            buttonPanelSettingsParser = new ButtonPanelSettingsParser(sectionElement);

            // Initialize resource manager.
            resourceManager = new ResourceManager(additionalInterfaces, deviceManager);
            resourceManager.DeviceAcquiredEvent += EnqueueEvent;
            resourceManager.DeviceReleasedEvent += EnqueueEvent;
            resourceManager.DeviceConnectedEvent += EnqueueEvent;
            resourceManager.DeviceRemovedEvent += EnqueueEvent;

            InitializeEventTable();
        }

        /// <inheritdoc />
        public void Connect()
        {
            isDisconnecting = false;
            IsConnected = true;
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            isDisconnecting = true;
            ClearQueuedEvents();
            IsConnected = false;
        }

        #endregion Constructors

        #region Private Methods

        /// <summary>
        /// Initialize the event table with the supported events.
        /// </summary>
        private void InitializeEventTable()
        {
            eventTable[typeof(ActivityStatusEventArgs)] = eventToPost => PostEvent(eventToPost, ActivityStatusEvent);
            eventTable[typeof(DeviceAcquiredEventArgs)] = eventToPost => PostEvent(eventToPost, DeviceAcquiredEvent);
            eventTable[typeof(DeviceReleasedEventArgs)] = eventToPost => PostEvent(eventToPost, DeviceReleasedEvent);
            eventTable[typeof(DeviceConnectedEventArgs)] = eventToPost => PostEvent(eventToPost, DeviceConnectedEvent);
            eventTable[typeof(DeviceRemovedEventArgs)] = eventToPost => PostEvent(eventToPost, DeviceRemovedEvent);
            eventTable[typeof(FoundationStateChangedEventArgs)] = eventToPost =>
                                                                  MachineActivityManager.FoundationStateChanged(
                                                                      eventToPost, null);
            eventTable[typeof(PlayerBankMeterChangedEventArgs)] = eventToPost =>
                                                                  MachineActivityManager.PlayerBankMeterChanged(
                                                                      eventToPost, null);
            eventTable[typeof(CabinetButtonPressedEventArgs)] =
                eventToPost => PostEvent(eventToPost, ButtonPressedEvent);
        }

        /// <summary>
        /// Posts an event by triggering an event handler passed in through the arguments.
        /// </summary>
        /// <typeparam name="TEventArgs">Event Type.</typeparam>
        /// <param name="eventToPost">Event to post.</param>
        /// <param name="handler">Event handler to call in order to post an event.</param>
        private void PostEvent<TEventArgs>(EventArgs eventToPost, EventHandler<TEventArgs> handler)
            where TEventArgs : EventArgs
        {
            handler?.Invoke(this, eventToPost as TEventArgs);
        }

        /// <summary>
        /// Process all queued up events.
        /// </summary>
        /// <param name="lockQueue">Flag determining if the caller should block while acquiring the
        /// queue lock.
        /// </param>
        private void ProcessQueuedEvents(bool lockQueue)
        {
            // The wait time determines if we block on trying to acquire the lock or not.
            int waitTime = lockQueue ? Timeout.Infinite : 0;

            Queue<EventArgs> cabinetEventQueueCopy = null;

            if(System.Threading.Monitor.TryEnter(cabinetEventQueueLocker, waitTime))
            {
                try
                {
                    if(cabinetEventQueue.Count > 0)
                    {
                        cabinetEventQueueCopy = cabinetEventQueue;
                        // RS 9.0, 9.1 flags this as a false positive. Fixed in 9.2+.
                        // ReSharper disable once InconsistentlySynchronizedField
                        cabinetEventQueue = new Queue<EventArgs>();
                    }
                }
                finally
                {
                    System.Threading.Monitor.Exit(cabinetEventQueueLocker);
                }
            }

            while(cabinetEventQueueCopy?.Count > 0)
            {
                var eventToPost = cabinetEventQueueCopy.Dequeue();
                if(eventToPost != null)
                {
                    var eventType = eventToPost.GetType();

                    if(eventTable.ContainsKey(eventType))
                    {
                        eventTable[eventType](eventToPost);
                    }
                }
            }
        }

        /// <summary>
        /// Generate the current Monitor Configuration.
        /// </summary>
        /// <returns>
        /// The current Monitor Configuration.
        /// </returns>
        private MonitorConfiguration GenerateMonitorConfiguration()
        {
            if(monitorSettingsParser.MonitorsList != null)
            {
                if(monitorList == null)
                {
                    monitorList = new List<Monitor>();
                    foreach(var monitorType in monitorSettingsParser.MonitorsList)
                    {
                        if(monitorType == null)
                        {
                            monitorList.Add(null);
                            continue;
                        }
                        var monitor = new Monitor
                        {
                            DeviceId = monitorType.DeviceId,
                            Role = (MonitorRole)monitorType.Role,
                            Style = (MonitorStyle)monitorType.Style,
                            Aspect = (MonitorAspect)monitorType.Aspect,
                            Model = monitorType.Model != null
                                ? new MonitorModel
                                {
                                    Manufacturer = monitorType.Model.Manufacturer,
                                    Model = monitorType.Model.Model,
                                    Version = monitorType.Model.Version
                                }
                                : null,
                            DesktopCoordinates = monitorType.DesktopCoordinates != null
                                ? new DesktopRectangle
                                {
                                    x = monitorType.DesktopCoordinates.x,
                                    y = monitorType.DesktopCoordinates.y,
                                    w = monitorType.DesktopCoordinates.w,
                                    h = monitorType.DesktopCoordinates.h
                                }
                                : null,
                            VirtualX = monitorType.VirtualX,
                            VirtualY = monitorType.VirtualY,
                            ColorProfileId = monitorType.ColorProfileId
                        };
                        monitorList.Add(monitor);
                    }
                }
                return new MonitorConfiguration
                {
                    Monitor = monitorList
                };
            }
            return new MonitorConfiguration();
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Add an interface to the standalone cabinet.
        /// </summary>
        /// <param name="interfaceType">The interface to add.</param>
        /// <param name="interfaceSupport">An object which supports the interface.</param>
        public void AddInterface(Type interfaceType, object interfaceSupport)
        {
            resourceManager.AddInterface(interfaceType, interfaceSupport);
        }

        /// <summary>
        /// Clear all queued events. Should be called on disconnecting.
        /// </summary>
        public void ClearQueuedEvents()
        {
            lock(cabinetEventQueueLocker)
            {
                cabinetEventQueue.Clear();
            }
        }

        #endregion Public Methods

        #region ICabinetLib Members

        /// <inheritdoc />
        /// <remarks>
        /// This event has empty add/remove methods to indicate it is unimplemented. This will prevent any warnings
        /// about the member being unused. If the event is implemented, then it should be changed to be auto-implemented.
        /// </remarks>
        public event EventHandler<WindowResizeEventArgs> WindowResizeEvent
        {
            add
            {
            }
            remove
            {
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This event has empty add/remove methods to indicate it is unimplemented. This will prevent any warnings
        /// about the member being unused. If the event is implemented, then it should be changed to be auto-implemented.
        /// </remarks>
        public event EventHandler<MultiWindowResizeEventArgs> MultiWindowResizeEvent
        {
            add
            {
            }
            remove
            {
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This event has empty add/remove methods to indicate it is unimplemented. This will prevent any warnings
        /// about the member being unused. If the event is implemented, then it should be changed to be auto-implemented.
        /// </remarks>
        public event EventHandler<WindowZOrderEventArgs> WindowZOrderEvent
        {
            add
            {
            }
            remove
            {
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This event has empty add/remove methods to indicate it is unimplemented. This will prevent any warnings
        /// about the member being unused. If the event is implemented, then it should be changed to be auto-implemented.
        /// </remarks>
        public event EventHandler<SoundVolumeChangedEventArgs> SoundVolumeChangedEvent
        {
            add
            {
            }
            remove
            {
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This event has empty add/remove methods to indicate it is unimplemented. This will prevent any warnings
        /// about the member being unused. If the event is implemented, then it should be changed to be auto-implemented.
        /// </remarks>
        public event EventHandler<SoundVolumeMuteAllStatusChangedEventArgs> SoundVolumeMuteAllStatusChangedEvent
        {
            add
            {
            }
            remove
            {
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This event has empty add/remove methods to indicate it is unimplemented. This will prevent any warnings
        /// about the member being unused. If the event is implemented, then it should be changed to be auto-implemented.
        /// </remarks>
        public event EventHandler<SoundVolumePlayerLevelChangedEventArgs> SoundVolumePlayerLevelChangedEvent
        {
            add
            {
            }
            remove
            {
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This event has empty add/remove methods to indicate it is unimplemented. This will prevent any warnings
        /// about the member being unused. If the event is implemented, then it should be changed to be auto-implemented.
        /// </remarks>
        public event EventHandler<SoundVolumePlayerSelectableStatusChangedEventArgs>
            SoundVolumePlayerSelectableStatusChangedEvent
        {
            add
            {
            }
            remove
            {
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// This event has empty add/remove methods to indicate it is unimplemented. This will prevent any warnings
        /// about the member being unused. If the event is implemented, then it should be changed to be auto-implemented.
        /// </remarks>
        public event EventHandler<HeadphoneJackChangedEventArgs> HeadphoneJackChangedEvent
        {
            add
            {
            }
            remove
            {
            }
        }

        /// <inheritdoc />
        public event EventHandler<DeviceAcquiredEventArgs> DeviceAcquiredEvent;

        /// <inheritdoc />
        /// <remarks>
        /// In standalone build, this event usually indicates a device tilt.
        /// </remarks>
        public event EventHandler<DeviceReleasedEventArgs> DeviceReleasedEvent;

        /// <inheritdoc />
        /// <remarks>
        /// In standalone build, this event could clear a previous device tilt.
        /// </remarks>
        public event EventHandler<DeviceConnectedEventArgs> DeviceConnectedEvent;

        /// <inheritdoc />
        /// <remarks>
        /// In standalone build, this event usually indicates a device tilt.
        /// </remarks>
        public event EventHandler<DeviceRemovedEventArgs> DeviceRemovedEvent;

        /// <inheritdoc />
        public event EventHandler<CabinetButtonPressedEventArgs> ButtonPressedEvent;

        /// <inheritdoc />
        public event EventHandler<ActivityStatusEventArgs> ActivityStatusEvent;

        /// <inheritdoc />
        /// <remarks>
        /// This event has empty add/remove methods to indicate it is unimplemented. This will prevent any warnings
        /// about the member being unused. If the event is implemented, then it should be changed to be auto-implemented.
        /// </remarks>
        public event EventHandler<AttractAestheticConfigurationEventArgs> AttractAestheticConfigurationChangedEvent
        {
            add
            {
            }
            remove
            {
            }
        }

        /// <inheritdoc />
        public event EventHandler<CabinetEventArgs> CabinetEvent;

        /// <inheritdoc />
        public void WaitForEvent(Type eventType)
        {
            while(true)
            {
                lock(cabinetEventQueueLocker)
                {
                    if(cabinetEventQueue.Any(args => args.GetType() == eventType))
                    {
                        break;
                    }
                }
                eventEnqueuedEvent.WaitOne();
            }
        }

        /// <inheritdoc />
        public bool IsConnected { get; set; }

        /// <inheritdoc />
        public void Update()
        {
            deviceManager?.Update();

            ProcessQueuedEvents(false);

            // Update each installed interface if needed.
            foreach(var updateCategory in updateableCategories)
            {
                updateCategory.Update();
            }
        }

        /// <inheritdoc />
        public void SetAllPixels(ButtonIdentifier buttonId, bool pixelState, bool errorOnFailure = true)
        {
            if(buttonId == null)
            {
                throw new ArgumentNullException(nameof(buttonId));
            }
        }

        /// <inheritdoc />
        public ulong CreateWindow(bool canHandleMld, Priority priority, List<long> windowHandles)
        {
            return CreateWindow(canHandleMld, priority, windowHandles, true);
        }

        /// <inheritdoc />
        public ulong CreateWindow(bool canHandleMld, Priority priority, List<long> windowHandles, bool multiTouchNativelySupported)
        {
            currentWindowId++;
            return currentWindowId;
        }

        /// <inheritdoc />
        public MonitorComposition RequestMonitorConfiguration()
        {
            var monitorConfig = GenerateMonitorConfiguration();

            return new MonitorComposition(monitorConfig.Monitor, new DesktopRectangle { h = 1, w = 1, x = 0, y = 0 });
        }

        /// <inheritdoc />
        public void DestroyWindow(ulong windowId)
        {
        }

        /// <inheritdoc />
        public void RequestRepositionWindow(ulong windowId, DesktopRectangle viewPortExtents, ViewportList viewports)
        {
            RequestRepositionWindow(windowId, WindowType.Dockable, viewPortExtents, viewports);
        }

        /// <inheritdoc />
        public void RequestRepositionWindow(ulong windowId, WindowType windowType, DesktopRectangle viewPortExtents,
                                            ViewportList viewports)
        {
        }

        /// <inheritdoc />
        public void RequestRepositionMultiWindows(IList<Window> windows)
        {
        }

        /// <inheritdoc />
        public void SendWindowResizeComplete(ulong requestId)
        {
            // Since the WindowResizeEvent currently has nothing that triggers it in the standalone implementation, this
            // method should never be allowed to be called since no request would have been started. If the resize event is
            // made functional in a future change then this method will need to be updated so it only throws the exception
            // if a request is not in progress.
            throw new InvalidOperationException("The complete message cannot be sent while no window resize request is in progress.");
        }

        /// <inheritdoc />
        public void SendMultiWindowResizeComplete(ulong requestId)
        {
            // Since the MultiWindowResizeEvent currently has nothing that triggers it in the standalone implementation, this
            // method should never be allowed to be called since no request would have been started. If the resize event is
            // made functional in a future change then this method will need to be updated so it only throws the exception
            // if a request is not in progress.
            throw new InvalidOperationException("The complete message cannot be sent while no multi window resize request is in progress.");
        }

        /// <inheritdoc/>
        public AcquireDeviceResult RequestAcquireDevice(DeviceType deviceType, string deviceId, Priority priority)
        {
            return resourceManager.AcquireDevice(deviceType, deviceId);
        }

        /// <inheritdoc />
        public AcquireGroupsResult RequestAcquireGroups(DeviceType deviceType, string deviceId, Priority priority,
            List<uint> groupList)
        {
            return new AcquireGroupsResult(true, null, new Dictionary<uint, GroupAcquisitionStatus>());
        }

        /// <inheritdoc />
        public IList<ConnectedDevice> GetConnectedDevicesWithGroups()
        {
            return new List<ConnectedDevice>();
        }

        /// <inheritdoc/>
        public bool DeviceAcquired(DeviceType deviceType, string deviceId)
        {
            return resourceManager.DeviceAcquired(deviceType, deviceId);
        }

        /// <inheritdoc/>
        public bool GroupAcquired(DeviceType deviceType, string deviceId, uint groupId)
        {
            return false;
        }

        /// <inheritdoc/>
        public IList<DeviceIdentifier> GetConnectedDevices()
        {
            return resourceManager.GetInstalledDevices();
        }

        /// <inheritdoc/>
        public void ReleaseDevice(DeviceType deviceType, string deviceId)
        {
            resourceManager.ReleaseDevice(deviceType, deviceId);
        }

        /// <inheritdoc/>
        public void ReleaseGroups(DeviceType deviceType, string deviceId, List<uint> groupList)
        {
        }

        /// <inheritdoc/>
        public void RequestEventRegistration(DeviceType deviceType, string deviceId)
        {
        }

        /// <inheritdoc/>
        public void ReleaseEventRegistration(DeviceType deviceType, string deviceId)
        {
        }

        /// <inheritdoc />
        public void SetLampState(IList<ButtonLampState> buttonLamps)
        {
            if(buttonLamps == null)
            {
                throw new ArgumentNullException(nameof(buttonLamps));
            }
        }

        /// <inheritdoc />
        public void SetLampState(ButtonIdentifier lampId, bool lampState)
        {
            if(lampId == null)
            {
                throw new ArgumentNullException(nameof(lampId));
            }
        }

        /// <inheritdoc />
        public bool GetLampState(ButtonIdentifier buttonId, bool errorOnFailure = true)
        {
            if(buttonId == null)
            {
                throw new ArgumentNullException(nameof(buttonId));
            }

            return false;
        }

        /// <inheritdoc />
        public int SendImageSet(string fileName, ButtonPanelLocation panelLocation, bool errorOnFailure = true)
        {
            return 0;
        }

        /// <inheritdoc />
        public void RemoveImageSet(ushort imageSetId, ButtonPanelLocation panelLocation, bool errorOnFailure = true)
        {
        }

        /// <inheritdoc />
        public void PlayButtonAnimation(ButtonIdentifier buttonId, ushort imageSetId, ushort animationId, ushort frameDelay,
            bool repeat, bool transitionMode, bool errorOnFailure = true)
        {
            if(buttonId == null)
            {
                throw new ArgumentNullException(nameof(buttonId));
            }
        }

        /// <inheritdoc />
        public bool GetDeviceId(ButtonPanelLocation panelLocation, out string deviceId)
        {
            deviceId = null;
            return true;
        }

        /// <inheritdoc />
        public IList<IButtonPanelConfiguration> GetButtonPanelConfigurations(bool errorOnFailure = true)
        {
            return buttonPanelConfigurations ?? (buttonPanelConfigurations = buttonPanelSettingsParser.GetButtonPanelConfigurations());
        }

        /// <inheritdoc />
        public void LockSolenoid()
        {
        }

        /// <inheritdoc />
        public void UnlockSolenoid()
        {
        }

        /// <inheritdoc />
        public void ClickSolenoid()
        {
        }

        /// <inheritdoc />
        public float GetVolume(GroupName soundGroupName)
        {
            return 1.0f;
        }

        /// <inheritdoc />
        public Dictionary<GroupName, float> GetVolumeAll()
        {
            return Enum.GetValues(typeof(GroupName)).OfType<GroupName>().ToDictionary(k => k, e => 1.0f);
        }

        /// <inheritdoc />
        public bool IsMuteAll()
        {
            return volumeSettingsParser.MuteAll;
        }

        /// <inheritdoc />
        public VolumeSelectableInfo GetVolumePlayerSelectableInfo()
        {
            return new VolumeSelectableInfo(volumeSettingsParser.VolumePlayerSelectable,
                                            volumeSettingsParser.VolumePlayerMuteSelectable);
        }

        /// <inheritdoc />
        public bool SetPlayerVolumeInfo(PlayerVolumeInfo volumeInfo)
        {
            var level = volumeInfo.VolumePlayerLevel;
            if(level >= 0 && level <= 1)
            {
                playerVolume = level;
                playerMuteSelected = volumeInfo.PlayerMuteSelected;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(volumeInfo), "volumeInfo.level should be 0 - 1.");
            }
            return true;
        }

        /// <inheritdoc />
        public PlayerVolumeSettings GetPlayerVolumeSettings()
        {
            return new PlayerVolumeSettings(playerVolume, playerMuteSelected, 0.0f);
        }

        /// <inheritdoc />
        public AttractAestheticConfiguration RequestAttractAestheticConfiguration()
        {
            return new AttractAestheticConfiguration
            {
                GameAttractStyle = GameAttractStyle.Subtle,
                GameAttractPlaylistGroup = 1
            };
        }

        /// <inheritdoc />
        public bool RequestCabinetEventRegistration(bool registerForEvents)
        {
            return false;
        }

        /// <inheritdoc />
        public MachineActivityStatus RequestActivityStatus()
        {
            return MachineActivityManager.RequestActivityStatus();
        }

        /// <inheritdoc />
        public CabinetCreditDisplayType GetCreditDisplayType()
        {
            return cabinetCachedCreditDisplayType;
        }

        /// <inheritdoc />
        public void SetCreditDisplayType(CabinetCreditDisplayType creditDisplayType)
        {
            cabinetCachedCreditDisplayType = creditDisplayType;
        }

        /// <inheritdoc />
        public TInterface GetInterface<TInterface>() where TInterface : class
        {
            var acquiredInterface = resourceManager.GetInterface<TInterface>();

            if(acquiredInterface == null)
            {
                var interfaceType = typeof(TInterface);
                if(nonResourceCategories.ContainsKey(interfaceType))
                {
                    acquiredInterface = (TInterface)nonResourceCategories[interfaceType];
                }
            }
            return acquiredInterface;
        }
        #endregion ICabinetLib Members

        #region ICabinetLibDemo Members

        /// <inheritdoc />
        public void EnqueueEvent(object sender, EventArgs eventArgs)
        {
            lock(cabinetEventQueueLocker)
            {
                // Don't queue up any new events during the disconnect process.
                if(!isDisconnecting)
                {
                    cabinetEventQueue.Enqueue(eventArgs);
                }
                eventEnqueuedEvent.Set();
            }
        }

        /// <inheritdoc />
        public void ReportUserInput()
        {
            MachineActivityManager.ReportUserInput();
        }

        #endregion ICabinetLibDemo Members

        #region IDisposable Members

        /// <summary>
        /// Dispose unmanaged and disposable resources held by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose resources held by this object.
        /// </summary>
        /// <param name="disposing">
        /// Flag indicating if the object is being disposed. If true Dispose was called, if false the finalizer called
        /// this function. If the finalizer called the function, then only unmanaged resources should be released.
        /// </param>
        private void Dispose(bool disposing)
        {
            if(!disposed && disposing)
            {
                deviceManager?.Dispose();
                eventEnqueuedEvent.Dispose();
            }

            disposed = true;
        }

        #endregion IDisposable Members
    }
}
