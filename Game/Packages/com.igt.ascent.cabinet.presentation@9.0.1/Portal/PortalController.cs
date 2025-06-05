//-----------------------------------------------------------------------
// <copyright file = "PortalController.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Portal
{
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas;
    using System;
    using System.Linq;

    /// <summary>
    /// Controller for the portal category.
    /// </summary>
    public static class PortalController
    {
        /// <summary>
        /// References the cabinet library in use.
        /// </summary>
        private static ICabinetLib cabinetLib;

        /// <summary>
        /// Reference the portal category in use.
        /// </summary>
        private static IPortal currentPortalCategory;

        /// <summary>
        /// Flag to check if acquisition of the portal class has been requested.
        /// </summary>
        private static bool portalClassRequested;

        /// <summary>
        /// Identifier for the portal class that was requested.
        /// </summary>
        private static string portalClassIdentifier;

        /// <summary>
        /// The <see cref="Priority"/> of the client that is using the portal category.
        /// </summary>
        private static Priority clientPriority;

        /// <summary>
        /// Flag to check if the portal class is acquired.
        /// </summary>
        public static bool PortalClassAcquired { get; private set; }

        /// <summary>
        /// Flag to check if the portal class is connected.
        /// </summary>
        public static bool PortalClassConnected { get; private set; }

        /// <summary>
        /// Event triggered when the portal class is acquired.
        /// </summary>
        public static event EventHandler<DeviceAcquiredEventArgs> PortalClassAcquiredEvent;

        /// <summary>
        /// Event triggered when the portal class is released.
        /// </summary>
        public static event EventHandler<DeviceReleasedEventArgs> PortalClassReleasedEvent;

        /// <summary>
        /// Event triggered when the portal class is connected.
        /// </summary>
        public static event EventHandler<DeviceConnectedEventArgs> PortalClassConnectedEvent;

        /// <summary>
        /// Event triggered when the portal class is removed.
        /// </summary>
        public static event EventHandler<DeviceRemovedEventArgs> PortalClassRemovedEvent;

        /// <summary>
        /// Sets the cabinet library instance.
        /// </summary>
        /// <param name="cabinetLibrary">The cabinet library in use.</param>
        /// <param name="priority">The priority of the client.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any of the input parameters are null.
        /// </exception>
        public static void SetCabinetLibrary(ICabinetLib cabinetLibrary, Priority priority)
        {
            cabinetLib = cabinetLibrary ?? throw new ArgumentNullException(nameof(cabinetLibrary));
            currentPortalCategory = cabinetLibrary.GetInterface<IPortal>();
            clientPriority = priority;
            ClearExistingCabinetEventHandlers();
            SetCabinetEventHandlers();
        }

        /// <summary>
        /// Remove the reference to the current cabinet library after releasing the portal category.
        /// This is needed when the game is parked and CSI is disconnected.
        /// </summary>
        public static void RemoveCabinetLibrary()
        {
            cabinetLib = null;
            currentPortalCategory = null;
            portalClassRequested = false;
            portalClassIdentifier = string.Empty;
            PortalClassAcquired = false;
            PortalClassConnected = false;
            ClearExistingCabinetEventHandlers();
        }

        /// <summary>
        /// Returns a portal category to be used with the specified portal class.
        /// </summary>
        /// <param name="portalClass">The identifier of the portal class to acquire ownership of./</param>
        /// <returns>
        /// Returns an instance of the portal category. Returns null if the portal class has been requested
        /// but not yet acquired.
        /// </returns>
        /// <exception cref="PortalControllerException">
        /// Thrown if the cabinet library has not been set and therefore cannot be used
        /// to acquire the portal category, if the client already has control of a portal class, or
        /// if the device is unable to be acquired.
        /// </exception>
        public static IPortal GetPortalClass(string portalClass)
        {
            if(cabinetLib == null)
            {
                throw new PortalControllerException("Cabinet library has not been set for the portal category.");
            }

            if(!string.IsNullOrEmpty(portalClassIdentifier) && portalClassIdentifier != portalClass)
            {
                throw new PortalControllerException(
                    "Client cannot acquire more than one portal class at a time" +
                    " (client has already acquired the '" + portalClassIdentifier + "' portal class).");
            }

            if(!PortalClassAcquired)
            {
                if(portalClassRequested)
                {
                    // If the portal class has been requested but not yet acquired, 
                    // return null until acquisition takes place.
                    return null;
                }

                var devices = cabinetLib.GetConnectedDevices();
                if(devices.Any(device => device.DeviceType == DeviceType.Portal))
                {
                    portalClassIdentifier = portalClass;
                    var result = cabinetLib.RequestAcquireDevice(DeviceType.Portal, portalClassIdentifier,
                        clientPriority);

                    if(!(PortalClassAcquired = result.Acquired))
                    {
                        if(result.Reason == DeviceAcquisitionFailureReason.RequestQueued)
                        {
                            portalClassRequested = true;
                        }
                        else
                        {
                            throw new PortalControllerException(
                                "Portal class unable to be acquired: " + result.Reason);
                        }

                        return null;
                    }
                }
            }

            return currentPortalCategory; 
        }

        /// <summary>
        /// Releases the acquisition of the portal class.
        /// </summary>
        public static void ReleasePortalClass()
        {
            if(cabinetLib != null && PortalClassAcquired)
            {
                if(!string.IsNullOrEmpty(portalClassIdentifier))
                {
                    cabinetLib.ReleaseDevice(DeviceType.Portal, portalClassIdentifier);
                    portalClassIdentifier = string.Empty;
                }

                PortalClassAcquired = false;
                PortalClassConnected = false;
                portalClassRequested = false;
            }
        }

        /// <summary>
        /// Sets event handlers for cabinet level events.
        /// </summary>
        private static void SetCabinetEventHandlers()
        {
            if(cabinetLib != null)
            {
                cabinetLib.DeviceAcquiredEvent += OnPortalClassAcquiredEvent;
                cabinetLib.DeviceConnectedEvent += OnPortalClassConnectedEvent;
                cabinetLib.DeviceReleasedEvent += OnPortalClassReleasedEvent;
                cabinetLib.DeviceRemovedEvent += OnPortalClassRemovedEvent;
            }
        }

        /// <summary>
        /// Clears any existing event handlers for cabinet level events.
        /// </summary>
        private static void ClearExistingCabinetEventHandlers()
        {
            if(cabinetLib != null)
            {
                cabinetLib.DeviceAcquiredEvent -= OnPortalClassAcquiredEvent;
                cabinetLib.DeviceConnectedEvent -= OnPortalClassConnectedEvent;
                cabinetLib.DeviceReleasedEvent -= OnPortalClassReleasedEvent;
                cabinetLib.DeviceRemovedEvent -= OnPortalClassRemovedEvent;
            }
        }

        #region CabinetEventHandlers

        /// <summary>
        /// The method called when the portal class is acquired.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="args">Event related arguments.</param>
        private static void OnPortalClassAcquiredEvent(object sender, DeviceAcquiredEventArgs args)
        {
            if(args.DeviceName == DeviceType.Portal)
            {
                PortalClassAcquired = true;
                portalClassRequested = false;

                PortalClassAcquiredEvent?.Invoke(sender, args);
            }
        }

        /// <summary>
        /// The method called when the portal class is released.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="args">Event related arguments.</param>
        private static void OnPortalClassReleasedEvent(object sender, DeviceReleasedEventArgs args)
        {
            if(args.DeviceName == DeviceType.Portal)
            {
                PortalClassAcquired = false;

                PortalClassReleasedEvent?.Invoke(sender, args);
            }
        }

        /// <summary>
        /// The method called when the portal class is connected.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="args">Event related arguments.</param>
        private static void OnPortalClassConnectedEvent(object sender, DeviceConnectedEventArgs args)
        {
            if(args.DeviceName == DeviceType.Portal)
            {
                PortalClassConnected = true;

                PortalClassConnectedEvent?.Invoke(sender, args);
            }
        }

        /// <summary>
        /// The method called when the portal class is removed.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="args">Event related arguments.</param>
        private static void OnPortalClassRemovedEvent(object sender, DeviceRemovedEventArgs args)
        {
            if(args.DeviceName == DeviceType.Portal)
            {
                PortalClassConnected = false;
                PortalClassAcquired = false;
                portalClassRequested = false;

                PortalClassRemovedEvent?.Invoke(sender, args);
            }
        }

        #endregion
    }
}