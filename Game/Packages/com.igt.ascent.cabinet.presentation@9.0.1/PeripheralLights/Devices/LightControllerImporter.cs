//-----------------------------------------------------------------------
// <copyright file = "LightControllerImporter.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Globalization;
    using System.IO;
    using Communication.Cabinet;

    /// <summary>
    /// Parses the .light file to extract light sequence information.
    /// </summary>
    /// <remarks>Requires the format version to be format version 1.
    /// And requires hardware (i.e. Controller type 3: Light bars or WideScreen)
    /// to support 6 bits color animation data since only 6 bits color animation data
    /// are constructed.</remarks>
    public class LightControllerImporter
    {
        #region Public Data Members

        /// <summary>
        /// The <see cref="MainLightControllerData"/> object.
        /// </summary>
        public MainLightControllerData MainLightControllerData { get; private set; }

        #endregion

        #region Private Data Members

        /// <summary>
        /// A tag used in the .light file to verify that the file contains the correct header.
        /// </summary>
        private const string FormTag = "FORM";

        /// <summary>
        /// The identifier indicating that the .light file contains light controller pattern data.
        /// </summary>
        private const string LbzlIdentifier = "LBZL";

        /// <summary>
        /// Light file format version supported.
        /// </summary>
        private const ushort VersionSupported = 1;

        #endregion

        #region Public Methods

        /// <summary>
        /// Parse the specified .light file.
        /// </summary>
        /// <param name="file">The specified .light file to parse.</param>
        /// <exception cref="IOException">Thrown if the file can't be open.</exception>
        /// <remarks>Dispose objects before losing scope is suppressed because the file object is closed when the BinaryReader object
        /// is also closed. </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void ParseFile(string file)
        {
            if(File.Exists(file))
            {
                // There is only one main header (i.e. FORM form) in the file.
                // BinaryReader reads in the UInt32 and UInt16 data types in little-endian format.
                using(var binReader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read)))
                {
                    binReader.BaseStream.Seek(0, SeekOrigin.Begin);

                    ReadMainHeaderData(binReader);
                }
            }
            else
            {
                throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture,
                    "The file {0} does not exist.", file));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Reads a specified number of characters from the binary reader stream.
        /// </summary>
        /// <remarks>The number of characters to read must be provided since .NET 3.5 doesn't support default parameters.</remarks>
        /// <param name="binReader">Reads primitive data types as binary values in a specific character encoding.</param>
        /// <param name="count">Number of characters to read.</param>
        /// <returns>The string read from the .light file.</returns>
        private static string ReadString(BinaryReader binReader, int count)
        {
            return new string(binReader.ReadChars(count));
        }

        #region Read the main header data

        /// <summary>
        /// Read in main header data.
        /// </summary>
        /// <param name="binReader">Reads primitive data types as binary values in a specific character encoding.</param>
        /// <exception cref="InvalidLightFileFormatException">Thrown if the file doesn't contain a FORM tag or if
        /// the LBZL identifier is missing from the file.</exception>
        private void ReadMainHeaderData(BinaryReader binReader)
        {
            var formTag = ReadString(binReader, 4);

            // The FORM tag must be present in the file for further parsing.
            if(formTag != FormTag)
            {
                throw new InvalidLightFileFormatException(string.Format(CultureInfo.InvariantCulture, "The FORM tag is missing in the .light file read."));
            }

            // The size field is ordered LSB first.
            // ReadUInt32() reads in an integer in little endian format, and
            // reverses the byte order.
            // Read in the data size.
            binReader.ReadUInt32();
 
            // Read in the form identifier.
            var formIdentifier = ReadString(binReader, 4);

            // The file must contain light bezel information, this is specified by the LBZL identifier in the main header.
            if(formIdentifier != LbzlIdentifier)
            {
                throw new InvalidLightFileFormatException(string.Format(CultureInfo.InvariantCulture, "The .light file read does not contain information for light bezels."));
            }

            ReadMainLightControllerData(binReader);
        }

        #endregion

        #region Read the main light controller data

        /// <summary>
        /// Read in the main light controller data.
        /// </summary>
        /// <param name="binReader">Reads primitive data types as binary values in a specific character encoding.</param>
        /// <exception cref="InvalidLightFileFormatException">Thrown if the format version is not the same as the format version supported.</exception>
        private void ReadMainLightControllerData(BinaryReader binReader)
        {
            // Read in the data tag.
            ReadString(binReader, 4);
 
            // Read in the data size.
            binReader.ReadUInt32();

            // Read in the format version for the main light controller data.
            var version = binReader.ReadUInt16();

            // Check to see if the format version is supported.
            if(version != VersionSupported)
            {
                throw new InvalidLightFileFormatException($"Only format version {VersionSupported} is supported.");
            }

            // Read in the number of light controller types within the .light file.
            var numberOfControllerTypes = binReader.ReadByte();

            // Read in the length of the animation sequence name of this .light file.
            var animationSequenceNameLength = binReader.ReadByte();

            // Read in the animation name of this .light file (i.e. "SingleColor").
            // The name might not always be specified (i.e. "Untitled").
            var animationSequenceName = ReadString(binReader, animationSequenceNameLength);

            // Create a new main light controller data object and read data into it.
            MainLightControllerData = new MainLightControllerData(animationSequenceName);

            // Read in each main light controller data for each light controller type.
            for(ushort numController = 0; numController < numberOfControllerTypes; ++numController)
            {
                ReadLightControllerDataForm(binReader);
            }
        }

        #endregion

        #region Read the light controller data

        /// <summary>
        /// Read in the light controller data.
        /// </summary>
        /// <param name="binReader">Reads primitive data types as binary values in a specific character encoding.</param>
        /// <exception cref="InvalidLightFileFormatException">Thrown if the format version is not as the
        /// same as the format version supported or if the data is compressed.</exception>
        private void ReadLightControllerDataForm(BinaryReader binReader)
        {
            // Read in the data tag.
            ReadString(binReader, 4);

            // Read in the data size.
            binReader.ReadUInt32();

            // Read in the light controller type.
            var identifer = binReader.ReadUInt32();

            var lightControllerData = new LightControllerData(identifer);

            // Read in the format version.
            var version = binReader.ReadUInt16();

            // Check to see if the format version is supported.
            if(version != VersionSupported)
            {
                throw new InvalidLightFileFormatException($"Only format version {VersionSupported} is supported.");
            }

            // Read in the default duration for the animation sequences/frames.
            var defaultDuration = binReader.ReadUInt32();

            // Read in the number of frames.
            var numberOfFrames = binReader.ReadUInt32();

            // Read in the number of pixels per frame. Each pixels consists of four bytes of data.
            var numberOfPixelsPerFrame = binReader.ReadUInt32();

            // Read in the data compression type.
            var compressionType = binReader.ReadByte();

            // Ensure that data compression is not supported.
            if(compressionType != 0)
            {
                throw new InvalidLightFileFormatException("Compression is not supported.");
            }

            // Read in the pixel data for each frame.
            for(ushort numFrame = 0; numFrame < numberOfFrames; ++numFrame)
            {
               ReadAnimationData(binReader, lightControllerData, defaultDuration, numberOfPixelsPerFrame);
            }

            MainLightControllerData.AddLightControllerData(lightControllerData);
        }

        #endregion

        #region Read the animation data

        /// <summary>
        /// Read in the light animation data.
        /// </summary>
        /// <remarks>Only the two lower significant bits are used when each color byte is
        /// packed into <see cref="Rgb6"/>.</remarks>
        /// <param name="binReader">Reads primitive data types as binary values in a specific character encoding.</param>
        /// <param name="lightControllerData">An <see cref="LightControllerData"/> object.</param>
        /// <param name="defaultDuration">The default duration for the animation frame if none is provided.</param>
        /// <param name="numberOfPixelsPerFrame">The number of animation frames for the light controller.</param>
        /// <exception cref="InvalidLightFileFormatException">Thrown if the format version is not in the same as the format version supported.</exception>
        private static void ReadAnimationData(BinaryReader binReader, LightControllerData lightControllerData, uint defaultDuration, uint numberOfPixelsPerFrame)
        {
            // Read in the data tag.
            ReadString(binReader, 4);

            // Read in the data size.
            binReader.ReadUInt32();

            // Read in the format version.
            var version = binReader.ReadUInt16();

            // Check to see if the format version is supported.
            if(version != VersionSupported)
            {
                throw new InvalidLightFileFormatException($"Only format version {VersionSupported} is supported.");
            }

            // Read in the frame number.
            binReader.ReadUInt32();
 
            // Read in the duration of this frame.
            var duration = binReader.ReadUInt32();

            // If the duration is not specified for this frame, the default duration is used.
            if(duration == 0)
            {
                duration = defaultDuration;
            }

            var animationData = new AnimationData(duration);

            // Read in all the color pixels.
            for(uint pixel = 0; pixel < numberOfPixelsPerFrame; ++pixel)
            {
                // Skip the reserved byte.
                binReader.ReadByte();

                // Read in the RGB values.
                var blue = binReader.ReadByte();
                var green = binReader.ReadByte();
                var red = binReader.ReadByte();

                animationData.AddFrameData(new Rgb6(red, green, blue));
            }

            lightControllerData.AddAnimationData(animationData);
        }

        #endregion

        #endregion
    }
}