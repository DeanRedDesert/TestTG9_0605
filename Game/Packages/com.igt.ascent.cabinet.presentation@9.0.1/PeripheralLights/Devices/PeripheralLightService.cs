// -----------------------------------------------------------------------
// <copyright file = "PeripheralLightService.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CabinetServices;
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas;

    /// <summary>
    /// The cabinet service that manages the interface objects to various USB peripheral lights.
    /// </summary>
    /// <remarks>
    /// A given piece of peripheral light hardware is only acquired after it has been requested once.
    /// There is only the initialization overhead of the first request for a light peripheral.
    /// Afterwards all subsequent requests return the same instance for a given light peripheral.
    /// </remarks>
    /// <devdoc>
    /// This service inherits from CabinetServiceBase rather than DeviceServiceBase because:
    /// 1. This service handles two DeviceTypes (Light and StreamingLight) rather one.
    /// 2. LightManagerBase implements most functionality of DeviceServiceBase already.
    /// 3. There is this special concept of FreeLightObjects implemented by LightManagerBase.
    /// </devdoc>
    public sealed class PeripheralLightService : CabinetServiceBase, IPeripheralLightService
    {
        #region Private Fields

        /// <summary>
        /// The path where the client executable is installed.
        /// </summary>
        private readonly string mountPoint;

        /// <summary>
        /// Priority of the client.
        /// </summary>
        private readonly Priority clientPriority;

        /// <summary>
        /// The object managing the peripheral light devices.
        /// </summary>
        private readonly PeripheralLightManager peripheralLightManager =
                                    new PeripheralLightManager(HardwareSpecs.GetPeripheralLightSpecs());

        /// <summary>
        /// The object managing the streaming light devices.
        /// </summary>
        private readonly StreamingLightManager streamingLightManager =
                                    new StreamingLightManager(HardwareSpecs.GetStreamingLightSpecs());

        /// <summary>
        /// The interface to the streaming lights provided by the cabinet.
        /// </summary>
        private IStreamingLights streamingLightsInterface;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a service with a specified priority.
        /// </summary>
        /// <param name="mountPoint">Directory containing the game executable.</param>
        /// <param name="clientPriority">CSI client priority for accessing devices.</param>
        public PeripheralLightService(string mountPoint, Priority clientPriority)
        {
            if(mountPoint == null)
            {
                throw new ArgumentNullException(nameof(mountPoint));
            }

            this.mountPoint = mountPoint.Replace('/', Path.DirectorySeparatorChar);
            this.clientPriority = clientPriority;
        }

        #endregion

        #region CabinetServiceBase Overrides

        /// <inheritdoc />
        protected override void OnAsyncConnect()
        {
            base.OnAsyncConnect();

            Streaming.LightSequence.SetGameMountPoint(mountPoint);

            // Gets the peripheral lights interface.
            var peripheralLightsInterface = CabinetLib.GetInterface<IPeripheralLights>();
            if(peripheralLightsInterface != null)
            {
                peripheralLightsInterface = new PeripheralLightsRecoveryDecorator(peripheralLightsInterface,
                                                                                  peripheralLightManager);
            }

            // Gets the streaming lights interface.
            streamingLightsInterface = CabinetLib.GetInterface<IStreamingLights>();

            UnsubscribeEventHandlers();
            SubscribeEventHandlers();

            peripheralLightManager.SetLightInterface(CabinetLib, clientPriority, peripheralLightsInterface);
            streamingLightManager.SetLightInterface(CabinetLib, clientPriority, streamingLightsInterface);

            UsbButtonEdgeLight.SetButtonPanelConfiguration(CabinetLib);
        }

        /// <inheritdoc />
        public override void Disconnect()
        {
            UnsubscribeEventHandlers();

            peripheralLightManager.RemoveLightInterface();
            streamingLightManager.RemoveLightInterface();

            streamingLightsInterface = null;
            base.Disconnect();
        }

        #endregion

        #region IPeripheralLightService Members

        #region Events

        /// <inheritdoc />
        public event Action<PeripheralLightDeviceEventArgs> PeripheralLightDeviceAcquired;

        /// <inheritdoc />
        public event Action<PeripheralLightDeviceEventArgs> PeripheralLightDeviceReleased;

        /// <inheritdoc />
        public event Action<PeripheralLightDeviceEventArgs> PeripheralLightDeviceRemoved;

        #endregion

        #region Release Lights Functions

        /// <inheritdoc />
        public void ReleaseLights()
        {
            peripheralLightManager.ReleaseLights();
            streamingLightManager.ReleaseLights();
        }

        #endregion

        #region Get Light Functions

        /// <inheritdoc />
        public UsbTopperLight GetPeripheralLight(TopperHardware hardware)
        {
            return GetPeripheralLight<UsbTopperLight>((Hardware)hardware);
        }

        /// <inheritdoc />
        public UsbHaloLight GetPeripheralLight(HaloHardware hardware)
        {
            return GetPeripheralLight<UsbHaloLight>((Hardware)hardware);
        }

        /// <inheritdoc />
        public UsbButtonEdgeLight GetPeripheralLight(ButtonHardware hardware)
        {
            return GetPeripheralLight<UsbButtonEdgeLight>((Hardware)hardware);
        }

        /// <inheritdoc />
        public UsbLightBars GetPeripheralLight(LightBarHardware hardware)
        {
            var light = GetPeripheralLight<UsbLightBars>((Hardware)hardware);
            light.GameMountPoint = mountPoint;
            return light;
        }

        /// <inheritdoc />
        public UsbFacadeLight GetPeripheralLight(FacadeHardware hardware)
        {
            return GetPeripheralLight<UsbFacadeLight>((Hardware)hardware);
        }

        /// <inheritdoc />
        public UsbIndividualLightControl GetPeripheralLight(LegacyReelLightHardware hardware)
        {
            switch(hardware)
            {
                case LegacyReelLightHardware.FrontLights:
                    return GetPeripheralLight<UsbReelFrontLight>((Hardware)hardware);

                case LegacyReelLightHardware.BackLights:
                    return GetPeripheralLight<UsbReelBackLight>((Hardware)hardware);

                default:
                    return null;
            }
        }

        /// <inheritdoc />
        public UsbCrystalCoreLight GetPeripheralLight(CrystalCoreLightHardware hardware)
        {
            return GetPeripheralLight<UsbCrystalCoreLight>((Hardware)hardware);
        }

        /// <inheritdoc />
        public UsbStreamingLight GetPeripheralLight(StreamingLightHardware hardware, bool allowCoupling = false)
        {
            if(hardware == StreamingLightHardware.Unknown)
            {
                throw new ArgumentException("The unknown device is not a valid hardware device.", nameof(hardware));
            }

            var light = GetPeripheralLight<UsbStreamingLight>((Hardware)hardware, allowCoupling);
            light.GameMountPoint = mountPoint;

            return light;
        }

        /// <inheritdoc />
        public UsbSymbolHighlightSupportedStreamingLight GetSymbolHighlightSupportedStreamingLight(
            SymbolHighlightSupportedStreamingLightHardware hardware)
        {
            if(hardware == SymbolHighlightSupportedStreamingLightHardware.Unknown)
            {
                throw new ArgumentException("The unknown device is not a valid hardware device.", nameof(hardware));
            }

            var light = GetPeripheralLight<UsbSymbolHighlightSupportedStreamingLight>((Hardware)hardware);

            light.GameMountPoint = mountPoint;

            return light;
        }

        /// <inheritdoc />
        public UsbHandleLight GetPeripheralLight(HandleLightHardware hardware)
        {
            return GetPeripheralLight<UsbHandleLight>((Hardware)hardware);
        }

        /// <inheritdoc />
        public UsbNestedWheelLight GetPeripheralLight(NestedwheelsLightHardware hardware)
        {
            return GetPeripheralLight<UsbNestedWheelLight>((Hardware)hardware);
        }

        /// <inheritdoc />
        public TwilightZone3DMonitorBezel GetPeripheralLight(TwilightZone3DMonitorBezelHardware hardware)
        {
            return GetPeripheralLight<TwilightZone3DMonitorBezel>((Hardware)hardware);
        }

        /// <inheritdoc />
        public UsbLegacyBacklight GetPeripheralLight(LegacyBacklightHardware hardware)
        {
            return GetPeripheralLight<UsbLegacyBacklight>((Hardware)hardware);
        }

        #endregion

        #region Device Intensity Functions

        /// <inheritdoc />
        public void SetLightIntensity(byte intensity)
        {
            streamingLightManager.SetLightIntensity(intensity);
        }

        /// <inheritdoc />
        public byte GetLightIntensity()
        {
            return streamingLightManager.GetLightIntensity();
        }

        #endregion

        #region Universal Color Functions

        /// <inheritdoc />
        public void SetUniversalColor(Color color)
        {
            var blankLightObjects = peripheralLightManager.GetBlankLightObjects()
                                   .Union(streamingLightManager.GetBlankLightObjects());

            foreach(var device in blankLightObjects)
            {
                device.SetUniversalColor(color);
            }
        }

        #endregion

        #region Enable and Disable

        /// <inheritdoc />
        public bool EnableStreamingLight(StreamingLightHardware deviceType)
        {
            if(deviceType == StreamingLightHardware.Unknown)
            {
                throw new ArgumentException("The unknown device is not a valid hardware device.", nameof(deviceType));
            }

            return streamingLightManager.UpdateStreamingLightEnabledFlag((Hardware)deviceType, true);
        }

        /// <inheritdoc />
        public bool DisableStreamingLight(StreamingLightHardware deviceType)
        {
            if(deviceType == StreamingLightHardware.Unknown)
            {
                throw new ArgumentException("The unknown device is not a valid hardware device.", nameof(deviceType));
            }

            return streamingLightManager.UpdateStreamingLightEnabledFlag((Hardware)deviceType, false);
        }

        /// <inheritdoc />
        public void ClearStreamingLights()
        {
            streamingLightManager.ClearStreamingLights();
        }

        #endregion

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the interface for querying the status of streaming light devices.
        /// </summary>
        /// <returns></returns>
        internal ILightDeviceInquiry GetStreamingLightDeviceInquiry()
        {
            return streamingLightManager;
        }

        #endregion

        #region CabinetLib Event Handlers

        private void SubscribeEventHandlers()
        {
            if(CabinetLib != null)
            {
                CabinetLib.DeviceAcquiredEvent += CabinetLibDeviceAcquiredEvent;
                CabinetLib.DeviceConnectedEvent += CabinetLibDeviceConnectedEvent;
                CabinetLib.DeviceReleasedEvent += CabinetLibDeviceReleasedEvent;
                CabinetLib.DeviceRemovedEvent += CabinetLibDeviceRemovedEvent;
            }

            if(streamingLightsInterface != null)
            {
                streamingLightsInterface.NotificationEvent += HandleStreamingLightNotificationEvent;
            }
        }

        private void UnsubscribeEventHandlers()
        {
            if(CabinetLib != null)
            {
                CabinetLib.DeviceAcquiredEvent -= CabinetLibDeviceAcquiredEvent;
                CabinetLib.DeviceConnectedEvent -= CabinetLibDeviceConnectedEvent;
                CabinetLib.DeviceReleasedEvent -= CabinetLibDeviceReleasedEvent;
                CabinetLib.DeviceRemovedEvent -= CabinetLibDeviceRemovedEvent;
            }

            if(streamingLightsInterface != null)
            {
                streamingLightsInterface.NotificationEvent -= HandleStreamingLightNotificationEvent;
            }
        }

        /// <summary>
        /// Called when a device is acquired by the CSI client.
        /// </summary>
        /// <param name="sender">The cabinet communications library for the CSI communications link.</param>
        /// <param name="eventArgs">The event information.</param>
        private void CabinetLibDeviceAcquiredEvent(object sender, DeviceAcquiredEventArgs eventArgs)
        {
            UsbLightBase device = null;

            switch(eventArgs.DeviceName)
            {
                case DeviceType.Light:
                    device = peripheralLightManager.HandleDeviceAcquired(eventArgs.DeviceId);
                    break;

                case DeviceType.StreamingLight:
                    device = streamingLightManager.HandleDeviceAcquired(eventArgs.DeviceId);
                    break;

                case DeviceType.ButtonPanel:
                    UsbButtonEdgeLight.SetButtonPanelConfiguration(CabinetLib);
                    break;
            }

            if(device != null)
            {
                // Ensure the panel configuration is set before trying to do the recovery or firing the connected event otherwise
                // the commands won't do anything because the available button list will be empty.
                if(!UsbButtonEdgeLight.ButtonPanelConfigurationSet &&
                   device is UsbButtonEdgeLight)
                {
                        UsbButtonEdgeLight.SetButtonPanelConfiguration(CabinetLib);
                }

                // Send off the event for handling by the process.
                OnPeripheralLightsAcquired(new PeripheralLightDeviceEventArgs(new List<UsbLightBase> { device }));
            }
        }

        /// <summary>
        /// Called when a device is plugged into the cabinet.
        /// </summary>
        /// <param name="sender">The cabinet communications library for the CSI communications link.</param>
        /// <param name="eventArgs">The event information.</param>
        private void CabinetLibDeviceConnectedEvent(object sender, DeviceConnectedEventArgs eventArgs)
        {
            UsbLightBase device = null;

            switch(eventArgs.DeviceName)
            {
                case DeviceType.Light:
                    device = peripheralLightManager.HandleDeviceConnected(eventArgs.DeviceId);
                    break;

                case DeviceType.StreamingLight:
                    device = streamingLightManager.HandleDeviceConnected(eventArgs.DeviceId);
                    break;

                case DeviceType.ButtonPanel:
                    // If the button panel was reconnected the configuration could have changed due to the Foundation
                    // switching the VBP on and off, or a different dynamic button panel being plugged in.
                    UsbButtonEdgeLight.SetButtonPanelConfiguration(CabinetLib);
                    break;
            }

            if(device != null)
            {
                // Ensure the panel configuration is set before trying to do the recovery or firing the connected event otherwise
                // the commands won't do anything because the available button list will be empty.
                if(!UsbButtonEdgeLight.ButtonPanelConfigurationSet &&
                   device is UsbButtonEdgeLight)
                {
                    UsbButtonEdgeLight.SetButtonPanelConfiguration(CabinetLib);
                }

                // Send off the event for handling by the process.
                OnPeripheralLightsAcquired(new PeripheralLightDeviceEventArgs(new List<UsbLightBase> { device }));
            }
        }

        /// <summary>
        /// Called when a device has been released by the CSI manager.
        /// </summary>
        /// <param name="sender">The cabinet communications library for the CSI communications link.</param>
        /// <param name="eventArgs">The event information.</param>
        private void CabinetLibDeviceReleasedEvent(object sender, DeviceReleasedEventArgs eventArgs)
        {
            UsbLightBase device = null;

            switch(eventArgs.DeviceName)
            {
                case DeviceType.Light:
                    device = peripheralLightManager.HandleDeviceReleased(eventArgs.DeviceId, eventArgs.Reason);
                    break;

                case DeviceType.StreamingLight:
                    device = streamingLightManager.HandleDeviceReleased(eventArgs.DeviceId, eventArgs.Reason);
                    break;
            }

            if(device != null)
            {
                OnPeripheralLightsReleased(new PeripheralLightDeviceEventArgs(new List<UsbLightBase> { device }));
            }
        }

        /// <summary>
        /// Called when a device has been removed from the cabinet.
        /// </summary>
        /// <param name="sender">The cabinet communications library for the CSI communications link.</param>
        /// <param name="eventArgs">The event information.</param>
        private void CabinetLibDeviceRemovedEvent(object sender, DeviceRemovedEventArgs eventArgs)
        {
            UsbLightBase device = null;

            switch(eventArgs.DeviceName)
            {
                case DeviceType.Light:
                    device = peripheralLightManager.HandleDeviceRemoved(eventArgs.DeviceId);
                    break;

                case DeviceType.StreamingLight:
                    device = streamingLightManager.HandleDeviceRemoved(eventArgs.DeviceId);
                    break;

                case DeviceType.ButtonPanel:
                    UsbButtonEdgeLight.ClearButtonPanelConfiguration();
                    break;
            }

            if(device != null)
            {
                OnPeripheralLightsRemoved(new PeripheralLightDeviceEventArgs(new List<UsbLightBase> { device }));
            }

        }

        /// <summary>
        /// Handles streaming light notification events.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="eventArgs">The event argument information.</param>
        private void HandleStreamingLightNotificationEvent(object sender,
                                                                  StreamingLightsNotificationEventArgs eventArgs)
        {
            streamingLightManager.HandleStreamingLightNotification(eventArgs);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets an instance of the peripheral lights hardware for the given type.
        /// </summary>
        /// <typeparam name="TLightDevice">The data type for controlling the hardware.</typeparam>
        /// <param name="hardware">The specific piece of hardware requested.</param>
        /// <param name="allowCoupling">Specifies that a game client can use multiple interfaces to request a single coupled light device.</param>
        /// <returns>The instance for the lights.  Null if the service is not connected.</returns>
        private TLightDevice GetPeripheralLight<TLightDevice>(Hardware hardware, bool allowCoupling = false) where TLightDevice : UsbLightBase
        {
            VerifyCabinetIsConnected();

            var lightObject = GetLightObject(hardware, typeof(TLightDevice), allowCoupling);

            if(!lightObject.DeviceAcquired)
            {
                // If the given hardware type was not acquired, and the light is for a topper,
                // attempt to obtain the other topper light type.
                var alternativeHardware = GetAlternativeHardware(hardware);
                if(alternativeHardware != hardware)
                {
                    lightObject = GetLightObject(alternativeHardware, typeof(TLightDevice), allowCoupling);
                }
            }

            return lightObject as TLightDevice;
        }

        /// <summary>
        /// Gets a light device of the specific hardware and light types.
        /// </summary>
        /// <param name="hardware">The hardware type of the light device to get.</param>
        /// <param name="lightType">The expected type of the light device to return.</param>
        /// <param name="allowCoupling">Specifies that a game client can use multiple interfaces to request a single coupled light device.</param>
        /// <returns>
        /// The light device object of <paramref name="lightType"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="hardware"/> is unknown to the light managers.
        /// </exception>
        private UsbLightBase GetLightObject(Hardware hardware, Type lightType, bool allowCoupling)
        {
            UsbLightBase lightObject;

            if(peripheralLightManager.Supports(hardware))
            {
                lightObject = peripheralLightManager.GetLightObject(hardware, lightType, allowCoupling);
            }
            else if(streamingLightManager.Supports(hardware))
            {
                lightObject = streamingLightManager.GetLightObject(hardware, lightType, allowCoupling);
            }
            else
            {
                throw new ArgumentException($"Unknown hardware type {hardware}.", nameof(hardware));
            }

            return lightObject;
        }

        /// <summary>
        /// Gets the hardware type that can substitutes for another hardware type.
        /// </summary>
        /// <param name="hardware">The original hardware type.</param>
        /// <returns>
        /// The substitute hardware type for <paramref name="hardware"/>.
        /// If no substitute is found, return the original value.
        /// </returns>
        private Hardware GetAlternativeHardware(Hardware hardware)
        {
            var result = hardware;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch(hardware)
            {
                case Hardware.GenericBacklitTopper:
                    result = Hardware.VideoTopper;
                    break;

                case Hardware.VideoTopper:
                    result = Hardware.GenericBacklitTopper;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Handle a peripheral light acquired event.
        /// </summary>
        /// <param name="peripheralLightDeviceEvent">List of peripheral devices that have been acquired.</param>
        private void OnPeripheralLightsAcquired(PeripheralLightDeviceEventArgs peripheralLightDeviceEvent)
        {
            PeripheralLightDeviceAcquired?.Invoke(peripheralLightDeviceEvent);
        }

        /// <summary>
        /// Handle a peripheral light released event.
        /// </summary>
        /// <param name="peripheralLightDeviceEvent">List of peripheral devices that have been released.</param>
        private void OnPeripheralLightsReleased(PeripheralLightDeviceEventArgs peripheralLightDeviceEvent)
        {
            PeripheralLightDeviceReleased?.Invoke(peripheralLightDeviceEvent);
        }

        /// <summary>
        /// Handle a peripheral light removed event.
        /// </summary>
        /// <param name="peripheralLightDeviceEvent">List of peripheral devices that have been removed.</param>
        private void OnPeripheralLightsRemoved(PeripheralLightDeviceEventArgs peripheralLightDeviceEvent)
        {
            PeripheralLightDeviceRemoved?.Invoke(peripheralLightDeviceEvent);
        }

        #endregion
    }
}
