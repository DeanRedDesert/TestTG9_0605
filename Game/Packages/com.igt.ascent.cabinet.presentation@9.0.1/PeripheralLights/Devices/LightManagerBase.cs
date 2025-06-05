// -----------------------------------------------------------------------
// <copyright file = "LightManagerBase.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas;

    /// <summary>
    /// The abstract base for all light manager classes that manages
    /// a list of light devices.
    /// </summary>
    internal abstract class LightManagerBase : ILightDeviceInquiry
    {
        #region Fields

        /// <summary>
        /// The specifications of light devices supported by this light manager.
        /// </summary>
        protected readonly Dictionary<Hardware, HardwareSpec> HardwareSpecs;

        /// <summary>
        /// The light devices that have been requested by the game via the GetPeripheralLight calls.
        /// </summary>
        protected readonly List<UsbLightBase> LightObjects =
            new List<UsbLightBase>();

        /// <summary>
        /// The light devices that have not been requested by the game.
        /// </summary>
        protected Dictionary<string, UsbLightBase> FreeLightObjects =
            new Dictionary<string, UsbLightBase>();

        /// <summary>
        /// The dictionary of light objects created per game's requests, keyed by the feature name.
        /// This dictionary is auto-generated based on <see cref="LightObjects"/>.
        /// </summary>
        protected Dictionary<string, UsbLightBase> LightObjectsByFeatureName =
            new Dictionary<string, UsbLightBase>();

        /// <summary>
        /// The dictionary of light objects created per game's requests, keyed by the hardware type.
        /// This dictionary is auto-generated based on <see cref="LightObjects"/>.
        /// </summary>
        protected Dictionary<Hardware, UsbLightBase> LightObjectsByHardware =
            new Dictionary<Hardware, UsbLightBase>();

        /// <summary>
        /// The interface for communicating with the CSI manager on device acquisitions and releases.
        /// </summary>
        protected ICabinetLib CabinetInterface;

        /// <summary>
        /// The priority of the process running the light manager.
        /// </summary>
        protected Priority ProcessPriority;

        /// <summary>
        /// The list of feature descriptions of all devices currently connected to the EGM.
        /// </summary>
        protected IList<LightFeatureDescription> FeatureDescriptions;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="LightManagerBase"/>.
        /// </summary>
        /// <param name="hardwareSpecList">
        /// The specifications of the light devices to be supported by the light manager.
        /// </param>
        protected LightManagerBase(IList<HardwareSpec> hardwareSpecList)
        {
            // Convert the list to a dictionary to boost the performance.
            HardwareSpecs = hardwareSpecList.ToDictionary(spec => spec.HardwareType, spec => spec);
        }

        #endregion

        #region ILightDeviceInquiry Members

        /// <inheritdoc/>
        public bool IsAnyDeviceConnected()
        {
            return FeatureDescriptions != null && FeatureDescriptions.Count > 0;
        }

        /// <inheritdoc/>
        public bool IsDeviceConnected(Hardware hardware)
        {
            return GetFeatureDescription(hardware) != null;
        }

        /// <inheritdoc/>
        public bool IsDeviceAcquired(string featureName)
        {
            var result = false;

            if(LightObjectsByFeatureName.TryGetValue(featureName, out var lightObject) ||
               FreeLightObjects.TryGetValue(featureName, out lightObject))
            {
                result = lightObject?.DeviceAcquired == true;
            }

            return result;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the communication interfaces and other arguments to be used by this light manager.
        /// </summary>
        /// <param name="cabinetLib">
        /// The interface for communicating with the CSI manager on device acquisitions and releases.
        /// </param>
        /// <param name="processPriority">
        /// The priority of the process running the light manager.
        /// </param>
        /// <param name="lightInterfaceObject">
        /// The interface for controlling light devices.  Derived classes should convert this object
        /// to the appropriate interfaces they need.
        /// </param>
        public void SetLightInterface(ICabinetLib cabinetLib, Priority processPriority, object lightInterfaceObject)
        {
            CabinetInterface = cabinetLib;
            ProcessPriority = processPriority;

            // This call sets the light interface for the light manager and all light objects managed.
            ConvertAndSetLightInterface(lightInterfaceObject);

            RefreshFeatureDescriptions();

            foreach(var lightObject in LightObjects)
            {
                // Try to re-acquire the device from the cabinet.
                AcquireLightDevice(lightObject);
            }
        }

        /// <summary>
        /// Remove the communication interfaces.
        /// </summary>
        public void RemoveLightInterface()
        {
            CabinetInterface = null;

            // This call sets the light interface for the light manager and all light objects managed.
            ConvertAndSetLightInterface(null);

            FeatureDescriptions = null;

            // Release lights without notifying CSI manager.
            // When the game is parked, the CSI manager will release all devices held by the game automatically.
            foreach(var lightObject in AllLightObjects)
            {
                lightObject.DeviceAcquired = false;

                // Reset the feature description in case it changes after re-connection.
                lightObject.SetFeatureDescription(null);
            }
        }

        /// <summary>
        /// Release control over any acquired light devices.
        /// </summary>
        public void ReleaseLights()
        {
            if(CabinetInterface != null)
            {
                foreach(var lightObject in AllLightObjects)
                {
                    lightObject.DeviceAcquired = false;
                    CabinetInterface.ReleaseDevice(LightDeviceType, lightObject.FeatureName);
                }
            }
        }

        /// <summary>
        /// Checks if this light manager supports a specific hardware type.
        /// </summary>
        /// <param name="hardware">The hardware type to check.</param>
        /// <returns>True if the light manager supports the given hardware type; False otherwise.</returns>
        public bool Supports(Hardware hardware)
        {
            return HardwareSpecs.ContainsKey(hardware);
        }

        /// <summary>
        /// Gets a light device object of a specific hardware type.
        /// Creates a new device instance if needed.
        /// </summary>
        /// <param name="hardware">The hardware type of the light device to get.</param>
        /// <param name="lightType">The light device type expected.</param>
        /// <param name="allowCoupling">Specifies that a game client can use multiple interfaces to request a single coupled light device.</param>
        /// <returns>The light device of the given hardware type.</returns>
        /// <exception cref="LightTypeNotCompatibleException">
        /// Thrown when the given hardware is not compatible with the created light object.
        /// </exception>
        /// <exception cref="CoupledLightException">
        /// Thrown when requesting a coupled light device with multiple interfaces and not explicitly stating to do so.
        /// </exception>
        public UsbLightBase GetLightObject(Hardware hardware, Type lightType, bool allowCoupling = false)
        {
            if(GetLightInterface() == null)
            {
                throw new CabinetDoesNotSupportLightsInterfaceException(LightDeviceType.ToString());
            }

            // Check if it has been requested by the game before.
            if(!LightObjectsByHardware.TryGetValue(hardware, out var lightObject))
            {
                // If not, create a new instance.
                lightObject = CreateLightObject(hardware);

                // Try to acquire the device.
                if(lightObject.WasFeatureFoundAtCreation)
                {
                    AcquireLightDevice(lightObject);
                }

                LightObjects.Add(lightObject);
                UpdateLookupTables();

                // If the device used to be a free light object, remove it from the list.
                if(FreeLightObjects.ContainsKey(lightObject.FeatureName))
                {
                    FreeLightObjects.Remove(lightObject.FeatureName);
                }
                var coupledFeatureName = HardwareSpecs[hardware].CoupledFeatureName;

                if(coupledFeatureName != null)
                {
                    // Check if it has been requested by the game before.
                    if(LightObjectsByFeatureName.ContainsKey(coupledFeatureName) && !allowCoupling)
                    {
                        throw new CoupledLightException(
                            $"Coupled Light Device has been requested with the hardware {lightObject.HardwareType}, " +
                            $"however the light device has previously been requested with the hardware {LightObjectsByFeatureName[coupledFeatureName].HardwareType}. " +
                            "Please ensure that you intended for this to occur by setting the flag allowCoupling to true " +
                            "when requesting the device with both types of hardware.");
                    }
                }
            }

            // Light devices can be access as a more basic type (ex. CatalinaReelBackLights that support symbol tracking can 
            // be obtained as both a UsbStreamingLight and UsbSymbolHighlightSupportedStreamingLight), but cannot be accessed 
            // as an inherited type (CatalinaReelBackLights that DO NOT support symbol tracking cannot be obtained as a 
            // UsbSymbolHighlightSupportedStreamingLight). 
            if(!lightType.IsInstanceOfType(lightObject))
            {
                throw new LightTypeNotCompatibleException(lightType, hardware);
            }

            return lightObject;
        }

        /// <summary>
        /// Handles the event when a device is acquired.
        /// </summary>
        /// <param name="featureName">The feature name of the light device that has been acquired.</param>
        /// <returns>The light device object that has been acquired.</returns>
        public UsbLightBase HandleDeviceAcquired(string featureName)
        {
            UsbLightBase result = null;

            // We update the status of both requested and free light object matching the feature name, but
            // only return the matching light object in the requested list.  Do not return the free ones.
            if(!LightObjectsByFeatureName.TryGetValue(featureName, out var lightObject))
            {
                FreeLightObjects.TryGetValue(featureName, out lightObject);
            }
            else
            {
                result = lightObject;
            }

            if(lightObject != null)
            {
                lightObject.DeviceAcquired = true;

                OnDeviceAcquisition(lightObject);
            }

            return result;
        }

        /// <summary>
        /// Handles the event when a device is connected.
        /// </summary>
        /// <param name="featureName">The feature name of the light device that has been connected.</param>
        /// <returns>The light device object that has been acquired after the connection.</returns>
        public UsbLightBase HandleDeviceConnected(string featureName)
        {
            RefreshFeatureDescriptions();

            UsbLightBase result = null;

            if(LightObjectsByFeatureName.TryGetValue(featureName, out var lightObject))
            {
                AcquireLightDevice(lightObject);

                // Only return the light object if it is acquired.
                if(lightObject.DeviceAcquired)
                {
                    result = lightObject;
                }
            }

            return result;
        }

        /// <summary>
        /// Handles the event when a device is released.
        /// </summary>
        /// <param name="featureName">The feature name of the light device that has been released.</param>
        /// <param name="reason">The reason why the light device was released.</param>
        /// <returns>The light device that was released.</returns>
        public UsbLightBase HandleDeviceReleased(string featureName, DeviceAcquisitionFailureReason reason)
        {
            if(LightObjectsByFeatureName.TryGetValue(featureName, out var lightObject) ||
               FreeLightObjects.TryGetValue(featureName, out lightObject))
            {
                lightObject.DeviceAcquired = false;
                lightObject.AcquireFailureReason = reason;
            }

            return lightObject;
        }

        /// <summary>
        /// Handles the event when a device is removed/disconnected.
        /// </summary>
        /// <param name="featureName">The feature name of the light device that has been removed.</param>
        /// <returns>The light device that was removed.</returns>
        public UsbLightBase HandleDeviceRemoved(string featureName)
        {
            RefreshFeatureDescriptions();

            // Check if the light feature has been requested by the game before.
            // No need to update free light objects, they are removed in RefreshFeatureDescriptions.
            if(LightObjectsByFeatureName.TryGetValue(featureName, out var lightObject))
            {
                lightObject.DeviceAcquired = false;

                // Reset the feature description in case it changes after re-connection.
                lightObject.SetFeatureDescription(null);
            }

            return lightObject;
        }

        /// <summary>
        /// Get all the light device objects that
        /// <list type="bullet">
        ///     <item>Have not been requested the game via the GetPeripheralLight call, OR</item>
        ///     <item>Have been requested by the game, but don not have valid light content yet.</item>
        /// </list>
        /// </summary>
        /// <returns>The list of the light device objects with blank content.</returns>
        public IList<UsbLightBase> GetBlankLightObjects()
        {
            var blankLights = GetVerifiedBlankLights();

            foreach(var lightObject in blankLights.Where(device => !device.DeviceAcquired))
            {
                AcquireLightDevice(lightObject);
            }

            return blankLights.Where(device => device.DeviceAcquired).ToList();
        }

        #endregion

        #region Unit Test Helpers

        /// <summary>
        /// Clear all light device objects managed by this light manager.
        /// This method is provided for unit testing purpose only.
        /// </summary>
        public void ClearLightObjects()
        {
            LightObjects.Clear();
            UpdateLookupTables();

            FreeLightObjects.Clear();
        }

        #endregion

        #region Abstract and Virtual Members

        /// <summary>
        /// Gets the CSI device type of the light device objects managed by this light manager.
        /// It is either <see cref="DeviceType.Light"/> or <see cref="DeviceType.StreamingLight"/>.
        /// </summary>
        protected abstract DeviceType LightDeviceType { get; }

        /// <summary>
        /// Converts the light interface object to the appropriate interface type needed by the light manager,
        /// then updates the light manager and all light device objects accordingly.
        /// </summary>
        /// <param name="lightInterfaceObject">The light interface object to convert.</param>
        protected abstract void ConvertAndSetLightInterface(object lightInterfaceObject);

        /// <summary>
        /// Gets the light interface object used by the light manager.
        /// </summary>
        /// <returns>The light interface object used by the light manager.</returns>
        protected abstract object GetLightInterface();

        /// <summary>
        /// Queries the CSI manager for the feature descriptions of all connected light devices.
        /// </summary>
        /// <returns>The list of the feature descriptions of all connected light devices</returns>
        protected abstract IList<LightFeatureDescription> QueryFeatureDescriptions();

        protected abstract UsbLightBase CreateFreeLightObject(string featureName);

        /// <summary>
        /// Post processor after a device acquisition.
        /// </summary>
        /// <param name="device">The light device that has been acquired.</param>
        protected virtual void OnDeviceAcquisition(UsbLightBase device)
        {
            if(device != null && device.FeatureDescription == null)
            {
                // Make sure to use Feature Name to find the description, do not use Hardware type,
                // since Free Light Objects will have unknown hardware types.
                device.SetFeatureDescription(GetFeatureDescription(device.FeatureName));
            }
        }

        #endregion

        #region Protected Members

        /// <summary>
        /// Gets all light device objects managed by the light manager.
        /// </summary>
        protected IEnumerable<UsbLightBase> AllLightObjects => LightObjects.Union(FreeLightObjects.Values);

        /// <summary>
        /// Gets the description of a specific light feature.
        /// </summary>
        /// <param name="featureName">The name of the light feature.</param>
        /// <returns>
        /// The feature description of the given light feature.
        /// Null if the light feature is not connected.
        /// </returns>
        protected LightFeatureDescription GetFeatureDescription(string featureName)
        {
            return FeatureDescriptions.FirstOrDefault(description => description.FeatureId == featureName);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the specification of a specific hardware type.
        /// </summary>
        /// <param name="hardware">The hardware type to get the specification for.</param>
        /// <returns>
        /// The specification of the given hardware type.
        /// Null if the given hardware type is not supported.
        /// </returns>
        private HardwareSpec GetHardwareSpec(Hardware hardware)
        {
            HardwareSpecs.TryGetValue(hardware, out var result);
            return result;
        }

        /// <summary>
        /// Update the lookup tables of the light objects.
        /// </summary>
        private void UpdateLookupTables()
        {
            LightObjectsByFeatureName = LightObjects.ToDictionary(device => device.FeatureName, device => device);
            LightObjectsByHardware = LightObjects.ToDictionary(device => device.HardwareType, device => device);
        }

        /// <summary>
        /// Queries the feature descriptions of all connected light devices,
        /// and updates the free light object list accordingly.
        /// </summary>
        private void RefreshFeatureDescriptions()
        {
            FeatureDescriptions = QueryFeatureDescriptions();

            // Get all feature names that are not shown in Light Objects.
            var freeFeatures = FeatureDescriptions.Select(description => description.FeatureId)
                                                  .Except(LightObjectsByFeatureName.Keys)
                                                  .ToList();

            // Remove the free light objects that are not connected any more.
            var removed = FreeLightObjects.Keys.Except(freeFeatures).ToList();
            foreach(var featureName in removed)
            {
                FreeLightObjects.Remove(featureName);
            }

            // Add the free light objects that are newly connected.
            var added = freeFeatures.Except(FreeLightObjects.Keys).ToList();
            foreach(var featureName in added)
            {
                FreeLightObjects.Add(featureName, CreateFreeLightObject(featureName));
            }
        }

        /// <summary>
        /// Gets the feature description of a specific hardware type.
        /// </summary>
        /// <param name="hardware">The hardware type to get the feature description for.</param>
        /// <returns>
        /// The feature description of the given hardware type.
        /// Null if the given hardware type is not supported or not connected.
        /// </returns>
        private LightFeatureDescription GetFeatureDescription(Hardware hardware)
        {
            LightFeatureDescription result = null;

            var spec = GetHardwareSpec(hardware);
            if(spec != null)
            {
                result = GetFeatureDescription(spec.FeatureName);
            }

            return result;
        }

        /// <summary>
        /// Creates a new instance of the light device object.
        /// </summary>
        /// <param name="hardware">The hardware type of the light device to create.</param>
        /// <returns>The light device object created.</returns>
        private UsbLightBase CreateLightObject(Hardware hardware)
        {
            var spec = GetHardwareSpec(hardware);
            var featureDescription = GetFeatureDescription(spec.FeatureName);
            var lightInterface = GetLightInterface();

            var lightObject = spec.CreateDevice(spec.FeatureName, featureDescription, lightInterface);

            lightObject.HardwareType = spec.HardwareType;
            lightObject.WasFeatureFoundAtCreation = featureDescription != null;

            return lightObject;
        }

        /// <summary>
        /// Acquires a light device from the CSI manager.
        /// </summary>
        /// <param name="device">The light device to acquire.</param>
        private void AcquireLightDevice(UsbLightBase device)
        {
            if(CabinetInterface != null && device != null)
            {
                var acquireDeviceResult = CabinetInterface.RequestAcquireDevice(LightDeviceType, device.FeatureName, ProcessPriority);

                device.DeviceAcquired = acquireDeviceResult.Acquired;
                device.AcquireFailureReason = acquireDeviceResult.Reason;

                if(device.DeviceAcquired)
                {
                    OnDeviceAcquisition(device);
                }
            }
        }

        /// <summary>
        /// Gets the light objects that have not been requested by the game or that have been requested but have not had any client content set yet.
        /// </summary>
        /// <remarks>
        /// This method is accommodating for coupled light devices.
        /// If both of the interfaces for a coupled light device have not been requested or have not had client content set, only 1 will be returned.
        /// If a coupled interface has already been requested by a client and set with client content, the related interface will not be returned.
        /// </remarks>
        /// <returns>The list of verified blank lights.</returns>
        private List<UsbLightBase> GetVerifiedBlankLights()
        {
            var blankLights = LightObjects.Where(device => !device.HasClientContent)
                .Union(FreeLightObjects.Values)
                .ToList();

            var verifiedBlankLights = new List<UsbLightBase>();

            foreach(var blankLight in blankLights)
            {
                // Get the spec of the blank light using FeatureName,
                // since Free Light Object does not have HardwareType set yet.
                var spec = HardwareSpecs.Values.FirstOrDefault(value => value.FeatureName == blankLight.FeatureName);

                // If a blank light has a coupled interface.
                if(spec?.CoupledFeatureName != null)
                {
                    // Check if the coupled interface has been requested by client.
                    if(LightObjectsByFeatureName.TryGetValue(spec.CoupledFeatureName, out var coupledLight))
                    {
                        // Add the blank light if the coupled one is also blank and has not been added.
                        if(!coupledLight.HasClientContent && !verifiedBlankLights.Contains(coupledLight))
                        {
                            verifiedBlankLights.Add(blankLight);
                        }
                    }
                    // If the coupled interface has not been requested by client.
                    else if(FreeLightObjects.TryGetValue(spec.CoupledFeatureName, out coupledLight))
                    {
                        // Add the blank light if the coupled one is not added yet.
                        if(!verifiedBlankLights.Contains(coupledLight))
                        {
                            verifiedBlankLights.Add(blankLight);
                        }
                    }
                }
                else
                {
                    verifiedBlankLights.Add(blankLight);
                }
            }
            return verifiedBlankLights;
        }

        #endregion
    }
}