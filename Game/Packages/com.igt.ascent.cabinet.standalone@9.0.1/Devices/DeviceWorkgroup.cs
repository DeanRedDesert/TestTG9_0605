//-----------------------------------------------------------------------
// <copyright file = "DeviceWorkgroup.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IgtUsbDevice;
    using UsbFeatureZero;

    /// <summary>
    /// This class represents a group of devices that share the
    /// same driver address.
    /// </summary>
    internal class DeviceWorkgroup
    {
        /// <summary>
        /// Get the address of the device driver.
        /// </summary>
        /// <remarks>
        /// This string can be viewed as the identifier of a communication
        /// channel, which could be shared by multiple devices, among which
        /// there should be one Feature Zero present, which is responsible
        /// for status reading for all devices at the same address.
        /// </remarks>
        public string DriverAddress { get; }

        /// <summary>
        /// The Feature Zero that is responsible for reading statuses for
        /// all devices in the group.
        /// </summary>
        private FeatureZero featureZero;

        /// <summary>
        /// A list of non-Feature Zero devices.
        /// </summary>
        private readonly List<DeviceBase> devices = new List<DeviceBase>();

        /// <summary>
        /// Object for synchronizing access to message queue.
        /// </summary>
        private readonly object messageQueueLocker = new object();

        /// <summary>
        /// The queue of USB message received from the feature zero.
        /// </summary>
        private Queue<UsbMessageEventArgs> messageQueue = new Queue<UsbMessageEventArgs>();

        /// <summary>
        /// Object for synchronizing access to polling requests.
        /// </summary>
        private readonly object pollingRequestsLocker = new object();

        /// <summary>
        /// A list of devices, identified by their interface numbers,
        /// that have requested an active polling.
        /// </summary>
        private HashSet<byte> pollingRequests = new HashSet<byte>();

        /// <summary>
        /// Initialize an instance of <see cref="DeviceWorkgroup"/>.
        /// </summary>
        /// <param name="driverAddress">
        /// The driver address shared by all devices in the workgroup.
        /// </param>
        public DeviceWorkgroup(string driverAddress)
        {
            DriverAddress = driverAddress;
        }

        /// <summary>
        /// Add a device to the workgroup.
        /// </summary>
        /// <param name="device">The device to add.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="device"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="device"/> has a different driver
        /// address than the workgroup, or is a <see cref="FeatureZero"/>,
        /// but there is already a Feature Zero present in the workgroup.
        /// </exception>
        public void AddDevice(DeviceBase device)
        {
            if(device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if(device.DriverAddress != DriverAddress)
            {
                throw new InvalidOperationException(
                    "Device must have the same driver address as the workgroup in order to be added.");
            }

            if(device is FeatureZero)
            {
                if(featureZero != null)
                {
                    throw new InvalidOperationException(
                        "Feature Zero in the device workgroup has already been assigned.");
                }

                featureZero = device as FeatureZero;

                featureZero.UsbMessageEvent += HandleUsbMessage;

                // If there is a non-Feature Zero device present,
                // start the feature zero.
                if(devices.Any())
                {
                    featureZero.Start();
                }
            }
            else
            {
                device.DeviceRequestPollingEvent += HandlePollingRequest;

                devices.Add(device);

                // If this is the first device added, and the feature zero is present,
                // start the feature zero.
                if(devices.Count == 1)
                {
                    featureZero?.Start();
                }
            }
        }

        /// <summary>
        /// Remove a device from the workgroup.
        /// </summary>
        /// <param name="device">The device to remove.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="device"/> is null.
        /// </exception>
        public void RemoveDevice(DeviceBase device)
        {
            if(device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            // Feature Zero cannot be removed.
            // It is removed when the workgroup is removed.
            if(device is FeatureZero)
                return;

            device.DeviceRequestPollingEvent -= HandlePollingRequest;

            devices.Remove(device);

            // If all non-Feature Zero devices have been removed,
            // stop the feature zero.
            if(!devices.Any())
            {
                featureZero?.Stop();
            }
        }

        /// <summary>
        /// Clear all devices from the workgroup and
        /// stop the feature zero thread.  This must
        /// be called before removing a workgroup.
        /// </summary>
        public void Clear()
        {
            if(featureZero != null)
            {
                featureZero.UsbMessageEvent -= HandleUsbMessage;
                featureZero.Stop();
                featureZero = null;
            }

            foreach(var device in devices)
            {
                device.DeviceRequestPollingEvent -= HandlePollingRequest;
            }

            devices.Clear();
        }

        /// <summary>
        /// Check and process USB messages, and poll devices as requested.
        /// </summary>
        public void Update()
        {
            featureZero?.KeepAlive();

            ProcessUsbMessages();
            ProcessPollingRequests();
        }

        /// <summary>
        /// Handle USB Message event from the feature zero.
        /// Simply enqueue the event, wait for <see cref="Update"/>
        /// to process it.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleUsbMessage(object sender, UsbMessageEventArgs eventArgs)
        {
            lock(messageQueueLocker)
            {
                messageQueue.Enqueue(eventArgs);
            }
        }

        /// <summary>
        /// Process all pending USB messages sent by the feature zero.
        /// </summary>
        private void ProcessUsbMessages()
        {
            Queue<UsbMessageEventArgs> localQueue = null;

            lock(messageQueueLocker)
            {
                if(messageQueue.Any())
                {
                    localQueue = messageQueue;
                    messageQueue = new Queue<UsbMessageEventArgs>();
                }
            }

            if(localQueue != null)
            {
                while(localQueue.Any())
                {
                    var usbMessage = localQueue.Dequeue();

                    DeviceBase recipient;

                    // Find the recipient of the message using the interface number.
                    if(featureZero != null && featureZero.InterfaceNumber == usbMessage.InterfaceNumber)
                    {
                        recipient = featureZero;
                    }
                    else
                    {
                        recipient = devices.FirstOrDefault(
                                       device => device.InterfaceNumber == usbMessage.InterfaceNumber);
                    }

                    // Handle the message.
                    recipient?.HandleMessage(usbMessage);
                }
            }
        }

        /// <summary>
        /// Handle Device Request Polling event from the devices.
        /// Simply enqueue the device's interface number, wait
        /// for <see cref="Update"/> to process it.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandlePollingRequest(object sender, DeviceRequestPollingEventArgs eventArgs)
        {
            lock(pollingRequestsLocker)
            {
                pollingRequests.Add(eventArgs.InterfaceNumber);
            }
        }

        /// <summary>
        /// Process all pending polling requests sent by the devices.
        /// </summary>
        private void ProcessPollingRequests()
        {
            HashSet<byte> localHashSet= null;

            lock(pollingRequests)
            {
                if(pollingRequests.Any())
                {
                    localHashSet = pollingRequests;
                    pollingRequests = new HashSet<byte>();
                }
            }

            if(localHashSet != null)
            {
                foreach(var interfaceNumber in localHashSet)
                {
                    var requester = devices.FirstOrDefault(device => device.InterfaceNumber == interfaceNumber);

                    requester?.PollDevice();
                }
            }
        }
    }
}
