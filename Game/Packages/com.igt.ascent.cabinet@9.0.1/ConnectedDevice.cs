//-----------------------------------------------------------------------
// <copyright file = "ConnectedDevice.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class that contains information of a connected device.
    /// </summary>
    public class ConnectedDevice
    {
        /// <summary>
        /// The identifier of the device.
        /// </summary>
        public DeviceIdentifier Identifier { get; }

        /// <summary>
        /// A list of groups of a device if available.
        /// </summary>
        public IList<uint> GroupList { get; }

        /// <summary>
        ///  Construct an instance of the ConnectedDevice.
        /// </summary>
        /// <param name="identifier">The identifier of the device.</param>
        /// <param name="groupList">A list of groups of a device if available.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if paramref name="groupList"/> is null.
        /// </exception>
        public ConnectedDevice(DeviceIdentifier identifier, IList<uint> groupList)
        {
            Identifier = identifier;
            GroupList = groupList ?? throw new ArgumentNullException(nameof(groupList));
        }
    }
}