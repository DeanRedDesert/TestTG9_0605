//-----------------------------------------------------------------------
// <copyright file = "DeviceManager.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using CSI.Schemas;
    using IgtUsbDevice;
    using Interop;
    using Microsoft.Win32.SafeHandles;
    using UsbFeatureZero;

    /// <summary>
    /// This class manages physical IGT USB devices that are connected to a standalone game.
    /// </summary>
    public sealed class DeviceManager : IDeviceManager, IDisposable
    {
        #region Constants

        /// <summary>
        /// GUID for IGT class devices.
        /// </summary>
        public static Guid IgtUsbGuid = new Guid(0xc638df45, 0x2530, 0x498d, 0x9d, 0x1a, 0x5b, 0xc9, 0x79, 0xb1, 0xff, 0xb0);

        /// <summary>
        /// Current IGT Class USB protocol version.
        /// </summary>
        public const byte CurrentIgtVersion = 0;

        /// <summary>
        /// Constant INVALID_HANDLE_VALUE in winbase.h.
        /// </summary>
        private static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

        /// <summary>
        /// A string prefix to be removed from some of
        /// the device's string descriptors.
        /// </summary>
        private const string IgtClassIdentifier = "©IGT2003";

        /// <summary>
        /// The max number of characters in a device's string descriptor.
        /// </summary>
        private const int MaxStringSize = 128;

        /// <summary>
        /// The max size of the buffer to retrieve a device's
        /// feature specific descriptor.
        /// </summary>
        private const int MaxFeatureSpecificBuffer = 255;

        /// <summary>
        /// Lookup table to map <see cref="DeviceFeatureNumber"/> values
        /// to <see cref="DeviceType"/> values.
        /// </summary>
        private static readonly Dictionary<DeviceFeatureNumber, DeviceType> FeatureNumberToDeviceType =
            new Dictionary<DeviceFeatureNumber, DeviceType>
            {
                {DeviceFeatureNumber.Light, DeviceType.Light},
                {DeviceFeatureNumber.Reel, DeviceType.Reel},
                {DeviceFeatureNumber.ButtonPanel, DeviceType.ButtonPanel},
                {DeviceFeatureNumber.StreamingLight, DeviceType.StreamingLight}
            };

        /// <summary>
        /// Lookup table to map <see cref="DeviceType"/> values
        /// to <see cref="DeviceFeatureNumber"/> values.
        /// This is a reversed dictionary of <see cref="FeatureNumberToDeviceType"/>.
        /// </summary>
        // ReSharper disable InconsistentNaming
        private readonly Dictionary<DeviceType, DeviceFeatureNumber> DeviceTypeToFeatureNumber;
        // ReSharper restore InconsistentNaming

        #endregion

        #region Private Fields

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        private volatile bool disposed;

        /// <summary>
        /// List of device data of connected devices.
        /// </summary>
        /// <remarks>
        /// This list is keyed by Driver Name of the device data.
        /// It is important to make sure that the driver names
        /// are unified in casing, e.g. always in lowercase.
        /// </remarks>
        private readonly List<UsbDeviceData> deviceDataList = new List<UsbDeviceData>();

        /// <summary>
        /// List of groups of devices registered by the game,
        /// grouped by the driver address.
        /// </summary>
        /// <remarks>
        /// This list is keyed by Driver Address of the device workgroup.
        /// It is important to make sure that the driver addresses
        /// are unified in casing, e.g. always in lowercase.
        /// </remarks>
        private readonly List<DeviceWorkgroup> deviceWorkgroups = new List<DeviceWorkgroup>();

        /// <summary>
        /// Object for synchronizing access to device change queue.
        /// </summary>
        private readonly object deviceChangeQueueLocker = new object();

        /// <summary>
        /// The queue of device changes reported by Device Monitor.
        /// </summary>
        private Queue<DeviceChangeEventArgs> deviceChangeQueue = new Queue<DeviceChangeEventArgs>();

        #endregion

        #region Methods

        #region Constructor

        /// <summary>
        /// Initialize an instance of <see cref="DeviceManager"/> class.
        /// </summary>
        public DeviceManager()
        {
            // Reverse the first dictionary to generate the second one.
            DeviceTypeToFeatureNumber = FeatureNumberToDeviceType.ToDictionary(x => x.Value, x => x.Key);

            // Find all IGT class devices, and initialize the
            // device data list.
            var devicePaths = FindIgtClassDevices();

            foreach(var devicePath in devicePaths)
            {
                AddDeviceData(devicePath);
            }

            // Instantiate Feature Zeros.
            var featureZeroDataList = deviceDataList.Where(deviceData => deviceData.IsFeatureZero);

            foreach(var deviceData in featureZeroDataList)
            {
                AddFeatureZero(deviceData);
            }

            // Subscribe to Device Monitor.
            DeviceMonitor.DeviceChangeEvent += HandleDeviceChange;

            // Start Device Monitor.
            DeviceMonitor.Start(IgtUsbGuid);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Create a device handle using the given device driver name.
        /// Utility method available for all devices to use as well.
        /// </summary>
        /// <param name="driverName">The driver name of the device.</param>
        /// <returns>The device handle created.</returns>
        /// <exception cref="DeviceControlException">
        /// Thrown if failed to create the handle.
        /// </exception>
        public static SafeFileHandle CreateDeviceHandle(string driverName)
        {
            var deviceHandle = Win32Methods.CreateFile(driverName,
                                                       EFileAccess.GenericRead | EFileAccess.GenericWrite,
                                                       EFileShare.Read | EFileShare.Write,
                                                       IntPtr.Zero,
                                                       ECreationDisposition.OpenExisting,
                                                       EFileAttributes.None,
                                                       IntPtr.Zero);
            if(deviceHandle.IsInvalid)
            {
                throw new DeviceControlException("Failed to create device handle.", Marshal.GetLastWin32Error());
            }

            return deviceHandle;
        }

        #endregion

        #region IDeviceManager Members

        /// <inheritdoc />
        public event EventHandler<DeviceConnectedEventArgs> DeviceConnectedEvent;

        /// <inheritdoc />
        public event EventHandler<DeviceRemovedEventArgs> DeviceRemovedEvent;

        /// <inheritdoc />
        public List<DeviceIdentifier> GetConnectedDevices()
        {
            var selected = from deviceData in deviceDataList
                           let featureNumber = deviceData.FunctionalDescriptor.FeatureNumber
                           where FeatureNumberToDeviceType.ContainsKey(featureNumber)
                           select new DeviceIdentifier(FeatureNumberToDeviceType[featureNumber],
                                                       deviceData.InterfaceName);

            return selected.ToList();
        }

        /// <inheritdoc />
        public List<UsbDeviceData> GetDeviceData(DeviceType deviceType)
        {
            List<UsbDeviceData> result;

            if(DeviceTypeToFeatureNumber.ContainsKey(deviceType))
            {
                var selected = from deviceData in deviceDataList
                               let featureNumber = DeviceTypeToFeatureNumber[deviceType]
                               where deviceData.FunctionalDescriptor.FeatureNumber == featureNumber
                               select deviceData;

                result = selected.ToList();
            }
            else
            {
                result = new List<UsbDeviceData>();
            }

            return result;
        }

        /// <inheritdoc />
        public UsbDeviceData GetDeviceData(DeviceType deviceType, string deviceId)
        {
            UsbDeviceData result = null;

            if(DeviceTypeToFeatureNumber.ContainsKey(deviceType))
            {
                var selected = from deviceData in deviceDataList
                               let featureNumber = DeviceTypeToFeatureNumber[deviceType]
                               where deviceData.FunctionalDescriptor.FeatureNumber == featureNumber &&
                                     deviceData.InterfaceName == deviceId
                               select deviceData;

                result = selected.FirstOrDefault();
            }

            return result;
        }

        /// <inheritdoc />
        public void RegisterDevice(DeviceBase device)
        {
            var deviceWorkgroup = GetDeviceWorkgroup(device.DriverAddress, true);

            deviceWorkgroup.AddDevice(device);
        }

        /// <inheritdoc />
        public void UnregisterDevice(DeviceBase device)
        {
            var deviceWorkgroup = GetDeviceWorkgroup(device.DriverAddress);

            deviceWorkgroup?.RemoveDevice(device);
        }

        /// <inheritdoc />
        public void Update()
        {
            foreach(var deviceWorkgroup in deviceWorkgroups)
            {
                deviceWorkgroup.Update();
            }

            ProcessDeviceChanges();
        }

        #endregion

        #region Private Methods

        #region Initialization

        /// <summary>
        /// Search for all IGT class devices that are connected,
        /// and return the unique device path.
        /// </summary>
        /// <returns>Device paths of all connected IGT class devices.</returns>
        private static IEnumerable<string> FindIgtClassDevices()
        {
            var result = new List<string>();

            // Look for all devices that match the IGT GUID.
            var deviceInfoSet = Win32Methods.SetupDiGetClassDevs(ref IgtUsbGuid,
                                                                 IntPtr.Zero,
                                                                 IntPtr.Zero,
                                                                 DiGetClassFlags.DeviceInterface |
                                                                 DiGetClassFlags.Present);
            if(deviceInfoSet != InvalidHandleValue)
            {
                var success = true;
                uint memberIndex = 0;

                while(success)
                {
                    // Create a structure to receive data.
                    var deviceInterfaceData = DeviceInterfaceData.New();

                    // Enumerating the devices.
                    success = Win32Methods.SetupDiEnumDeviceInterfaces(deviceInfoSet,
                                                                       IntPtr.Zero,
                                                                       ref IgtUsbGuid,
                                                                       memberIndex,
                                                                       ref deviceInterfaceData);

                    if(success)
                    {
                        // Create a structure to receive data.
                        var deviceInterfaceDetailData = DeviceInterfaceDetailData.New();

                        // Get the detailed information on the device.
                        if(Win32Methods.SetupDiGetDeviceInterfaceDetail(deviceInfoSet,
                                                                        ref deviceInterfaceData,
                                                                        ref deviceInterfaceDetailData,
                                                                        Marshal.SizeOf(deviceInterfaceDetailData),
                                                                        out _,
                                                                        IntPtr.Zero))
                        {
                            result.Add(deviceInterfaceDetailData.DevicePath);
                        }
                    }
                    memberIndex++;
                }
            }

            Win32Methods.SetupDiDestroyDeviceInfoList(deviceInfoSet);

            return result;
        }

        /// <summary>
        /// Read the device data from the device at the given path.
        /// </summary>
        /// <param name="devicePath">Path to the device.</param>
        /// <returns>The device data retrieved.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="devicePath"/> is null.
        /// </exception>
        private static UsbDeviceData ReadDeviceData(string devicePath)
        {
            if(devicePath == null)
            {
                throw new ArgumentNullException(nameof(devicePath));
            }

            var result = new UsbDeviceData { DriverName = devicePath };

            using(var deviceHandle = CreateDeviceHandle(result.DriverName))
            {
                RetrieveGeneralDescriptors(deviceHandle, result);

                // Must be called after general descriptors have been retrieved.
                RetrieveStringDescriptors(deviceHandle, result);

                RetrievePortAddress(deviceHandle, result);
            }

            return result;
        }

        /// <summary>
        /// Read all general descriptors from a given device and save to
        /// a <see cref="UsbDeviceData"/> object.
        /// </summary>
        /// <param name="deviceHandle">The handle of the device to read from.</param>
        /// <param name="result">The device data that keeps all the results.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="deviceHandle"/> or <paramref name="result"/>
        /// is null.
        /// </exception>
        /// <exception cref="DeviceControlException">
        /// Thrown when device manager failed to read data from the device.
        /// </exception>
        private static void RetrieveGeneralDescriptors(SafeFileHandle deviceHandle, UsbDeviceData result)
        {
            if(deviceHandle == null)
            {
                throw new ArgumentNullException(nameof(deviceHandle));
            }

            if(result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            // Read the device descriptor.
            var deviceDescriptor = new UsbDeviceDescriptor();
            var success = ReadGeneralDescriptor(deviceHandle, UsbDescriptorType.Device, deviceDescriptor);
            if(!success)
            {
                throw new DeviceControlException("Failed to read device descriptor.", Marshal.GetLastWin32Error());
            }

            result.DeviceDescriptor = deviceDescriptor;

            // Read the configuration descriptor.
            var configurationDescriptor = new UsbConfigurationDescriptor();
            success = ReadGeneralDescriptor(deviceHandle, UsbDescriptorType.Configuration, configurationDescriptor);
            if(!success)
            {
                throw new DeviceControlException("Failed to read configuration descriptor.",
                                                 Marshal.GetLastWin32Error());
            }

            // Read again with bigger buffer for all associated descriptors.
            var totalLength = configurationDescriptor.TotalLength;
            var totalBuffer = new byte[totalLength];
            success = ReadGeneralDescriptor(deviceHandle, UsbDescriptorType.Configuration, totalBuffer, totalLength);
            if(!success)
            {
                throw new DeviceControlException("Failed to read the complete configuration descriptor.",
                                                 Marshal.GetLastWin32Error());
            }
            result.SetConfigurationDescriptors(totalBuffer);

            // Read the feature specific descriptor.
            var buffer = new byte[MaxFeatureSpecificBuffer];
            success = ReadGeneralDescriptor(deviceHandle, UsbDescriptorType.FeatureSpecific, buffer,
                                            MaxFeatureSpecificBuffer);
            if(!success)
            {
                throw new DeviceControlException("Failed to read feature specific descriptor.",
                                                 Marshal.GetLastWin32Error());
            }
            result.FeatureDescriptorData = buffer;
        }

        /// <summary>
        /// Read all string descriptors from a given device and save to
        /// a <see cref="UsbDeviceData"/> object.
        /// </summary>
        /// <param name="deviceHandle">The handle of the device to read from.</param>
        /// <param name="result">The device data that keeps all the results.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="deviceHandle"/> or <paramref name="result"/>
        /// is null.
        /// </exception>
        /// <exception cref="DeviceControlException">
        /// Thrown when device manager failed to read data from the device.
        /// </exception>
        private static void RetrieveStringDescriptors(SafeFileHandle deviceHandle, UsbDeviceData result)
        {
            if(deviceHandle == null)
            {
                throw new ArgumentNullException(nameof(deviceHandle));
            }

            if(result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            // Read the product name.
            result.ProductName = ReadStringDescriptor(deviceHandle,
                                                      (byte)(result.DeviceDescriptor.ManufacturerIndex + 1));
            if(result.ProductName == null)
            {
                throw new DeviceControlException("Failed to read product name.", Marshal.GetLastWin32Error());
            }

            // Read the vendor name.
            result.VendorName = ReadStringDescriptor(deviceHandle, result.DeviceDescriptor.ManufacturerIndex);
            if(result.VendorName == null)
            {
                throw new DeviceControlException("Failed to read vendor name.", Marshal.GetLastWin32Error());
            }

            // Read the interface name.
            // For composite devices the product index is the interface number.
            var interfaceName = ReadStringDescriptor(deviceHandle, result.DeviceDescriptor.ProductIndex);
            if(interfaceName == null)
            {
                throw new DeviceControlException("Failed to read interface name.", Marshal.GetLastWin32Error());
            }

            // Remove the prefix.
            result.InterfaceName = interfaceName.Remove(0, IgtClassIdentifier.Length);

            // Read the serial name.
            result.SerialName = ReadStringDescriptor(deviceHandle, result.DeviceDescriptor.SerialNumberIndex);
            if(result.SerialName == null)
            {
                throw new DeviceControlException("Failed to read serial name.", Marshal.GetLastWin32Error());
            }

            // Read file name.
            result.FileName = ReadStringDescriptor(deviceHandle, result.ConfigurationDescriptor.ConfigurationIndex);
            if(result.FileName == null)
            {
                throw new DeviceControlException("Failed to read file name.", Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Read the port address from a given device and save to
        /// a <see cref="UsbDeviceData"/> object.
        /// </summary>
        /// <param name="deviceHandle">The handle of the device to read from.</param>
        /// <param name="result">The device data that keeps all the results.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="deviceHandle"/> or <paramref name="result"/>
        /// is null.
        /// </exception>
        /// <exception cref="DeviceControlException">
        /// Thrown when device manager failed to read data from the device.
        /// </exception>
        private static void RetrievePortAddress(SafeFileHandle deviceHandle, UsbDeviceData result)
        {
            if(deviceHandle == null)
            {
                throw new ArgumentNullException(nameof(deviceHandle));
            }

            if(result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            // Read the port address.
            var buffer = new byte[sizeof(uint)];
            // Pin the buffer in order to receive data.
            var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                var success = Win32Methods.DeviceIoControl(deviceHandle,
                                                           IgtUsbIoControlCode.PortAddress,
                                                           null,
                                                           0,
                                                           gcHandle.AddrOfPinnedObject(),
                                                           sizeof(uint),
                                                           out _,
                                                           IntPtr.Zero);
                if(!success)
                {
                    throw new DeviceControlException("Failed to read port address.", Marshal.GetLastWin32Error());
                }
                result.PortAddress = buffer[0];
            }
            finally
            {
                gcHandle.Free();
            }
        }

        /// <summary>
        /// Read a general descriptor from the device to an output buffer
        /// whose size is calculated automatically based on its type.
        /// </summary>
        /// <remarks>
        /// Do not call this method if <paramref name="outBuffer"/> is a Byte array.
        /// Size of Byte array can not be calculated by Marshal.SizeOf.
        /// </remarks>
        /// <param name="deviceHandle">Handle of the device to read from.</param>
        /// <param name="type">Type of the descriptor to read.</param>
        /// <param name="outBuffer">The output buffer to store the result.</param>
        /// <returns>True if succeed, false otherwise.</returns>
        private static bool ReadGeneralDescriptor(SafeFileHandle deviceHandle,
                                                  UsbDescriptorType type,
                                                  object outBuffer)
        {
            return ReadGeneralDescriptor(deviceHandle,
                                         type,
                                         outBuffer,
                                         Marshal.SizeOf(outBuffer));
        }

        /// <summary>
        /// Read a general descriptor from the device to an output buffer
        /// with an explicitly specified size.
        /// </summary>
        /// <remarks>
        /// Call this method if <paramref name="outBuffer"/> is a Byte array.
        /// Size of Byte array can not be calculated by <see cref="Marshal.SizeOf(object)"/>.
        /// </remarks>
        /// <param name="deviceHandle">Handle of the device to read from.</param>
        /// <param name="type">Type of the descriptor to read.</param>
        /// <param name="outBuffer">The output buffer to store the result.</param>
        /// <param name="outBufferSize">Size of <paramref name="outBuffer"/>.</param>
        /// <returns>True if succeed, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="deviceHandle"/> or <paramref name="outBuffer"/>
        /// is null.
        /// </exception>
        private static bool ReadGeneralDescriptor(SafeFileHandle deviceHandle,
                                                  UsbDescriptorType type,
                                                  object outBuffer,
                                                  int outBufferSize)
        {
            if(deviceHandle == null)
            {
                throw new ArgumentNullException(nameof(deviceHandle));
            }

            if(outBuffer == null)
            {
                throw new ArgumentNullException(nameof(outBuffer));
            }

            bool success;

            // Pin the object first in order to receive data.
            var gcHandle = GCHandle.Alloc(outBuffer, GCHandleType.Pinned);

            try
            {
                // Input
                var generalRequest = new UsbGeneralDescriptorRequest { DescriptorType = type };

                // Read the general descriptor.
                success = Win32Methods.DeviceIoControl(deviceHandle,
                                                       IgtUsbIoControlCode.GetDescriptor,
                                                       generalRequest,
                                                       Marshal.SizeOf(generalRequest),
                                                       gcHandle.AddrOfPinnedObject(),
                                                       outBufferSize,
                                                       out _,
                                                       IntPtr.Zero);
            }
            finally
            {
                gcHandle.Free();
            }

            return success;
        }

        /// <summary>
        /// Read a string descriptor from the device.
        /// </summary>
        /// <param name="deviceHandle">Handle of the device to read from.</param>
        /// <param name="configurationIndex">Configuration index of the descriptor to read.</param>
        /// <returns>The descriptor retrieved.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="deviceHandle"/> is null.
        /// </exception>
        private static string ReadStringDescriptor(SafeFileHandle deviceHandle, byte configurationIndex)
        {
            if (deviceHandle == null)
            {
                throw new ArgumentNullException(nameof(deviceHandle));
            }

            string result = null;

            if(configurationIndex == 0)
            {
                result = string.Empty;
            }
            else
            {
                // Create a buffer for output.
                var descriptorString = new char[MaxStringSize];

                // Pin the buffer in order to receive data.
                var gcHandle = GCHandle.Alloc(descriptorString, GCHandleType.Pinned);

                try
                {
                    // Input
                    var stringRequest = new UsbStringDescriptorRequest
                                            {
                                                DescriptorType = UsbDescriptorType.String,
                                                LanaguageId = 0x409, // English
                                                ConfigurationIndex = configurationIndex
                                            };

                    // Read the string descriptor.
                    if(Win32Methods.DeviceIoControl(deviceHandle,
                                                    IgtUsbIoControlCode.GetDescriptor,
                                                    stringRequest,
                                                    Marshal.SizeOf(stringRequest),
                                                    gcHandle.AddrOfPinnedObject(),
                                                    MaxStringSize * sizeof(char),
                                                    out var bytesReturned,
                                                    IntPtr.Zero))
                    {
                        // The first Unicode character contains the descriptor length (1st byte) and type (2nd byte).
                        // The Unicode string starts from index 1.
                        result = new string(descriptorString, 1, bytesReturned / sizeof(char) - 1);
                    }
                }
                finally
                {
                    gcHandle.Free();
                }
            }

            return result;
        }

        #endregion

        #region Device Data Management

        /// <summary>
        /// Read the device data at the specified device path,
        /// and add it to the device data list.
        /// </summary>
        /// <param name="devicePath">Path to the device.</param>
        /// <returns>
        /// The device data just added.
        /// Null if the device is not supported by device manager.
        /// </returns>
        private UsbDeviceData AddDeviceData(string devicePath)
        {
            // The driver name in the device data is used as the key for
            // managing the device data list.  So we must make sure the
            // casing is unified.  Always make driver names in lowercase.
            var deviceData = ReadDeviceData(devicePath.ToLower());

            // Not all IGT class devices are supported by device manager.
            // Only add the supported ones to the list.
            if(deviceData.IsFeatureZero ||
               FeatureNumberToDeviceType.ContainsKey(deviceData.FunctionalDescriptor.FeatureNumber))
            {
                deviceDataList.Add(deviceData);
            }
            else
            {
                deviceData = null;
            }

            return deviceData;
        }

        /// <summary>
        /// Find the device data at the specified device path,
        /// and remove it from the device data list.
        /// </summary>
        /// <param name="devicePath">Path to the device.</param>
        /// <returns>
        /// The device data just removed.
        /// Null if the device data is not found.
        /// </returns>
        private UsbDeviceData RemoveDeviceData(string devicePath)
        {
            // Driver names are always in lowercase.
            var target = deviceDataList.FirstOrDefault(
                            deviceData => deviceData.DriverName == devicePath.ToLower());

            if(target != null)
            {
                deviceDataList.Remove(target);
            }

            return target;
        }

        #endregion

        #region Device Workgroup Management

        /// <summary>
        /// Create a Feature Zero, and add it to its corresponding workgroup.
        /// </summary>
        /// <param name="deviceData">The device data for the Feature Zero.</param>
        private void AddFeatureZero(UsbDeviceData deviceData)
        {
            var featureZero = new FeatureZero(deviceData);

            // Create the workgroup if it does not exist yet.
            var deviceWorkgroup = GetDeviceWorkgroup(featureZero.DriverAddress, true);

            deviceWorkgroup.AddDevice(featureZero);
        }

        /// <summary>
        /// Get the device workgroup by the driver address.
        /// </summary>
        /// <param name="driverAddress">
        /// The driver address of the target workgroup.
        /// </param>
        /// <returns>
        /// The device workgroup with the specified driver address.
        /// Null if one is not found.
        /// </returns>
        private DeviceWorkgroup GetDeviceWorkgroup(string driverAddress)
        {
            // ReSharper disable IntroduceOptionalParameters.Local

            return GetDeviceWorkgroup(driverAddress, false);

            // ReSharper restore IntroduceOptionalParameters.Local
        }

        /// <summary>
        /// Get the device workgroup by the driver address.
        /// </summary>
        /// <param name="driverAddress">
        /// The driver address of the target workgroup.
        /// </param>
        /// <param name="createNew">
        /// The flag indicating whether to create a new workgroup if the target one is not found.
        /// </param>
        /// <returns>
        /// The device workgroup with the specified driver address.
        /// Null if one is not found and <paramref name="createNew"/> is false.
        /// </returns>
        private DeviceWorkgroup GetDeviceWorkgroup(string driverAddress, bool createNew)
        {
            var result = deviceWorkgroups.FirstOrDefault(workgroup => workgroup.DriverAddress == driverAddress);

            if(result == null && createNew)
            {
                result = new DeviceWorkgroup(driverAddress);
                deviceWorkgroups.Add(result);
            }

            return result;
        }

        #endregion

        #region Device Connection Update

        /// <summary>
        /// Handle Device Change events from the device monitor.
        /// Simply enqueue the event, wait for <see cref="Update"/>
        /// to process it.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleDeviceChange(object sender, DeviceChangeEventArgs eventArgs)
        {
            lock(deviceChangeQueueLocker)
            {
                deviceChangeQueue.Enqueue(eventArgs);
            }
        }

        /// <summary>
        /// Process all pending device change events.
        /// </summary>
        private void ProcessDeviceChanges()
        {
            Queue<DeviceChangeEventArgs> localQueue = null;

            lock(deviceChangeQueueLocker)
            {
                if(deviceChangeQueue.Any())
                {
                    localQueue = deviceChangeQueue;
                    deviceChangeQueue = new Queue<DeviceChangeEventArgs>();
                }
            }

            if(localQueue != null)
            {
                while(localQueue.Any())
                {
                    var deviceChange = localQueue.Dequeue();

                    switch(deviceChange.Type)
                    {
                        case DeviceChangeType.Arrival:
                            ProcessDeviceArrival(deviceChange.DevicePath);
                            break;

                        case DeviceChangeType.Removal:
                            ProcessDeviceRemoval(deviceChange.DevicePath);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Process a device arrival.
        /// </summary>
        /// <param name="devicePath">Path to the device inserted.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="devicePath"/> is null.
        /// </exception>
        private void ProcessDeviceArrival(string devicePath)
        {
            if(devicePath == null)
            {
                throw new ArgumentNullException(nameof(devicePath));
            }

            // Read the device data and add to device data list.
            var deviceData = AddDeviceData(devicePath);

            // If it is a supported device...
            if(deviceData != null)
            {
                if(deviceData.IsFeatureZero)
                {
                    AddFeatureZero(deviceData);
                }
                else
                {
                    // Raise corresponding event.
                    DeviceConnectedEvent?.Invoke(this,
                        new DeviceConnectedEventArgs(
                            FeatureNumberToDeviceType[deviceData.FunctionalDescriptor.FeatureNumber],
                            deviceData.InterfaceName));
                }
            }
        }

        /// <summary>
        /// Process a device removal.
        /// </summary>
        /// <param name="devicePath">Path to the device removed.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="devicePath"/> is null.
        /// </exception>
        private void ProcessDeviceRemoval(string devicePath)
        {
            if(devicePath == null)
            {
                throw new ArgumentNullException(nameof(devicePath));
            }

            // Find the corresponding device data and remove it from device data list.
            var deviceData = RemoveDeviceData(devicePath);

            // If one is removed...
            if(deviceData != null)
            {
                if(deviceData.IsFeatureZero)
                {
                    // Removing Feature Zero removes the workgroup.
                    var driverAddress = DeviceBase.ComputeDriverAddress(deviceData.DriverName);
                    var deviceWorkgroup = GetDeviceWorkgroup(driverAddress);

                    if(deviceWorkgroup != null)
                    {
                        // Clear the workgroup first.  This stops Feature Zero.
                        deviceWorkgroup.Clear();
                        deviceWorkgroups.Remove(deviceWorkgroup);
                    }
                }
                else
                {
                    // Raise corresponding event.
                    DeviceRemovedEvent?.Invoke(this,
                        new DeviceRemovedEventArgs(
                            FeatureNumberToDeviceType[deviceData.FunctionalDescriptor.FeatureNumber],
                            deviceData.InterfaceName));
                }
            }
        }

        #endregion

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Dispose unmanaged and disposable resources held by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose unmanaged and disposable resources held by this object.
        /// </summary>
        /// <param name="disposing">True if called from dispose function.</param>
        private void Dispose(bool disposing)
        {
            if(!disposed && disposing)
            {
                // Stop device monitor.
                DeviceMonitor.Stop();

                // Release all event subscriptions.
                DeviceConnectedEvent = null;
                DeviceRemovedEvent = null;

                // Clear all device workgroups, which stops all Feature Zero threads.
                foreach(var deviceWorkgroup in deviceWorkgroups)
                {
                    deviceWorkgroup.Clear();
                }
            }

            disposed = true;
        }

        #endregion

        #endregion
    }
}