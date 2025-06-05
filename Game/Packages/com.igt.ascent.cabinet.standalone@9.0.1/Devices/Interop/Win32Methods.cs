//-----------------------------------------------------------------------
// <copyright file = "Win32Methods.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// This class provides the import declarations of Win32 methods,
    /// as well as helper functions to facilitate the inter-op calls.
    /// Part of the implementation is based on code pulled from
    /// http://www.pinvoke.net
    /// </summary>
    internal static class Win32Methods
    {
        #region kernel32.dll

        /// <remarks>
        /// The arguments of IntPtr type are expected to be IntPtr.Zero,
        /// corresponding to the NULL pointers in native method calls.
        /// </remarks>>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(string lpFileName,
                                                       EFileAccess dwDesiredAccess,
                                                       EFileShare dwShareMode,
                                                       IntPtr lpSecurityAttributes,
                                                       ECreationDisposition dwCreationDisposition,
                                                       EFileAttributes dwFlagsAndAttributes,
                                                       IntPtr hTemplateFile);

        /// <remarks>
        /// This version is used for getting data back from the device driver.
        /// <paramref name="lpOutBuffer"/> refers to the address of a managed
        /// object that is pinned by a GCHandle.
        /// </remarks>>
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeviceIoControl(SafeFileHandle hDevice,
                                                  IgtUsbIoControlCode dwIoControlCode,
                                                  [MarshalAs(UnmanagedType.AsAny)]
                                                  object lpInBuffer,
                                                  int nInBufferSize,
                                                  IntPtr lpOutBuffer,
                                                  int nOutBufferSize,
                                                  out int lpBytesReturned,
                                                  IntPtr lpOverlapped);

        /// <remarks>
        /// This version is used for sending data to the device driver, using
        /// both <paramref name="lpInBuffer"/> and <paramref name="lpOutBuffer"/>
        /// as input.
        /// </remarks>>
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeviceIoControl(SafeFileHandle hDevice,
                                                  IgtUsbIoControlCode dwIoControlCode,
                                                  [MarshalAs(UnmanagedType.AsAny)]
                                                  object lpInBuffer,
                                                  int nInBufferSize,
                                                  byte[] lpOutBuffer,
                                                  int nOutBufferSize,
                                                  out int lpBytesReturned,
                                                  IntPtr lpOverlapped);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool ReadFile(SafeFileHandle hFile,
                                           [Out]
                                           byte[] lpBuffer,
                                           int nNumberOfBytesToRead,
                                           out int lpNumberOfBytesRead,
                                           IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        #endregion

        #region setupapi.dll

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid,
                                                        IntPtr enumerator,
                                                        IntPtr hwndParent,
                                                        DiGetClassFlags flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet,
                                                                 IntPtr deviceInfoData,
                                                                 ref Guid interfaceClassGuid,
                                                                 uint memberIndex,
                                                                 ref DeviceInterfaceData deviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet,
                                                                     ref DeviceInterfaceData deviceInterfaceData,
                                                                     ref DeviceInterfaceDetailData deviceInterfaceDetailData,
                                                                     int deviceInterfaceDetailDataSize,
                                                                     out int requiredSize,
                                                                     IntPtr deviceInfoData);

        #endregion

        #region user32.dll

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient,
                                                               [MarshalAs(UnmanagedType.AsAny)]
                                                               object notificationFilter,
                                                               uint flags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterDeviceNotification(IntPtr handle);


        #endregion

        #region Helper Functions

        /// <summary>
        /// Convert the signed integer error code returned by <see cref="Marshal.GetLastWin32Error"/>
        /// to a meaningful unsigned integer.
        /// </summary>
        /// <param name="errorCode">The signed integer error code.</param>
        /// <returns>
        /// The unsigned integer error code, whose high word indicates
        /// the component generating the error, and low word contains
        /// the Windows error code defined in winerror.h.
        /// </returns>
        public static uint ProcessErrorCode(int errorCode)
        {
            return uint.MaxValue - (uint)~errorCode;
        }

        /// <summary>
        /// Retrieve the Windows error code from an unsigned error code.
        /// </summary>
        /// <param name="processedErrorCode">
        /// The unsigned integer error code, whose high word indicates
        /// the component generating the error, and low word contains
        /// the Windows error code defined in winerror.h.
        /// </param>
        /// <returns>The Windows error code as defined in winerror.h.</returns>
        public static uint RetrieveWinErrorValue(uint processedErrorCode)
        {
            return processedErrorCode << 16 >> 16;
        }

        /// <summary>
        /// Map a byte array to a a newly allocated managed object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the target object.</typeparam>
        /// <param name="buffer">The byte array from which the managed object is mapped.</param>
        /// <param name="offset">The offset in the array where the target data starts.</param>
        /// <returns>The object mapped from the byte array.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="buffer"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <typeparamref name="T"/> does not have sequential layout.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="buffer"/> starting at <paramref name="offset"/>
        /// is not big enough to unpack data type <typeparamref name="T"/>.
        /// </exception>
        public static T Unpack<T>(byte[] buffer, int offset) where T : new()
        {
            if(buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            var structType = typeof(T);

            // Make sure the target struct has sequential layout.
            if(!structType.IsLayoutSequential)
            {
                throw new ArgumentException("The type to be marshaled must have sequential layout.");
            }

            var result = new T();

            // Make sure there is enough buffer.
            if(buffer.Length - offset < Marshal.SizeOf(result))
            {
                throw new ArgumentOutOfRangeException(
                    $"There is no enough buffer to unpack {structType.Name} with array length of {buffer.Length} and offset {offset}");
            }

            var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                if(offset == 0)
                {
                    result = (T)Marshal.PtrToStructure(pinnedBuffer.AddrOfPinnedObject(), structType);
                }
                else
                {
                    result = (T)Marshal.PtrToStructure(Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset), structType);
                }
            }
            finally
            {
                pinnedBuffer.Free();
            }

            return result;
        }

        #endregion
    }
}