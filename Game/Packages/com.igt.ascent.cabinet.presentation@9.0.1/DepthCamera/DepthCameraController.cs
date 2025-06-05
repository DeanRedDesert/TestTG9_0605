//-----------------------------------------------------------------------
// <copyright file = "DepthCameraController.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.DepthCamera
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas;

    /// <summary>
    /// A class responsible for acquiring/releasing/controlling depth camera devices.
    /// </summary>
    public static class DepthCameraController
    {
        #region Private Fields

        /// <summary>
        /// Reference to the cabinet library.
        /// </summary>
        private static ICabinetLib cabinet;

        /// <summary>
        /// List of available depth cameras
        /// </summary>
        private static IEnumerable<DepthCameraDevice> availableDepthCameras;

        /// <summary>
        /// The IDepthCamera interface.
        /// </summary>
        private static IDepthCamera depthCameraInterface;

        /// <summary>
        /// List of acquired depth cameras.
        /// </summary>
        private static IList<string> AcquiredCameras { get; set; }

        #endregion

        #region Public Fields

        /// <summary>
        /// List of acquired depth cameras.
        /// </summary>
        public static IList<string> AcquiredDepthCameras => AcquiredCameras;

        /// <summary>
        /// An event raised when a device is acquired.
        /// </summary>
        public static event EventHandler<DeviceAcquiredEventArgs> DeviceAcquiredEvent;

        /// <summary>
        /// An event raised when a device is released.
        /// </summary>
        public static event EventHandler<DeviceReleasedEventArgs> DeviceReleasedEvent; 

        #endregion

        #region Public Methods

        /// <summary>
        /// Set up the current category and acquire the cameras.
        /// </summary>
        /// <param name="cabinetLib">
        /// Reference to the current cabinet library. This interface will be used for requesting control of depth cameras.
        /// </param>
        /// <param name="priority">The priority.</param>
        /// <exception cref="ArgumentNullException">Thrown if the cabinetLib is null.</exception>
        public static void SetCabinetLibrary(ICabinetLib cabinetLib, Priority priority)
        {
            cabinet = cabinetLib ?? throw new ArgumentNullException(nameof(cabinetLib), "A valid cabinet interface is required.");

            if(AcquiredCameras == null)
            {
                AcquiredCameras = new List<string>();
            }

            if(cabinet != null)
            {
                cabinetLib.DeviceAcquiredEvent -= OnDeviceAcquired;
                cabinetLib.DeviceReleasedEvent -= OnDeviceReleased;

                depthCameraInterface = cabinet.GetInterface<IDepthCamera>();
                if(depthCameraInterface != null)
                {
                    availableDepthCameras = depthCameraInterface.GetAvailableDepthCameras();

                    if(availableDepthCameras.Any())
                    {
                        cabinet.DeviceAcquiredEvent += OnDeviceAcquired;
                        cabinet.DeviceReleasedEvent += OnDeviceReleased;

                        AcquireDepthCameras(priority);
                    }
                }
            }
        }

        /// <summary>
        /// Remove any event handlers or other references to the cabinet library.
        /// </summary>
        public static void RemoveCabinetLibrary()
        {
            if(cabinet != null)
            {
                var depthCameraList = new List<string>(AcquiredCameras);

                // Release acquired devices
                foreach(var camera in depthCameraList)
                {
                    cabinet.ReleaseDevice(DeviceType.DepthCamera, camera);
                    OnDeviceReleased(null, new DeviceReleasedEventArgs(DeviceType.DepthCamera, camera, DeviceAcquisitionFailureReason.RequestQueued));
                }

                cabinet.DeviceAcquiredEvent -= OnDeviceAcquired;
                cabinet.DeviceReleasedEvent -= OnDeviceReleased;

                cabinet = null;
                depthCameraInterface = null;
                availableDepthCameras = null;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Attempt to acquire all available depth camera devices.
        /// </summary>
        /// <exception cref="DepthCameraControllerException">
        /// Thrown if cameras cannot be acquired.
        /// </exception>
        private static void AcquireDepthCameras(Priority priority)
        {
            if(cabinet == null || depthCameraInterface == null)
            {
                throw new DepthCameraControllerException("Error Acquiring depth cameras.");
            }

            // Clear cached cameras
            AcquiredCameras.Clear();

            if(availableDepthCameras != null)
            {
                foreach(var camera in availableDepthCameras)
                {
                    // Acquire cameras.
                    var result = cabinet.RequestAcquireDevice(DeviceType.DepthCamera, camera.DepthCameraDeviceId,
                        priority);

                    // Check the result
                    if(result.Acquired)
                    {
                        // Fire the acquire event
                        OnDeviceAcquired(null, new DeviceAcquiredEventArgs(DeviceType.DepthCamera, camera.DepthCameraDeviceId));
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for <see cref="ICabinetLib.DeviceAcquiredEvent"/>.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        private static void OnDeviceAcquired(object sender, DeviceAcquiredEventArgs args)
        {
            // We are only interested in depth cameras.
            if(args.DeviceName == DeviceType.DepthCamera)
            {
                var cameraId = args.DeviceId;

                // Add device to the cache.
                if(cameraId != null && !AcquiredCameras.Contains(cameraId))
                {
                    AcquiredCameras.Add(cameraId);

                    // Fire the custom event
                    DeviceAcquiredEvent?.Invoke(sender, args);
                }
            }
        }

        /// <summary>
        /// Event handler for <see cref="ICabinetLib.DeviceReleasedEvent"/>.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        private static void OnDeviceReleased(object sender, DeviceReleasedEventArgs args)
        {
            // We are only interested in depth cameras.
            if(args.DeviceName == DeviceType.DepthCamera)
            {
                var cameraId = args.DeviceId;
                
                // Device released, remove it from the cache.
                if(cameraId != null && AcquiredCameras.Contains(cameraId))
                {
                    AcquiredCameras.Remove(cameraId);
                }

                // Fire the custom event
                DeviceReleasedEvent?.Invoke(sender, args);
            }
        }

        #endregion
    }
}
