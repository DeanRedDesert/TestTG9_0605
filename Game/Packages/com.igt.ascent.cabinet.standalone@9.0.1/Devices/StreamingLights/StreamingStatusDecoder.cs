//-----------------------------------------------------------------------
// <copyright file = "StreamingStatusDecoder.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.StreamingLights
{
    using System;

    /// <summary>
    /// Decodes the status code into streaming lights status information.
    /// </summary>
    internal class StreamingStatusDecoder
    {
        private const ushort StatusCodeMask = 0xFF00;
        private const ushort GroupNumberMask = 0x00FF;

        /// <summary>
        /// Construct a new instance using the status code.
        /// </summary>
        /// <param name="statusCode">The device status code to decode.</param>
        public StreamingStatusDecoder(ushort statusCode)
        {
            GroupNumber = (byte)(statusCode & GroupNumberMask);

            var status = statusCode & StatusCodeMask;
            
            if(Enum.IsDefined(typeof(StreamingStatusCode), (ushort)status))
            {
                IsValidStatusCode = true;
                StatusCode = (StreamingStatusCode)status;
            }
        }

        /// <summary>
        /// Gets if the status code information is valid or not.
        /// </summary>
        public bool IsValidStatusCode
        {
            get;
        }

        /// <summary>
        /// Gets the status code.
        /// </summary>
        public StreamingStatusCode StatusCode
        {
            get;
        }

        /// <summary>
        /// Gets the group number the status code is for.
        /// </summary>
        public byte GroupNumber
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return IsValidStatusCode 
                ? $"Group: {GroupNumber} Status: {StatusCode}"
                : "Invalid Status Code";
        }
    }
}
