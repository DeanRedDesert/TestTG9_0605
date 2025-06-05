//-----------------------------------------------------------------------
// <copyright file = "ICabinetLib.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// CabinetLib interface containing functionality used by the client code
    /// </summary>
    public interface ICabinetLib
    {
        /// <summary>
        /// Event requesting a change in the size of our window.
        /// </summary>
        event EventHandler<WindowResizeEventArgs> WindowResizeEvent;

        /// <summary>
        /// Event requesting a change in the sizes of multi-windows.
        /// </summary>
        event EventHandler<MultiWindowResizeEventArgs> MultiWindowResizeEvent;

        /// <summary>
        /// Event requesting a change in the z order of our window.
        /// </summary>
        event EventHandler<WindowZOrderEventArgs> WindowZOrderEvent;

        /// <summary>
        /// Event indicating a change in the sound groups volume levels.
        /// </summary>
        event EventHandler<SoundVolumeChangedEventArgs> SoundVolumeChangedEvent;

        /// <summary>
        /// Event indicating a change in the sound volume Mute All status.
        /// </summary>
        event EventHandler<SoundVolumeMuteAllStatusChangedEventArgs> SoundVolumeMuteAllStatusChangedEvent;

        /// <summary>
        /// Event indicating a change in the sound volume Player Selectable status.
        /// </summary>
        event EventHandler<SoundVolumePlayerSelectableStatusChangedEventArgs> SoundVolumePlayerSelectableStatusChangedEvent;

        /// <summary>
        /// Event indicating a change in the player sound volume level.
        /// </summary>
        event EventHandler<SoundVolumePlayerLevelChangedEventArgs> SoundVolumePlayerLevelChangedEvent;

        /// <summary>
        /// Event which is fired when CSI Manager notifies the client that the state of the headphone jack has changed.
        /// </summary>
        event EventHandler<HeadphoneJackChangedEventArgs> HeadphoneJackChangedEvent;

        /// <summary>
        /// Event indicating that a cabinet button has been pressed.
        /// </summary>
        event EventHandler<CabinetButtonPressedEventArgs> ButtonPressedEvent;

        /// <summary>
        /// Event indicating that the cabinet status has changed.
        /// </summary>
        event EventHandler<ActivityStatusEventArgs> ActivityStatusEvent;

        /// <summary>
        /// Event indicating that the attract aesthetic configuration has changed.
        /// </summary>
        event EventHandler<AttractAestheticConfigurationEventArgs> AttractAestheticConfigurationChangedEvent;

        /// <summary>
        /// Event handler for non-game CSI clients invoked when a cabinet-specific event (such as a device connect/disconnect) occurs.
        /// This event is only invoked after CSI clients register for cabinet-specific events via <see cref="RequestCabinetEventRegistration"/>
        /// </summary>
        event EventHandler<CabinetEventArgs> CabinetEvent;

        /// <summary>
        /// Event indicating that a device has been acquired.
        /// </summary>
        event EventHandler<DeviceAcquiredEventArgs> DeviceAcquiredEvent;

        /// <summary>
        /// Event indicating that a device has been released.
        /// </summary>
        event EventHandler<DeviceReleasedEventArgs> DeviceReleasedEvent;

        /// <summary>
        /// Event indicating a device has been connected.
        /// </summary>
        event EventHandler<DeviceConnectedEventArgs> DeviceConnectedEvent;

        /// <summary>
        /// Event indicating a device has been removed.
        /// </summary>
        event EventHandler<DeviceRemovedEventArgs> DeviceRemovedEvent;

        /// <summary>
        /// Block until an event of the indicated type has been enqueued.
        /// </summary>
        /// <param name="eventType">Type of the event to wait for.</param>
        void WaitForEvent(Type eventType);

        /// <summary>
        /// The flag indicating whether the Cabinet Lib is connected to the CSI manager or not.
        /// Usually the Cabinet Lib is disconnected after the game is parked.
        /// </summary>
        bool IsConnected { get; set; }

        /// <summary>
        /// Connect to the socket previously set in the call to the constructor.
        /// Any connection failures will throw an exception and the caller should check
        /// for them.
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnect from the socket currently connected. Any disconnection failures
        /// will throw an exception and the caller should check for them.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Checks the event queue and fires the appropriate events
        /// </summary>
        void Update();

        /// <summary>
        /// Sets all of the pixels on a given button on or off.
        /// </summary>
        /// <param name="buttonId">The button identifier to set the pixels on.  0xFF represents all buttons.</param>
        /// <param name="pixelState">
        /// If true, all pixels will be turned on.  If false, all pixels will be turned off.
        /// </param>
        /// <param name="errorOnFailure">Flag indicating whether an error should be thrown if the command failed to execute on the Foundation. Defaults to true.</param>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when <paramref name="buttonId"/> is null.
        /// </exception>
        void SetAllPixels(ButtonIdentifier buttonId, bool pixelState, bool errorOnFailure = true);

        /// <summary>
        /// Request that the CSI manager create a window, with implication that
        /// the client supports the native multi-touch implementation.
        /// </summary>
        /// <param name="canHandleMld">Flag which indicates if the client can handle MLD (e.g., auto-white) on its own.</param>
        /// <param name="priority">The client's priority type.</param>
        /// <param name="windowHandles">Window handles associated with the window.</param>
        /// <returns>The window id.</returns>
        ulong CreateWindow(bool canHandleMld, Priority priority, List<long> windowHandles);

        /// <summary>
        /// Request that the CSI manager create a window.
        /// </summary>
        /// <param name="canHandleMld">
        /// Flag which indicates if the client can handle MLD (e.g., auto-white) on its own.
        /// </param>
        /// <param name="priority">
        /// The client's priority type.
        /// </param>
        /// <param name="windowHandles">
        /// Window handles associated with the window.
        /// </param>
        /// <param name="multiTouchNativelySupported">
        /// The flag indicating whether the client supports the native multi-touch implementation on all display devices,
        /// or if Foundation needs to interpret touches and generate Windows mouse events.
        /// </param>
        /// <returns>The window id.</returns>
        ulong CreateWindow(bool canHandleMld, Priority priority, List<long> windowHandles, bool multiTouchNativelySupported);

        /// <summary>
        /// Informs the CSI Manager that a window is requested to be destroyed.
        /// </summary>
        /// <param name="windowId">The ID of the window to destroy.</param>
        void DestroyWindow(ulong windowId);

        /// <summary>
        /// Request the CSI manager to reposition the window/viewports for a dockable window.
        /// </summary>
        /// <param name="windowId">The ID of the window to reposition/resize.</param>
        /// <param name="viewPortExtents">The extents of the viewports.</param>
        /// <param name="viewports">A list of viewports within the window.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the windowRect or viewports parameter is null.
        /// </exception>
        void RequestRepositionWindow(ulong windowId, DesktopRectangle viewPortExtents, ViewportList viewports);

        /// <summary>
        /// Request the CSI manager to reposition the window/viewports.
        /// </summary>
        /// <param name="windowId">The ID of the window to reposition/resize.</param>
        /// <param name="windowType">The type of window for viewport management purposes.</param>
        /// <param name="viewPortExtents">The extents of the viewports.</param>
        /// <param name="viewports">A list of viewports within the window.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either the windowRect or viewports parameter is null.
        /// </exception>
        void RequestRepositionWindow(ulong windowId, WindowType windowType, DesktopRectangle viewPortExtents, ViewportList viewports);

        /// <summary>
        /// Request the CSI manager to change the positions of multiple client windows.
        /// </summary>
        /// <param name="windows">Information about the positions of multiple windows.</param>
        void RequestRepositionMultiWindows(IList<Window> windows);

        /// <summary>
        /// Request the monitor configuration from the CSI manager.
        /// </summary>
        /// <returns>The monitor configuration.</returns>
        MonitorComposition RequestMonitorConfiguration();

        /// <summary>
        /// Notifies the CSI manager that the window has finished resizing.
        /// </summary>
        /// <param name="requestId">The ID of the request to send the complete message for.</param>
        void SendWindowResizeComplete(ulong requestId);

        /// <summary>
        /// Notifies the CSI manager that multiple windows have finished resizing.
        /// </summary>
        /// <param name="requestId">The ID of the request to send the complete message for.</param>
        void SendMultiWindowResizeComplete(ulong requestId);

        /// <summary>
        /// Request for acquiring a control of a device.
        /// </summary>
        /// <param name="deviceType">The type of the device to acquire.</param>
        /// <param name="deviceId">The id of the device to acquire.</param>
        /// <param name="priority">The priority of the requesting client.</param>
        /// <returns>An AcquireDeviceResult indicating if the device was acquired and if not why.</returns>
        AcquireDeviceResult RequestAcquireDevice(DeviceType deviceType, string deviceId, Priority priority);

        /// <summary>
        /// Request for acquiring a control of a device.
        /// </summary>
        /// <param name="deviceType">The type of the device to which the groups belong.</param>
        /// <param name="deviceId">The id of the device.</param>
        /// <param name="priority">The priority of the requesting client.</param>
        /// <param name="groups">The groups of a device that need to be acquired.</param>
        /// <returns>An AcquireDeviceResult indicating if the groups were acquired and if not why.</returns>
        AcquireGroupsResult RequestAcquireGroups(DeviceType deviceType, string deviceId, Priority priority, List<uint> groups);

        /// <summary>
        /// Release the specified device.
        /// </summary>
        /// <param name="deviceType">The device to release.</param>
        /// <param name="deviceId">The device ID of the device to release.</param>
        void ReleaseDevice(DeviceType deviceType, string deviceId);

        /// <summary>
        /// Release the specified device.
        /// </summary>
        /// <param name="deviceType">The device to release.</param>
        /// <param name="deviceId">The device ID of the device to release.</param>
        /// <param name="groupList">The groups of a device that need to be released.</param>
        void ReleaseGroups(DeviceType deviceType, string deviceId, List<uint> groupList);

        /// <summary>
        /// Returns true if specified device is acquired.
        /// </summary>
        /// <param name="deviceType">Device to check for having been acquired.</param>
        /// <param name="deviceId">The ID of the device to check if it has been acquired.</param>
        /// <returns>True if the device has been acquired.</returns>
        bool DeviceAcquired(DeviceType deviceType, string deviceId);

        /// <summary>
        /// Returns true if specified group is acquired.
        /// </summary>
        /// <param name="deviceType">The type of device to check for having been acquired.</param>
        /// <param name="deviceId">The ID of the device to which the group belongs.</param>
        /// <param name="groupId">The ID of the group to check if it has been acquired.</param>
        /// <returns>True if the device has been acquired.</returns>
        bool GroupAcquired(DeviceType deviceType, string deviceId, uint groupId);

        /// <summary>
        /// Get a list of the connected devices.
        /// </summary>
        /// <returns>List of all the currently connected devices.</returns>
        IList<DeviceIdentifier> GetConnectedDevices();

        /// <summary>
        /// Get a list of the connected devices alongwith the groups.
        /// </summary>
        /// <returns>List of all currently connected devices with groups.</returns>
        IList<ConnectedDevice> GetConnectedDevicesWithGroups();

        /// <summary>
        /// Convert the device Id from a panel location.
        /// </summary>
        /// <param name="panelLocation">Location of the panel.</param>
        /// <param name="deviceId">The device Id retrieved.</param>
        /// <returns>True if the conversion succeeds. Otherwise, false.</returns>
        bool GetDeviceId(ButtonPanelLocation panelLocation, out string deviceId);

        /// <summary>
        /// Requests event registration for a given device.
        /// </summary>
        /// <param name="deviceType">The device type.</param>
        /// <param name="deviceId">The ID of the device to request event registration for.</param>
        /// <exception cref="ResourceManagementCategoryException">
        /// Thrown if the device doesn't support event registration.
        /// </exception>
        void RequestEventRegistration(DeviceType deviceType, string deviceId);

        /// <summary>
        /// Releases the event registration of a device.
        /// </summary>
        /// <param name="deviceType">The device type.</param>
        /// <param name="deviceId">The ID of the device to release event registration on.</param>
        void ReleaseEventRegistration(DeviceType deviceType, string deviceId);

        /// <summary>
        /// Sets the state of all lamps.  They can be a mix of on and off.
        /// </summary>
        /// <param name="buttonLamps">
        /// The list of states to use to set ALL lamps.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when <paramref name="buttonLamps"/> is null.
        /// </exception>
        void SetLampState(IList<ButtonLampState> buttonLamps);

        /// <summary>
        /// Turns a given lamp on/off.
        /// </summary>
        /// <param name="lampId">The button identifier of the lamp to turn on/off.</param>
        /// <param name="lampState">True to turn the lamp on.  False to turn it off.</param>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when <paramref name="lampId"/> is null.
        /// </exception>
        void SetLampState(ButtonIdentifier lampId, bool lampState);

        /// <summary>
        /// Get the lamp state of the requested button.
        /// </summary>
        /// <param name="buttonId">The button identifier to check lamp state of.</param>
        /// <param name="errorOnFailure">Flag indicating whether an error should be thrown if the command failed to execute on the Foundation. Defaults to true.</param>
        /// <returns>True if the lamp is lit and false otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when <paramref name="buttonId"/> is null.
        /// </exception>
        bool GetLampState(ButtonIdentifier buttonId, bool errorOnFailure = true);

        /// <summary>
        /// Sends an image set to the dynamic buttons.
        /// </summary>
        /// <param name="fileName">The full path to the image set file to load.</param>
        /// <param name="panelLocation">The location of the button panel.</param>
        /// <param name="errorOnFailure">Flag indicating whether an error should be thrown if the command failed to execute on the Foundation. Defaults to true.</param>
        /// <returns>The image set id.</returns>
        int SendImageSet(string fileName, ButtonPanelLocation panelLocation, bool errorOnFailure = true);

        /// <summary>
        /// Removes the given image set from the dynamic buttons.
        /// </summary>
        /// <param name="imageSetId">The ID of the image set to remove.</param>
        /// <param name="panelLocation">The location of the button panel.</param>
        /// <param name="errorOnFailure">Flag indicating whether an error should be thrown if the command failed to execute on the Foundation. Defaults to true.</param>
        void RemoveImageSet(ushort imageSetId, ButtonPanelLocation panelLocation, bool errorOnFailure = true);

        /// <summary>
        /// Plays an animation on a dynamic button.
        /// </summary>
        /// <param name="buttonId">The identifier of the button to play the animation on.</param>
        /// <param name="imageSetId">The ID of the image set to use for the animation.</param>
        /// <param name="animationId">The ID of the animation to use.</param>
        /// <param name="frameDelay">The frame delay of the animation, in milliseconds.</param>
        /// <param name="repeat">Flag to set repeat on or off</param>
        /// <param name="transitionMode">Flag to set transition as true or false.</param>
        /// <param name="errorOnFailure">Flag indicating whether an error should be thrown if the command failed to execute on the Foundation. Defaults to true.</param>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when <paramref name="buttonId"/> is null.
        /// </exception>
        void PlayButtonAnimation(ButtonIdentifier buttonId, ushort imageSetId, ushort animationId, ushort frameDelay, bool repeat, bool transitionMode, bool errorOnFailure = true);

        /// <summary>
        /// Gets the volume level given a sound group name.
        /// </summary>
        /// <param name="soundGroupName">The name of the sound group.</param>
        /// <returns>The volume gain as a float between 0 and 1</returns>
        /// <remarks>The volume level is provided as an attenuation level between 0 and 10000.
        /// We map this range to a volume scale between 0 and 1</remarks>
        float GetVolume(GroupName soundGroupName);

        /// <summary>
        /// Gets the volume level of each sound group.
        /// </summary>
        /// <returns>The volume gain as a float between 0 and 1 for each sound group</returns>
        /// <remarks>The volume level is provided as an attenuation level between 0 and 10000.
        /// We map this range to a volume scale between 0 and 1</remarks>
        Dictionary<GroupName, float> GetVolumeAll();

        /// <summary>
        /// Gets the Mute All status.
        /// </summary>
        /// <returns>The Mute All status.</returns>
        bool IsMuteAll();

        /// <summary>
        /// Gets <see cref="VolumeSelectableInfo"/> from Foundation.
        /// </summary>
        /// <returns>The <see cref="VolumeSelectableInfo"/> which contains Player Volume Selectable and Player Mute Selectable.</returns>
        VolumeSelectableInfo GetVolumePlayerSelectableInfo();

        /// <summary>
        /// Gets the <see cref="PlayerVolumeSettings"/> from Foundation.
        /// </summary>
        /// <returns>The player volume settings.</returns>
        PlayerVolumeSettings GetPlayerVolumeSettings();

        /// <summary>
        /// Sets <see cref="PlayerVolumeInfo"/> to Foundation.
        /// </summary>
        /// <param name="volumeInfo">A <see cref="PlayerVolumeInfo"/> that include the player volume level and mute.</param>
        /// <returns>True if set succeeded.</returns>
        bool SetPlayerVolumeInfo(PlayerVolumeInfo volumeInfo);

        /// <summary>
        ///  Gets the configurations of the button panels.
        /// </summary>
        /// <param name="errorOnFailure">Flag indicating whether an error should be thrown if the command failed to execute on the Foundation. Defaults to true.</param>
        /// <returns>The configurations of the button panel or null on failure.</returns>
        /// <remarks>
        /// This is not implemented as a property because it would violate the performance expectations of a property.
        /// </remarks>
        IList<IButtonPanelConfiguration> GetButtonPanelConfigurations(bool errorOnFailure = true);

        /// <summary>
        /// Sets the handle solenoid to locked.
        /// </summary>
        void LockSolenoid();

        /// <summary>
        /// Sets the handle solenoid to unlocked.
        /// </summary>
        void UnlockSolenoid();

        /// <summary>
        /// Clicks the handle solenoid.
        /// </summary>
        void ClickSolenoid();

        /// <summary>
        /// Requests the current attract aesthetic configuration.
        /// </summary>
        /// <returns>The current configuration.</returns>
        AttractAestheticConfiguration RequestAttractAestheticConfiguration();

        /// <summary>
        /// Requests that the current CSI client register or unregister for cabinet-specific events,
        /// such as device connects/disconnects.
        /// </summary>
        /// <param name="registerForEvents">
        /// True to register for events, false to unregister for events.
        /// </param>
        /// <returns>
        /// True if the client successfully registered/unregistered for cabinet events.
        /// </returns>
        /// <remarks>
        /// Only non-game CSI clients are allowed to register for cabinet events.
        /// </remarks>
        bool RequestCabinetEventRegistration(bool registerForEvents);

        /// <summary>
        /// Request the machine activity status.
        /// </summary>
        /// <returns>The machine activity status data.</returns>
        MachineActivityStatus RequestActivityStatus();

        /// <summary>
        /// Attempt to get the cabinet component with the specified interface. This function should not be called until
        /// after a CSI connection has been established.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface to get.</typeparam>
        /// <returns>The interface if available, otherwise null.</returns>
        TInterface GetInterface<TInterface>() where TInterface : class;

        /// <summary>
        /// Gets the credit display type.
        /// </summary>
        /// <returns>The credit display type.</returns>
        CabinetCreditDisplayType GetCreditDisplayType();

        /// <summary>
        /// Sets the credit display type.
        /// </summary>
        /// <param name="creditDisplayType">The credit display type.</param>
        void SetCreditDisplayType(CabinetCreditDisplayType creditDisplayType);
    }
}
