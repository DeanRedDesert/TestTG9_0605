//-----------------------------------------------------------------------
// <copyright file = "IgtUsbIoControlCode.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.Interop
{
    using System;

#pragma warning disable 1591

    /// <summary>
    /// Enumeration indicating the type of command for
    /// IGT class USB devices' IO control.
    /// </summary>
    [Serializable]
    internal enum IgtUsbIoControlCode : uint
    {
        GetDescriptor        = (EFileDevice.IgtUsb << 16) | (0x0801 << 2) | EMethod.Buffered,
        GetConfiguration     = (EFileDevice.IgtUsb << 16) | (0x0802 << 2) | EMethod.Buffered,
        SetConfiguration     = (EFileDevice.IgtUsb << 16) | (0x0803 << 2) | EMethod.Buffered,
        FeatureControl       = (EFileDevice.IgtUsb << 16) | (0x0806 << 2) | EMethod.Buffered,
        GetStatus            = (EFileDevice.IgtUsb << 16) | (0x0807 << 2) | EMethod.Buffered,
        SetInterface         = (EFileDevice.IgtUsb << 16) | (0x0808 << 2) | EMethod.Buffered,
        ClassOrVendorRequest = (EFileDevice.IgtUsb << 16) | (0x0809 << 2) | EMethod.OutDirect,
        CyclePort            = (EFileDevice.IgtUsb << 16) | (0x080A << 2) | EMethod.Buffered,
        UnconfigureDevice    = (EFileDevice.IgtUsb << 16) | (0x080B << 2) | EMethod.Buffered,
        ResetDevice          = (EFileDevice.IgtUsb << 16) | (0x080C << 2) | EMethod.Buffered,
        LockDevice           = (EFileDevice.IgtUsb << 16) | (0x080D << 2) | EMethod.Buffered,
        ResetPipe            = (EFileDevice.IgtUsb << 16) | (0x0810 << 2) | EMethod.Buffered,
        LinkPipe             = (EFileDevice.IgtUsb << 16) | (0x0811 << 2) | EMethod.Buffered,
        GetHandle            = (EFileDevice.IgtUsb << 16) | (0x0812 << 2) | EMethod.Buffered,
        GetInterface         = (EFileDevice.IgtUsb << 16) | (0x0813 << 2) | EMethod.Buffered,
        PortAddress          = (EFileDevice.IgtUsb << 16) | (0x0814 << 2) | EMethod.Buffered
    }

    #pragma warning restore 1591
}
