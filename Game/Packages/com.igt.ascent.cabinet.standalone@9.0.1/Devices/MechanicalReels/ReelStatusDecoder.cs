//-----------------------------------------------------------------------
// <copyright file = "ReelStatusDecoder.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.MechanicalReels
{
    using System;
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// This class decodes an integer reel status code, and
    /// provide the reel number and reel status as the results.
    /// </summary>
    [Serializable]
    internal class ReelStatusDecoder
    {
        #region Constants
        // Constants are implemented as integers instead of enum for
        // the sake of easier bit mask operation.

        /// <summary>
        /// Mask to extract the reel number from a reel status code.
        /// </summary>
        private const ushort ReelNumberMask = 0x0F00;

        /// <summary>
        /// Inverse of <see cref="ReelNumberMask"/>, used
        /// to zero out the reel number in a reel status code.
        /// </summary>
        private const ushort ReelNumberBlankMask = 0xF0FF;

        /// <summary>
        /// Mask to extract the upper status code that
        /// does not have reel number or stop position.
        /// </summary>
        private const ushort StatusMask = 0xF000;

        /// <summary>
        /// A table used to translate a <see cref="ReelStatusCode"/> value
        /// to a <see cref="ReelStatus"/> value.
        /// </summary>
        private static readonly Dictionary<ReelStatusCode, ReelStatus> TranslationTable = 
            new Dictionary<ReelStatusCode, ReelStatus>
                {
                    {ReelStatusCode.Stopped, ReelStatus.Stopped},
                    {ReelStatusCode.StoppedAtUnknownStop, ReelStatus.Stopped},
                    {ReelStatusCode.Accelerating, ReelStatus.Accelerating},
                    {ReelStatusCode.Decelerating, ReelStatus.Decelerating},
                    {ReelStatusCode.ConstantSpeed, ReelStatus.ConstantSpeed},
                    {ReelStatusCode.MovingIrregularly, ReelStatus.MovingIrregularly}
                };

        #endregion

        #region Properties

        /// <summary>
        /// Get the flag indicating if the code for decoding
        /// is a valid reel status code.
        /// </summary>
        public bool IsValidStatusCode { get; private set; }

        /// <summary>
        /// Get the <see cref="ReelStatusCode"/> value decoded
        /// from the given integer code, if it is valid.
        /// </summary>
        public ReelStatusCode ReelStatusCode { get; private set; }

        /// <summary>
        /// Get the flag indicating if the decoded reel status code
        /// corresponds to a <see cref="ReelStatus"/> value.
        /// </summary>
        public bool IsDefinedStatus { get; private set; }

        /// <summary>
        /// Get the <see cref="ReelStatus"/> value corresponding
        /// to the decoded reel status code, if one is defined.
        /// </summary>
        public ReelStatus ReelStatus { get; private set; }

        /// <summary>
        /// Get the reel number decoded from the given
        /// reel status code, if it is valid.
        /// </summary>
        public byte ReelNumber { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize an instance of <see cref="ReelStatusDecoder"/>,
        /// which decodes the given integer code, and set its properties
        /// accordingly as the decoding results.
        /// </summary>
        /// <param name="statusCode">The code to interpret.</param>
        public ReelStatusDecoder(ushort statusCode)
        {
            // Extract and validate reel number.
            var reelNumber = (byte)((statusCode & ReelNumberMask) >> 8);

            // Remove reel number from the status.
            statusCode &= ReelNumberBlankMask;

            // Check if it is a Stopped status code.
            if((statusCode & StatusMask) == (int)ReelStatusCode.Stopped)
            {
                statusCode = (ushort)ReelStatusCode.Stopped;
            }

            // Validate the status code.
            IsValidStatusCode = Enum.IsDefined(typeof(ReelStatusCode), statusCode);

            // If it is valid, record the results.
            if(IsValidStatusCode)
            {
                ReelStatusCode = (ReelStatusCode)statusCode;
                ReelNumber = reelNumber;

                //Check if it corresponds to a defined reel status.
                IsDefinedStatus = TranslationTable.ContainsKey(ReelStatusCode);
                if(IsDefinedStatus)
                {
                    ReelStatus = TranslationTable[ReelStatusCode];
                }
            }
        }

        #endregion
    }
}
