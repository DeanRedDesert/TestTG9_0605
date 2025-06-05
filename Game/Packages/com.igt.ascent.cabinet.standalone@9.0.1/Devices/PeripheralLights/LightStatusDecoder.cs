//-----------------------------------------------------------------------
// <copyright file = "LightStatusDecoder.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.PeripheralLights
{
    using System;

    /// <summary>
    /// This class decodes an integer light status code, and
    /// provide the light group and light status as the results.
    /// </summary>
    [Serializable]
    internal class LightStatusDecoder
    {
        #region Constants
        // Constants are implemented as integers instead of enum for
        // the sake of easier bit mask operation.

        /// <summary>
        /// Mask to extract the light group number from a reel status code.
        /// </summary>
        private const ushort GroupNumberMask = 0x00FF;

        /// <summary>
        /// Inverse of <see cref="GroupNumberMask"/>, used
        /// to zero out the group number in a light status code.
        /// </summary>
        private const ushort GroupNumberBlankMask = 0xFF00;

        #endregion

        #region Properties

        /// <summary>
        /// Get the flag indicating if the code for decoding
        /// is a valid light status code.
        /// </summary>
        public bool IsValidStatusCode { get; private set; }

        /// <summary>
        /// Get the <see cref="LightStatusCode"/> value decoded
        /// from the given integer code, if it is valid.
        /// </summary>
        public LightStatusCode LightStatusCode { get; private set; }

        /// <summary>
        /// Get the light group number decoded from the given
        /// light status code, if it is valid.
        /// </summary>
        public byte GroupNumber { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize an instance of <see cref="LightStatusDecoder"/>,
        /// which decodes the given integer code, and set its properties
        /// accordingly as the decoding results.
        /// </summary>
        /// <param name="statusCode">The code to interpret.</param>
        public LightStatusDecoder(ushort statusCode)
        {
            // Extract and validate group number.
            var groupNumber = (byte)(statusCode & GroupNumberMask);

            // Remove group number from the status.
            statusCode &= GroupNumberBlankMask;

            // Validate the status code.
            IsValidStatusCode = Enum.IsDefined(typeof(LightStatusCode), statusCode);

            // If it is valid, record the results.
            if(IsValidStatusCode)
            {
                LightStatusCode = (LightStatusCode)statusCode;
                GroupNumber = groupNumber;
            }
        }

        #endregion
    }
}
