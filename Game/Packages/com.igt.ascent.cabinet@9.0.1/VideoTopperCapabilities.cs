//-----------------------------------------------------------------------
// <copyright file = "VideoTopperCapabilities.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System.Collections.Generic;

    /// <summary>
    /// Enumeration containing portal device support capability.
    /// </summary>
    public enum VideoTopperPortalSupport
    {
        /// <summary>
        /// Unknown portal device support capability.
        /// </summary>
        Unknown,

        /// <summary>
        /// Portal device is supported.
        /// </summary>
        PortalSupported,

        /// <summary>
        /// Portal device is not supported.
        /// </summary>
        PortalUnsupported
    }

    /// <summary>
    /// Contains the supported media informations.
    /// </summary>
    public struct SupportedMedia
    {
        /// <summary>
        /// Type of MIME supported.
        /// </summary>
        public string MimeType;

        /// <summary>
        /// Extension of the file.
        /// </summary>
        public string FileExtension;
    }

    /// <summary>
    /// Contains the video topper device capabilities.
    /// </summary>
    public class VideoTopperCapabilities
    {
        /// <summary>
        /// Width of the video topper.
        /// </summary>
        public uint Width { get; }

        /// <summary>
        /// Height of the video topper.
        /// </summary>
        public uint Height { get; }

        /// <summary>
        /// Types of media supported by the video topper.
        /// </summary>
        public List<SupportedMedia> SupportedMedia { get; }

        /// <summary>
        /// Portal device support capability.
        /// </summary>
        public VideoTopperPortalSupport SupportsPortals { get; }

        /// <summary>
        /// Initializes all the video topper capabilities.
        /// </summary>
        /// <param name="width">Width of the video topper.</param>
        /// <param name="height">Height of the video topper.</param>
        /// <param name="supportedMedia">Types of media supported by the video topper.</param>
        /// <param name="supportsPortals">Portal device support capability.</param>
        public VideoTopperCapabilities(uint width, uint height, List<SupportedMedia> supportedMedia, VideoTopperPortalSupport supportsPortals)
        {
            Width = width;
            Height = height;
            SupportedMedia = supportedMedia;
            SupportsPortals = supportsPortals;
        }
    }
}
