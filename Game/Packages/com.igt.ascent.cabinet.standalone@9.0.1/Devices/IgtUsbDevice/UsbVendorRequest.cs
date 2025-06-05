//-----------------------------------------------------------------------
// <copyright file = "UsbVendorRequest.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.Runtime.InteropServices;

#pragma warning disable 1591

    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    internal class UsbVendorRequest
    {
        public ushort Target;
        public Description Type;
        public byte ReservedBits;
        public RequestCode Request;
        public ushort Value;

        [Serializable]
        public enum Description : byte
        {
            VendorRequest = 2,
            RequestInMask = 0x80,

            RequestOut = VendorRequest,
            RequestIn = VendorRequest | RequestInMask
        }

        [Serializable]
        public enum RequestCode : byte
        {
            Command = 0,
            Query = 1
        }
    }

    #pragma warning restore 1591
}
