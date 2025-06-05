//-----------------------------------------------------------------------
// <copyright file = "IgtUsbStatus.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    #pragma warning disable 1591
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Status code for IGT USB devices.
    /// </summary>
    /// <remarks>
    /// Copied from AVP code: projects\WindowsSim\IGTUSBDriverDLL\inc\status.h
    /// </remarks>
    internal enum IgtUsbStatus : uint
    {
        OK                       = 0,
        IGT_USB_STATUS_MASK      = 0xE0100000,
        CRC                      = 0xE0100001,
        BTSTUFF                  = 0xE0100002,
        DATA_TOGGLE_MISMATCH     = 0xE0100003,
        STALL_PID                = 0xE0100004,
        DEV_NOT_RESPONDING       = 0xE0100005,
        PID_CHECK_FAILURE        = 0xE0100006,
        UNEXPECTED_PID           = 0xE0100007,
        DATA_OVERRUN             = 0xE0100008,
        DATA_UNDERRUN            = 0xE0100009,
        BUFFER_OVERRUN           = 0xE010000C,
        BUFFER_UNDERRUN          = 0xE010000D,
        NOT_ACCESSED             = 0xE010000F,
        FIFO                     = 0xE0100010,
        ENDPOINT_HALTED          = 0xE0100030,
        NO_MEMORY                = 0xE0100100,
        INVALID_URB_FUNCTION     = 0xE0100200,
        INVALID_PARAMETER        = 0xE0100300,
        ERROR_BUSY               = 0xE0100400,
        REQUEST_FAILED           = 0xE0100500,
        INVALID_PIPE_HANDLE      = 0xE0100600,
        NO_BANDWIDTH             = 0xE0100700,
        INTERNAL_HC_ERROR        = 0xE0100800,
        ERROR_SHORT_TRANSFER     = 0xE0100900,
        BAD_START_FRAME          = 0xE0100A00,
        ISOCH_REQUEST_FAILED     = 0xE0100B00,
        FRAME_CONTROL_OWNED      = 0xE0100C00,
        FRAME_CONTROL_NOT_OWNED  = 0xE0100D00,
        CANCELED                 = 0xE0110000,
        CANCELING                = 0xE0120000,
        ALREADY_CONFIGURED       = 0xE0110001,
        UNCONFIGURED             = 0xE0110002,
        NO_SUCH_DEVICE           = 0xE01F0002,
        DEVICE_NOT_FOUND         = 0xE01F0003,
        NOT_SUPPORTED            = 0xE01F0005,
        IO_PENDING               = 0xE01F0006,
        IO_TIMEOUT               = 0xE01F0007,
        DEVICE_REMOVED           = 0xE01F0008,
        PIPE_NOT_LINKED          = 0xE01F0009,
        CONNECTED_PIPES          = 0xE01F000A,
        DEVICE_LOCKED            = 0xE01F0010
    }

    // ReSharper restore InconsistentNaming
    #pragma warning restore 1591
}
