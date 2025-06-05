//-----------------------------------------------------------------------
// <copyright file = "CabinetLib.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Communication;
    using CSI.Schemas;
    using CsiTransport;
    using Cabinet;
    using Threading;
    using Tracing;

    /// <summary>
    /// This is the cabinet communications library for the CSI communications
    /// link. Standard implementation.
    /// </summary>
    public class CabinetLib : ICabinetLib, IDisposable
    {
        #region Properties

        /// <summary>
        /// Transport used to communicate with the CSIManager.
        /// </summary>
        public CsiTransport Transport { get; }

        #endregion Properties

        #region Fields

        ///<summary>
        /// A value representing the maximum volume attenuation level.
        ///</summary>
        public const float MaxVolumeAttenuationLevel = 10000.0f;

        /// <summary>
        /// A value representing the maximum volume scale.
        /// </summary>
        public const float MaxVolumeScale = 1.0f;

        /// <summary>
        /// Queue of events from the CSI Manager.
        /// </summary>
        private Queue<EventArgs> cabinetEventQueue = new Queue<EventArgs>();

        /// <summary>
        /// Flag which indicates if this object has been disposed or not.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Button category implementation.
        /// </summary>
        private readonly ButtonCategory buttonCategory;

        /// <summary>
        /// Resource management category implementation.
        /// </summary>
        private readonly ResourceManagementCategory resourceManagementCategory;

        /// <summary>
        /// Window management category implementation.
        /// </summary>
        private readonly WindowManagementCategory windowManagementCategory;

        /// <summary>
        /// Sound category implementation.
        /// </summary>
        private readonly SoundCategory soundCategory;

        /// <summary>
        /// Solenoid category implementation.
        /// </summary>
        private readonly SolenoidCategory solenoidCategory;

        /// <summary>
        /// Cabinet status category implementation.
        /// </summary>
        private readonly CabinetStatusCategory cabinetStatusCategory;

        /// <summary>
        /// Table of event handlers to their associated event arguments.
        /// </summary>
        private readonly Dictionary<Type, Action<EventArgs>> eventTable = new Dictionary<Type, Action<EventArgs>>();

        /// <summary>
        /// Used by WaitForEvent and EnqueueEvent to wait for an event.
        /// </summary>
        private readonly AutoResetEvent eventEnqueuedEvent = new AutoResetEvent(false);

        /// <summary>
        /// List of the installed categories.
        /// </summary>
        private readonly List<ICabinetCategory> installedCategories = new List<ICabinetCategory>();

        /// <summary>
        /// List of categories that need to have their event queues dequeued.
        /// </summary>
        private readonly List<ICabinetUpdate> updateableCategories = new List<ICabinetUpdate>();

        /// <summary>
        /// Dictionary used to cache interface types to the category which supports them.
        /// </summary>
        private readonly Dictionary<Type, ICabinetCategory> categoryMap = new Dictionary<Type, ICabinetCategory>();

        /// <summary>
        /// Indicates if the window is visible.
        /// </summary>
        private bool isWindowVisible;

        /// <summary>
        /// Indicates if the button panels have been registered.
        /// </summary>
        private readonly Dictionary<string, bool> panelEventRegistrationInfo = new Dictionary<string, bool>();

        /// <summary>
        /// Indicates if the button panels have been acquired.
        /// </summary>
        private readonly Dictionary<string, bool> panelAcquisitionInfo = new Dictionary<string, bool>();

        /// <summary>
        /// A place holder string for panel device Id, since it might be null from foundation previous to G series.
        /// This is agreed with Foundation.
        /// </summary>
        private const string NoDeviceId = "NoDeviceId";

        /// <summary>
        /// Flag indicating the game is in the process of disconnecting from the CSI.
        /// </summary>
        private volatile bool isDisconnecting;

        /// <summary>
        /// Locker for the primary cabinet event queue.
        /// </summary>
        private readonly object cabinetEventQueueLocker = new object();

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of a <see cref="CabinetLib"/>.
        /// </summary>
        /// <param name="address">The IP address to use to connect to the CSI Manager.</param>
        /// <param name="port">The port to connect on.</param>
        /// <param name="token">Token used by the CSI manager to identify the game instance.</param>
        /// <param name="clientType">The client type of the connecting application.</param>
        /// <param name="foundationTarget">The foundation version to target.</param>
        /// <exception cref="ArgumentNullException">Thrown when the address is null.</exception>
        public CabinetLib(string address, ushort port, string token, ClientType clientType,
            FoundationTarget foundationTarget)
        {
            if(address == null)
            {
                throw new ArgumentNullException(nameof(address), "Parameter may not be null.");
            }

            InitializeEventTable();

            var socketTransport = new CsiSocketTransport(address, port);
            var connectHandler = new ConnectHandler(token, clientType);

            Transport = new CsiTransport(socketTransport);

            // Any categories that raise events and/or need regular service updates synced with the
            // cabinet lib and game frame timer update calls should implement ICabinetUpdate
            // and add themselves to updateableCategories.

            buttonCategory = new ButtonCategory(foundationTarget);
            buttonCategory.ButtonPressedEvent += EnqueueEvent;
            connectHandler.RequestCategory(buttonCategory);
            updateableCategories.Add(buttonCategory);

            resourceManagementCategory = new ResourceManagementCategory(foundationTarget);

            resourceManagementCategory.DeviceAcquiredEvent += EnqueueEvent;
            resourceManagementCategory.DeviceReleasedEvent += EnqueueEvent;
            resourceManagementCategory.DeviceConnectedEvent += EnqueueEvent;
            resourceManagementCategory.DeviceRemovedEvent += EnqueueEvent;

            connectHandler.RequestCategory(resourceManagementCategory);

            windowManagementCategory = new WindowManagementCategory(foundationTarget);
            windowManagementCategory.ChangeZOrderEvent += EnqueueEvent;
            windowManagementCategory.WindowResizeEvent += EnqueueEvent;
            windowManagementCategory.MultiWindowResizeEvent += EnqueueEvent;
            connectHandler.RequestCategory(windowManagementCategory);

            cabinetStatusCategory = new CabinetStatusCategory(foundationTarget);
            cabinetStatusCategory.ActivityStatusEvent += EnqueueEvent;
            cabinetStatusCategory.AttractAestheticChangedEvent += EnqueueEvent;
            cabinetStatusCategory.CabinetEvent += EnqueueEvent;
            connectHandler.RequestCategory(cabinetStatusCategory);

            soundCategory = new SoundCategory(foundationTarget);
            soundCategory.SoundVolumeChangedEvent += EnqueueEvent;
            soundCategory.SoundVolumeMuteAllStatusChangedEvent += EnqueueEvent;
            soundCategory.SoundVolumePlayerSelectableStatusChangedEvent += EnqueueEvent;
            soundCategory.SoundVolumePlayerLevelChangedEvent += EnqueueEvent;
            soundCategory.HeadphoneJackChangedEvent += EnqueueEvent;

            connectHandler.RequestCategory(soundCategory);

            var lightCategory = new PeripheralLightsCategory(foundationTarget);
            connectHandler.RequestCategory(lightCategory);
            updateableCategories.Add(lightCategory);

            var mechanicalReelsCategory = new MechanicalReelsCategory(foundationTarget);
            connectHandler.RequestCategory(mechanicalReelsCategory);
            updateableCategories.Add(mechanicalReelsCategory);

            solenoidCategory = new SolenoidCategory();
            connectHandler.RequestCategory(solenoidCategory);

            var streamingLightCategory = new StreamingLightsCategory(foundationTarget);
            connectHandler.RequestCategory(streamingLightCategory);
            updateableCategories.Add(streamingLightCategory);

            var bankSynchronizationCategory = new BankSynchronizationCategory(foundationTarget);
            connectHandler.RequestCategory(bankSynchronizationCategory);
            updateableCategories.Add(bankSynchronizationCategory);

            var videoTopperCategory = new VideoTopperCategory(foundationTarget);
            connectHandler.RequestCategory(videoTopperCategory);
            updateableCategories.Add(videoTopperCategory);

            var readinessCategory = new ReadinessCategory();
            connectHandler.RequestCategory(readinessCategory);
            updateableCategories.Add(readinessCategory);

            var monitorCategory = new MonitorCategory(foundationTarget);
            connectHandler.RequestCategory(monitorCategory);

            var touchScreenCategory = new TouchScreenCategory(foundationTarget);
            connectHandler.RequestCategory(touchScreenCategory);
            updateableCategories.Add(touchScreenCategory);

            var portalCategory = PortalCategory.CreateInstance(foundationTarget);
            if(portalCategory != null)
            {
                connectHandler.RequestCategory(portalCategory);
                updateableCategories.Add(portalCategory);
            }

            var depthCameraCategory = DepthCameraCategory.CreateInstance();
            if(depthCameraCategory != null)
            {
                connectHandler.RequestCategory(depthCameraCategory);
            }

            var presentationSpeedCategory = PresentationSpeedCategory.CreateInstance(foundationTarget);
            if(presentationSpeedCategory != null)
            {
                connectHandler.RequestCategory(presentationSpeedCategory);
            }

            var hapticCategory = HapticCategory.CreateInstance(foundationTarget);
            if(hapticCategory != null)
            {
                connectHandler.RequestCategory(hapticCategory);
            }

            var mechanicalBellCategory = MechanicalBellCategory.CreateInstance(foundationTarget);
            if(mechanicalBellCategory != null)
            {
                connectHandler.RequestCategory(mechanicalBellCategory);
            }

            var serviceCategory = ServiceCategory.CreateInstance(foundationTarget);
            if(serviceCategory != null)
            {
                connectHandler.RequestCategory(serviceCategory);
                updateableCategories.Add(serviceCategory);
            }

            var emulatedCashoutService = EmulatedCashoutServiceCategory.CreateInstance();
            if(emulatedCashoutService != null)
            {
                connectHandler.RequestCategory(emulatedCashoutService);
                updateableCategories.Add(emulatedCashoutService);
            }

            Transport.InstallConnectHandler(connectHandler);
            connectHandler.CategoryInstalledEvent += OnCategoryInstalled;

            DeviceAcquiredEvent += HandleDeviceAcquiredEvent;
            WindowResizeEvent += HandleWindowResizeEvent;
            MultiWindowResizeEvent += HandleMultiWindowResizeEvent;
            DeviceReleasedEvent += HandleDeviceReleasedEvent;
        }

        /// <inheritdoc />
        public void Connect()
        {
            isDisconnecting = false;
            ClearButtonPanelInfo();
            isWindowVisible = false;
            if(!IsConnected)
            {
                IsConnected = true;
                Transport.Connect();

                GameLifeCycleTracing.Log.CabinetLibConnected(true);
            }
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            isDisconnecting = true;

            // Clear any messages queued up before disconnection.
            ClearQueuedEvents();

            if(IsConnected)
            {
                resourceManagementCategory.ResetAllDevices();
                Transport.Disconnect();
                IsConnected = false;
                windowManagementCategory.ClearPendingRequests();
            }

            ClearInstalledCategories();

            // Report the exception for this connection right away.
            // Don't wait till next connect attempt.
            CheckException();
        }

        #endregion Constructor

        #region Private Methods

        /// <summary>
        /// Check if any exception has occurred during the transport communication.
        /// If yes, throw it.
        /// </summary>
        private void CheckException()
        {
            var exception = Transport.TransportExceptionMonitor.CheckException();
            if(exception != null)
            {
                throw new RelayedException("Unhandled exception from CSI transport thread.", exception);
            }
        }

        /// <summary>
        /// Event handler which is called when a category is installed.
        /// </summary>
        /// <param name="sender">The originator of the event.</param>
        /// <param name="categoryInstalledEventArgs">Event arguments containing the installed category.</param>
        private void OnCategoryInstalled(object sender, CategoryInstalledEventArgs categoryInstalledEventArgs)
        {
            installedCategories.Add(categoryInstalledEventArgs.InstalledCategory);
        }

        /// <summary>
        /// Clear the installed categories.
        /// </summary>
        private void ClearInstalledCategories()
        {
            installedCategories.Clear();
        }

        /// <summary>
        /// En-queue the given event.
        /// </summary>
        /// <param name="sender">Originator of the event.</param>
        /// <param name="eventArgs">The event to en-queue.</param>
        private void EnqueueEvent(object sender, EventArgs eventArgs)
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

        /// <summary>
        /// Process all queued up events.
        /// </summary>
        /// <param name="lockQueue">Flag determining if the caller should block while acquiring the
        /// queue lock.
        /// </param>
        private void ProcessQueuedEvents(bool lockQueue)
        {
            // The wait time determines if we block on trying to acquire the lock or not.
            var waitTime = lockQueue ? Timeout.Infinite : 0;

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

            while(cabinetEventQueueCopy != null && cabinetEventQueueCopy.Count > 0)
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
        /// Clear all queued events. Should be called on disconnecting.
        /// </summary>
        private void ClearQueuedEvents()
        {
            lock(cabinetEventQueueLocker)
            {
                cabinetEventQueue.Clear();
            }
        }

        /// <summary>
        /// Initialize the event table with the supported events.
        /// </summary>
        private void InitializeEventTable()
        {
            eventTable[typeof(WindowResizeEventArgs)] = eventToPost => PostEvent(eventToPost, WindowResizeEvent);
            eventTable[typeof(MultiWindowResizeEventArgs)] = eventToPost => PostEvent(eventToPost, MultiWindowResizeEvent);
            eventTable[typeof(WindowZOrderEventArgs)] = eventToPost => PostEvent(eventToPost, WindowZOrderEvent);
            eventTable[typeof(CabinetButtonPressedEventArgs)] =
                eventToPost => PostEvent(eventToPost, ButtonPressedEvent);
            eventTable[typeof(ActivityStatusEventArgs)] = eventToPost => PostEvent(eventToPost, ActivityStatusEvent);
            eventTable[typeof(DeviceAcquiredEventArgs)] = eventToPost => PostEvent(eventToPost, DeviceAcquiredEvent);
            eventTable[typeof(DeviceReleasedEventArgs)] = eventToPost => PostEvent(eventToPost, DeviceReleasedEvent);
            eventTable[typeof(DeviceConnectedEventArgs)] = eventToPost => PostEvent(eventToPost, DeviceConnectedEvent);
            eventTable[typeof(DeviceRemovedEventArgs)] = eventToPost => PostEvent(eventToPost, DeviceRemovedEvent);
            eventTable[typeof(SoundVolumeChangedEventArgs)] =
                eventToPost => PostEvent(eventToPost, SoundVolumeChangedEvent);
            eventTable[typeof(SoundVolumePlayerSelectableStatusChangedEventArgs)] =
                eventToPost => PostEvent(eventToPost, SoundVolumePlayerSelectableStatusChangedEvent);
            eventTable[typeof(SoundVolumePlayerLevelChangedEventArgs)] =
                eventToPost => PostEvent(eventToPost, SoundVolumePlayerLevelChangedEvent);
            eventTable[typeof(SoundVolumeMuteAllStatusChangedEventArgs)] =
                eventToPost => PostEvent(eventToPost, SoundVolumeMuteAllStatusChangedEvent);
            eventTable[typeof(AttractAestheticConfigurationEventArgs)] =
                eventToPost => PostEvent(eventToPost, AttractAestheticConfigurationChangedEvent);
            eventTable[typeof(CabinetEventArgs)] =
                eventToPost => PostEvent(eventToPost, CabinetEvent);
            eventTable[typeof(HeadphoneJackChangedEventArgs)] =
                eventToPost => PostEvent(eventToPost, HeadphoneJackChangedEvent);
        }

        /// <summary>
        /// Post the given event with the given event handler.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the EventArgs the event uses.</typeparam>
        /// <param name="eventToPost">The event to post.</param>
        /// <param name="handler">The handler for the event.</param>
        private void PostEvent<TEventArgs>(EventArgs eventToPost, EventHandler<TEventArgs> handler)
            where TEventArgs : EventArgs
        {
            //Do not need to cache the handler as it was passed to this method.
            handler?.Invoke(this, eventToPost as TEventArgs);
        }

        /// <summary>
        /// Clear the cached button panel acquisition and registration state information.
        /// </summary>
        private void ClearButtonPanelInfo()
        {
            panelAcquisitionInfo.Clear();
            panelEventRegistrationInfo.Clear();
        }

        /// <summary>
        /// Update the acquisition state for a specific panel.
        /// </summary>
        /// <param name="panelDeviceId">the device Id of the panel to be updated.</param>
        /// <param name="acquired">The acquisition state to be set.</param>
        private void UpdateButtonPanelAcquisitionState(string panelDeviceId, bool acquired)
        {
            // For foundation previous to G series, the panel device Id might be null.
            var deviceId = panelDeviceId ?? NoDeviceId;
            if(panelAcquisitionInfo.ContainsKey(deviceId))
            {
                panelAcquisitionInfo[deviceId] = acquired;
            }
            else
            {
                panelAcquisitionInfo.Add(deviceId, acquired);
            }
        }

        /// <summary>
        /// Update the registration state for a specific panel.
        /// </summary>
        /// <param name="panelDeviceId">the device Id of the panel to be updated.</param>
        /// <param name="registered">The registration state to be set.</param>
        private void UpdateButtonPanelRegistrationState(string panelDeviceId, bool registered)
        {
            // For foundation previous to G series, the panel device Id might be null.
            var deviceId = panelDeviceId ?? NoDeviceId;
            if(panelEventRegistrationInfo.ContainsKey(deviceId))
            {
                panelEventRegistrationInfo[deviceId] = registered;
            }
            else
            {
                panelEventRegistrationInfo.Add(deviceId, registered);
            }
        }

        /// <summary>
        /// Check if a specific panel is acquired.
        /// </summary>
        /// <param name="panelDeviceId">The device Id of the panel to be checked.</param>
        /// <returns>True if the panel is acquired. Otherwise, false.</returns>
        private bool IsButtonPanelAcquired(string panelDeviceId)
        {
            // For foundation previous to G series, the panel device Id might be null.
            var deviceId = panelDeviceId ?? NoDeviceId;
            return panelAcquisitionInfo.ContainsKey(deviceId) && panelAcquisitionInfo[deviceId];
        }

        /// <summary>
        /// Check if a specific panel is registered.
        /// </summary>
        /// <param name="panelDeviceId">The device Id of the panel to be checked.</param>
        /// <returns>True if the panel is registered. Otherwise, false.</returns>
        private bool IsButtonPanelRegistered(string panelDeviceId)
        {
            // For foundation previous to G series, the panel device Id might be null.
            var deviceId = panelDeviceId ?? NoDeviceId;
            return panelEventRegistrationInfo.ContainsKey(deviceId) && panelEventRegistrationInfo[deviceId];
        }

        /// <summary>
        /// Handle a device acquired event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="deviceAcquiredEvent">Information about the acquired device.</param>
        private void HandleDeviceAcquiredEvent(object sender, DeviceAcquiredEventArgs deviceAcquiredEvent)
        {
            // Button panels require that the game register to receive events for them.
            if(deviceAcquiredEvent.DeviceName == DeviceType.ButtonPanel)
            {
                UpdateButtonPanelAcquisitionState(deviceAcquiredEvent.DeviceId, true);
                UpdateButtonPanelRegistration(deviceAcquiredEvent.DeviceId);
            }
        }

        /// <summary>
        /// Handle window resize event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="windowResizeEvent">Information about the current window.</param>
        private void HandleWindowResizeEvent(object sender, WindowResizeEventArgs windowResizeEvent)
        {
            isWindowVisible = windowResizeEvent.RequestedWindow.Status;
            UpdateButtonPanelRegistration();
        }

        /// <summary>
        /// Handle multi-windows resize event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="multiWindowResizeEvent">Information about the multi-windows.</param>
        private void HandleMultiWindowResizeEvent(object sender, MultiWindowResizeEventArgs multiWindowResizeEvent)
        {
            foreach(var requestedWindow in multiWindowResizeEvent.RequestedWindows)
            {
                isWindowVisible = requestedWindow.Status;
                UpdateButtonPanelRegistration();
            }
        }

        /// <summary>
        /// Handle a device released event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="deviceReleasedEvent">Information about the released device.</param>
        private void HandleDeviceReleasedEvent(object sender, DeviceReleasedEventArgs deviceReleasedEvent)
        {
            if(deviceReleasedEvent.DeviceName == DeviceType.ButtonPanel)
            {
                UpdateButtonPanelAcquisitionState(deviceReleasedEvent.DeviceId, false);
                UpdateButtonPanelRegistration(deviceReleasedEvent.DeviceId);
            }
        }

        /// <summary>
        /// Registers/Unregisters all the button panels.
        /// </summary>
        private void UpdateButtonPanelRegistration()
        {
            foreach(var panelInfo in panelAcquisitionInfo)
            {
                // restore the device Id as null if needed.
                var deviceId = panelInfo.Key == NoDeviceId ? null : panelInfo.Key;
                UpdateButtonPanelRegistration(deviceId);
            }
        }

        /// <summary>
        /// Registers/Unregisters a specific button panel.
        /// </summary>
        /// <param name="deviceId">the deviceId of the panel.</param>
        private void UpdateButtonPanelRegistration(string deviceId)
        {
            var isAcquired = IsButtonPanelAcquired(deviceId);
            var isRegistered = IsButtonPanelRegistered(deviceId);

            if(isWindowVisible && isAcquired && !isRegistered)
            {
                RequestEventRegistration(DeviceType.ButtonPanel, deviceId);
                UpdateButtonPanelRegistrationState(deviceId, true);
            }
            else if((!isAcquired || !isWindowVisible) && isRegistered)
            {
                ReleaseEventRegistration(DeviceType.ButtonPanel, deviceId);
                UpdateButtonPanelRegistrationState(deviceId, false);
            }
        }

        /// <summary>
        /// Get the monitor composition using the monitor category.
        /// </summary>
        /// <returns>The window composition.</returns>
        private MonitorComposition GetMonitorCompositionUsingMonitorCategory()
        {
            var monitorCategory = GetInterface<IMonitor>();

            if(monitorCategory == null)
            {
                throw new InvalidOperationException("Foundation target requires monitor category support.");
            }

            return monitorCategory.GetComposition();
        }

        #endregion Private Methods

        #region ICabinetLib Members

        /// <inheritdoc />
        public event EventHandler<WindowResizeEventArgs> WindowResizeEvent;

        /// <inheritdoc />
        public event EventHandler<MultiWindowResizeEventArgs> MultiWindowResizeEvent;

        /// <inheritdoc />
        public event EventHandler<WindowZOrderEventArgs> WindowZOrderEvent;

        /// <inheritdoc />
        public event EventHandler<SoundVolumeChangedEventArgs> SoundVolumeChangedEvent;

        /// <inheritdoc />
        public event EventHandler<SoundVolumePlayerSelectableStatusChangedEventArgs> SoundVolumePlayerSelectableStatusChangedEvent;

        /// <inheritdoc />
        public event EventHandler<SoundVolumePlayerLevelChangedEventArgs> SoundVolumePlayerLevelChangedEvent;

        /// <inheritdoc />
        public event EventHandler<SoundVolumeMuteAllStatusChangedEventArgs> SoundVolumeMuteAllStatusChangedEvent;

        /// <inheritdoc />
        public event EventHandler<HeadphoneJackChangedEventArgs> HeadphoneJackChangedEvent;

        /// <inheritdoc />
        public event EventHandler<CabinetButtonPressedEventArgs> ButtonPressedEvent;

        /// <inheritdoc />
        public event EventHandler<ActivityStatusEventArgs> ActivityStatusEvent;

        /// <inheritdoc />
        public event EventHandler<AttractAestheticConfigurationEventArgs> AttractAestheticConfigurationChangedEvent;

        /// <inheritdoc />
        public event EventHandler<CabinetEventArgs> CabinetEvent;

        /// <inheritdoc />
        public event EventHandler<DeviceAcquiredEventArgs> DeviceAcquiredEvent;

        /// <inheritdoc />
        public event EventHandler<DeviceReleasedEventArgs> DeviceReleasedEvent;

        /// <inheritdoc />
        public event EventHandler<DeviceConnectedEventArgs> DeviceConnectedEvent;

        /// <inheritdoc />
        public event EventHandler<DeviceRemovedEventArgs> DeviceRemovedEvent;

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
                eventEnqueuedEvent.WaitOne(Transport.TransportExceptionMonitor);
            }
        }

        /// <inheritdoc />
        public void Update()
        {
            CheckException();

            ProcessQueuedEvents(false);

            // Update each installed category if needed.
            foreach(var updateCategory in updateableCategories)
            {
                updateCategory.Update();
            }
        }

        #region Window Management

        /// <inheritdoc />
        public bool IsConnected { get; set; }

        /// <inheritdoc />
        public ulong CreateWindow(bool canHandleMld, Priority priority, List<long> windowHandles)
        {
            return CreateWindow(canHandleMld, priority, windowHandles, true);
        }

        /// <inheritdoc />
        public ulong CreateWindow(bool canHandleMld, Priority priority, List<long> windowHandles, bool multiTouchNativelySupported)
        {
            return windowManagementCategory.CreateWindow(canHandleMld, priority, windowHandles, multiTouchNativelySupported);
        }

        /// <inheritdoc />
        public void DestroyWindow(ulong windowId)
        {
            windowManagementCategory.DestroyWindowRequest(windowId);
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
            //TODO: Determine if status should be an argument.
            windowManagementCategory.RepositionWindowRequest(new Window
            {
                Status = true,
                Type = windowType,
                ViewportExtents = viewPortExtents,
                Viewports = viewports,
                WindowId = windowId
            });
        }

        /// <inheritdoc />
        public void RequestRepositionMultiWindows(IList<Window> windows)
        {
            windowManagementCategory.RepositionMultiWindowRequest(windows);
        }

        /// <inheritdoc />
        public MonitorComposition RequestMonitorConfiguration()
        {
            return GetMonitorCompositionUsingMonitorCategory();
        }

        /// <inheritdoc />
        public void SendWindowResizeComplete(ulong requestId)
        {
            windowManagementCategory.SizeRequestComplete(requestId);
        }

        /// <inheritdoc />
        public void SendMultiWindowResizeComplete(ulong requestId)
        {
            windowManagementCategory.MultiWindowSizeRequestComplete(requestId);
        }

        #endregion Window Management

        #region Sound

        /// <summary>
        /// Converts a volume attenuation value to a volume scale value.
        /// </summary>
        /// <param name="volumeAttenuation">The volume attenuation.</param>
        /// <returns>The volume scale.</returns>
        public static float ConvertVolumeLevel(uint volumeAttenuation)
        {
            return MaxVolumeScale - volumeAttenuation / MaxVolumeAttenuationLevel;
        }

        /// <inheritdoc />
        public float GetVolume(GroupName soundGroupName)
        {
            var volumeAttenuation = soundCategory.GetVolume(soundGroupName);
            return ConvertVolumeLevel(volumeAttenuation);
        }

        /// <inheritdoc />
        public bool IsMuteAll()
        {
            return soundCategory.IsMuteAll();
        }

        /// <inheritdoc />
        public Dictionary<GroupName, float> GetVolumeAll()
        {
            var volumeSettings = soundCategory.GetVolumeAll();
            return volumeSettings.ToDictionary(k => k.SoundGroup, e => ConvertVolumeLevel(e.VolumeLevel));
        }

        /// <inheritdoc />
        public VolumeSelectableInfo GetVolumePlayerSelectableInfo()
        {
            return soundCategory.GetVolumePlayerSelectableInfo();
        }

        /// <inheritdoc />
        public PlayerVolumeSettings GetPlayerVolumeSettings()
        {
            return soundCategory.GetPlayerVolumeSettings();
        }

        /// <inheritdoc />
        public bool SetPlayerVolumeInfo(PlayerVolumeInfo volumeInfo)
        {
            return soundCategory.SetPlayerVolumeInfo(volumeInfo);
        }

        #endregion Sound

        #region Button Panel

        /// <inheritdoc />
        public void SetAllPixels(ButtonIdentifier buttonId, bool pixelState, bool errorOnFailure = true)
        {
            if(buttonId == null)
            {
                throw new ArgumentNullException(nameof(buttonId));
            }

            buttonCategory.SetAllPixels(buttonId, pixelState, errorOnFailure);
        }

        /// <inheritdoc />
        public void SetLampState(IList<ButtonLampState> buttonLamps)
        {
            if(buttonLamps == null)
            {
                throw new ArgumentNullException(nameof(buttonLamps));
            }
            buttonCategory.SetLampStates(buttonLamps);
        }

        /// <inheritdoc />
        public void SetLampState(ButtonIdentifier lampId, bool lampState)
        {
            if(lampId == null)
            {
                throw new ArgumentNullException(nameof(lampId));
            }

            SetLampState(new List<ButtonLampState> { new ButtonLampState(lampId, lampState) });
        }

        /// <inheritdoc />
        public bool GetLampState(ButtonIdentifier buttonId, bool errorOnFailure = true)
        {
            var result = false;

            if(buttonId == null)
            {
                throw new ArgumentNullException(nameof(buttonId));
            }

            try
            {
                var lamps = new List<ButtonIdentifier> { buttonId };
                var lampStates = buttonCategory.GetLampState(lamps, errorOnFailure);

                if(lampStates != null && lampStates.Count > 0)
                {
                    result = lampStates[0].State;
                }
            }
            catch(Exception)
            {
                if(errorOnFailure)
                {
                    throw;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public int SendImageSet(string fileName, ButtonPanelLocation panelLocation, bool errorOnFailure = true)
        {
            return buttonCategory.DownloadImageSet(fileName, panelLocation, errorOnFailure);
        }

        /// <inheritdoc />
        public void RemoveImageSet(ushort imageSetId, ButtonPanelLocation panelLocation, bool errorOnFailure = true)
        {
            buttonCategory.RemoveImageSet(imageSetId, panelLocation, errorOnFailure);
        }

        /// <inheritdoc />
        public void PlayButtonAnimation(ButtonIdentifier buttonId, ushort imageSetId, ushort animationId, ushort frameDelay,
                                        bool repeat, bool transitionMode, bool errorOnFailure = true)
        {
            if(buttonId == null)
            {
                throw new ArgumentNullException(nameof(buttonId));
            }

            buttonCategory.PlayAnimation(animationId, buttonId, imageSetId, repeat, transitionMode, frameDelay, errorOnFailure);
        }

        /// <inheritdoc />
        public bool GetDeviceId(ButtonPanelLocation panelLocation, out string deviceId)
        {
            return buttonCategory.GetDeviceId(panelLocation, out deviceId);
        }

        /// <inheritdoc />
        public IList<IButtonPanelConfiguration> GetButtonPanelConfigurations(bool errorOnFailure = true)
        {
            IList<IButtonPanelConfiguration> result = null;

            try
            {
                var configurations = buttonCategory.GetButtonPanelConfiguration(errorOnFailure);
                result = configurations?.Select(configuration =>
                        configuration.ToButtonPanelConfiguration()).Cast<IButtonPanelConfiguration>().ToList();
            }
            catch(Exception)
            {
                if(errorOnFailure)
                {
                    throw;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public void LockSolenoid()
        {
            solenoidCategory.SetStateToLocked();
        }

        /// <inheritdoc />
        public void UnlockSolenoid()
        {
            solenoidCategory.SetStateToUnlocked();
        }

        /// <inheritdoc />
        public void ClickSolenoid()
        {
            solenoidCategory.ClickSolenoid();
        }

        /// <inheritdoc />
        public AttractAestheticConfiguration RequestAttractAestheticConfiguration()
        {
            return cabinetStatusCategory.RequestAttractAestheticConfiguration();
        }

        /// <inheritdoc />
        public bool RequestCabinetEventRegistration(bool registerForEvents)
        {
            return cabinetStatusCategory.RequestCabinetEventRegistration(registerForEvents);
        }

        #endregion Button Panel

        /// <inheritdoc />
        public MachineActivityStatus RequestActivityStatus()
        {
            var response = cabinetStatusCategory.RequestActivityStatus();
            return new MachineActivityStatus
            (
                response.Active,
                response.AttractInterval,
                response.InactivityDelay,
                response.GameAttractsEnabled,
                response.NewGame
            );
        }

        #region ResourceManagement

        /// <inheritdoc/>
        public AcquireDeviceResult RequestAcquireDevice(DeviceType deviceType, string deviceId, Priority priority)
        {
            var acquireDeviceResult = resourceManagementCategory.AcquireDevice(deviceType, deviceId, priority);

            if(acquireDeviceResult.Acquired && acquireDeviceResult.Reason == null)
            {
                if(deviceType == DeviceType.ButtonPanel)
                {
                    UpdateButtonPanelAcquisitionState(deviceId, true);
                    UpdateButtonPanelRegistration(deviceId);
                }
            }

            return acquireDeviceResult;
        }

        /// <inheritdoc/>
        public AcquireGroupsResult RequestAcquireGroups(DeviceType deviceType, string deviceId, Priority priority, List<uint> groupList)
        {
            return resourceManagementCategory.AcquireGroups(deviceType, deviceId, priority, groupList);
        }

        /// <inheritdoc/>
        public void ReleaseDevice(DeviceType deviceType, string deviceId = null)
        {
            // Since the button panel automatically requests event registration, the event
            // registration should be automatically removed if the client releases the device.
            if(deviceType == DeviceType.ButtonPanel)
            {
                UpdateButtonPanelAcquisitionState(deviceId, false);
                UpdateButtonPanelRegistration(deviceId);
            }

            resourceManagementCategory.ReleaseDevice(deviceType, deviceId);
        }

        /// <inheritdoc/>
        public void ReleaseGroups(DeviceType deviceType, string deviceId, List<uint> groupList)
        {
            resourceManagementCategory.ReleaseGroups(deviceType, deviceId, groupList);
        }

        /// <inheritdoc/>
        public bool DeviceAcquired(DeviceType deviceType, string deviceId)
        {
            return resourceManagementCategory.DeviceAcquired(deviceType, deviceId);
        }

        /// <inheritdoc/>
        public bool GroupAcquired(DeviceType deviceType, string deviceId, uint groupId)
        {
            return resourceManagementCategory.GroupAcquired(deviceType, deviceId, groupId);
        }

        /// <inheritdoc/>
        public IList<DeviceIdentifier> GetConnectedDevices()
        {
            return resourceManagementCategory.GetConnectedDevices();
        }

        /// <inheritdoc/>
        public IList<ConnectedDevice> GetConnectedDevicesWithGroups()
        {
            return resourceManagementCategory.GetConnectedDevicesWithGroups();
        }

        /// <inheritdoc/>
        public void RequestEventRegistration(DeviceType deviceType, string deviceId)
        {
            resourceManagementCategory.RequestEventRegistration(deviceId, deviceType);
        }

        /// <inheritdoc/>
        public void ReleaseEventRegistration(DeviceType deviceType, string deviceId)
        {
            resourceManagementCategory.ReleaseEventRegistration(deviceId, deviceType);
        }

        #endregion ResourceManagement

        #region Cabinet Credits

        /// <inheritdoc />
        public CabinetCreditDisplayType GetCreditDisplayType()
        {
            return cabinetStatusCategory.GetCreditDisplayType();
        }

        /// <inheritdoc />
        public void SetCreditDisplayType(CabinetCreditDisplayType creditDisplayType)
        {
            cabinetStatusCategory.SetCreditDisplayType(creditDisplayType);
        }

        #endregion Cabinet Credits

        /// <inheritdoc />
        public TInterface GetInterface<TInterface>() where TInterface : class
        {
            if(categoryMap.ContainsKey(typeof(TInterface)))
            {
                return categoryMap[typeof(TInterface)] as TInterface;
            }

            var implementation =
                (from category in installedCategories where category is TInterface select category).FirstOrDefault();

            //Add the implementation to the map even if it is null. This will allow the access to be from the dictionary
            //for subsequent requests.
            categoryMap[typeof(TInterface)] = implementation;

            return implementation as TInterface;
        }

        #endregion ICabinetLib Members

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
                if(Transport is IDisposable disposableTransport)
                {
                    disposableTransport.Dispose();
                }
                disposed = true;
            }
        }

        #endregion IDisposable Members
    }
}
