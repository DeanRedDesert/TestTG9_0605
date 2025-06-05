//-----------------------------------------------------------------------
// <copyright file = "UnsupportedLightSequenceVersionException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming
{
    using System;

    /// <summary>
    /// Represents errors when trying to read a light sequence file that
    /// is a version newer than the library supports.
    /// </summary>
    [Serializable]
    public class UnsupportedLightSequenceVersionException : Exception
    {
        private const string MessageString = "Light sequence version {0} is unsupported. Maximum supported version: {1}.";

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="detectedVersion">The detected light sequence version number.</param>
        /// <param name="supportedVersion">The maximum supported version number.</param>
        public UnsupportedLightSequenceVersionException(ushort detectedVersion, ushort supportedVersion)
            : base(string.Format(MessageString, detectedVersion, supportedVersion))
        {
            SupportedVersion = supportedVersion;
            DetectedVersion = detectedVersion;
        }

        /// <summary>
        /// The maximum supported sequence version.
        /// </summary>
        public ushort SupportedVersion
        {
            get;
            private set;
        }

        /// <summary>
        /// The detected sequence version.
        /// </summary>
        public ushort DetectedVersion
        {
            get;
            private set;
        }
    }
}
