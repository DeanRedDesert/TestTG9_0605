//-----------------------------------------------------------------------
// <copyright file = "LightSequence.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using CompactSerialization;

    /// <summary>
    /// Represents a streaming light sequence.
    /// </summary>
    public class LightSequence : ILightSequence
    {
        /// <summary>
        /// The current/highest sequence version supported by this library.
        /// </summary>
        public const ushort CurrentSequenceVersion = 3;

        private const ushort EndOfSegmentMarker = 0xA5A5;

        private readonly List<Segment> segments;
        private string name;
        private static string gameMountPoint;
        private string uniqueId;

        #region Constructors

        /// <summary>
        /// Construct a new light sequence from a stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="stream"/> is null.
        /// </exception>
        public LightSequence(Stream stream)
            : this()
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Deserialize(stream);
        }

        /// <summary>
        /// Construct a new light sequence from a file on disk.
        /// </summary>
        /// <param name="file">The light sequence to load.</param>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Thrown if the file specified by <paramref name="file"/> cannot be found.
        /// </exception>
        public LightSequence(string file)
            : this()
        {
            file = GetAbsolutePath(file);

            if(!File.Exists(file))
            {
                throw new FileNotFoundException();
            }

            using(var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Construct an empty light sequence for a device.
        /// </summary>
        /// <param name="hardware">The hardware device this sequence is for.</param>
        public LightSequence(StreamingLightHardware hardware)
            : this()
        {
            LightDevice = hardware;
        }

        /// <summary>
        /// Construct an empty light sequence for a device.
        /// </summary>
        /// <param name="hardware">The hardware device this sequence is for.</param>
        /// <param name="version">The LightSequence version.</param>
        /// <exception cref="UnsupportedLightSequenceVersionException">Throw if an invald version is passed in.</exception>
        public LightSequence(StreamingLightHardware hardware, ushort version)
            : this()
        {
            LightDevice = hardware;
            if(version > CurrentSequenceVersion || version == 0)
            {
                throw new UnsupportedLightSequenceVersionException(version, CurrentSequenceVersion);
            }
            Version = version;
        }

        /// <summary>
        /// Construct a new empty light sequence.
        /// </summary>
        protected LightSequence()
        {
            segments = new List<Segment>();
            name = "";
            Version = CurrentSequenceVersion;
        }

        #endregion

        #region ILightSequence Implementation

        /// <inheritdoc />
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="value" /> is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="value"/> is longer than 128 characters.
        /// </exception>
        public string Name
        {
            get => name;
            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if(value.Length > 128)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The name cannot exceed 128 characters.");
                }
                name = value;
            }
        }

        /// <inheritdoc />
        public StreamingLightHardware LightDevice
        {
            get;
            protected set;
        }

        /// <inheritdoc />
        public bool Loop
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ushort Version
        {
            get;
            protected set;
        }

        /// <inheritdoc />
        public IList<Segment> Segments => segments.AsReadOnly();

        /// <inheritdoc />
        public ulong DisplayTime
        {
            get
            {
                ulong total = 0;
                // This is done instead of the LINQ sum function because the sum function
                // doesn't support UInt64.
                segments.ForEach(segment => total += segment.DisplayTime);
                return total;
            }
        }

        /// <inheritdoc />
        public string UniqueId
        {
            get
            {
                if(uniqueId == null)
                {
                    uniqueId = "";
                    // Make a hash of this sequence so that a unique identifier can be created for light sequence caching.
                    using(var hasher = SHA1.Create())
                    {
                        // Convert the sequence bytes into a base 64 string.
                        uniqueId = Convert.ToBase64String(hasher.ComputeHash(GetSequenceBytes()));
                    }
                }

                return uniqueId;
            }
            private set => uniqueId = value;
        }

        /// <inheritdoc />
        public byte[] GetSequenceBytes()
        {
            string base64String;

            using(var streamBuffer = new MemoryStream())
            {
                Serialize(streamBuffer);
                streamBuffer.Seek(0, SeekOrigin.Begin);
                var byteBuffer = new byte[streamBuffer.Length];
                streamBuffer.Read(byteBuffer, 0, Convert.ToInt32(streamBuffer.Length));
                base64String = Convert.ToBase64String(byteBuffer);
            }

            return new ASCIIEncoding().GetBytes(base64String);
        }

        /// <summary>
        /// Serializes the class using the compact serializer.
        /// </summary>
        /// <param name="stream">The stream to write the data to.</param>
        protected void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, Name);
            CompactSerializer.Write(stream, Version);
            CompactSerializer.Write(stream, LightDevice);
            CompactSerializer.Write(stream, Loop);

            if(Version > 1)
            {
                CompactSerializer.Write(stream, UniqueId);
            }

            var segmentsNull = segments == null;
            CompactSerializer.Write(stream, segmentsNull);
            if(!segmentsNull)
            {
                CompactSerializer.Write(stream, segments.Count);
                foreach(var segment in segments)
                {
                    var segmentNull = segment == null;
                    CompactSerializer.Write(stream, segmentNull);
                    if(!segmentNull)
                    {
                        segment.SerializeByVersion(stream, Version);
                    }
                    CompactSerializer.Write(stream, EndOfSegmentMarker);
                }
            }
        }

        /// <summary>
        /// Deserializes the class using the compact serializer.
        /// </summary>
        /// <param name="stream">The stream to read the data from.</param>
        /// <exception cref="UnsupportedLightSequenceVersionException">
        /// Thrown if the light sequence version number is larger than
        /// what is supported by this library.
        /// </exception>
        protected void Deserialize(Stream stream)
        {
            Name = CompactSerializer.ReadString(stream);
            Version = CompactSerializer.ReadUshort(stream);

            if(Version > CurrentSequenceVersion)
            {
                throw new UnsupportedLightSequenceVersionException(Version, CurrentSequenceVersion);
            }

            try
            {
                LightDevice = CompactSerializer.ReadEnum<StreamingLightHardware>(stream);
            }
            catch(ArgumentException ex)
            {
                // If the enum value cannot be found, set the light device to unknown.
                if(ex.Message.Contains("not found"))
                {
                    LightDevice = StreamingLightHardware.Unknown;
                }
                else
                {
                    throw;
                }
            }

            Loop = CompactSerializer.ReadBool(stream);
            if(Version > 1)
            {
                UniqueId = CompactSerializer.ReadString(stream);
            }

            if(!CompactSerializer.ReadBool(stream))
            {
                var segmentCount = CompactSerializer.ReadInt(stream);
                segments.Clear();
                while(segmentCount > 0)
                {
                    var segmentNull = CompactSerializer.ReadBool(stream);
                    if(!segmentNull)
                    {
                        var segment = new Segment();
                        segment.DeserializeByVersion(stream, Version);
                        AddSegment(segment);
                    }
                    --segmentCount;
                    var marker = CompactSerializer.ReadUshort(stream);
                    if(marker != EndOfSegmentMarker)
                    {
                        throw new CompactSerializationException("End of segment marker was not valid.");
                    }
                }
            }
        }

        /// <inheritdoc />
        public bool Equals(ILightSequence other)
        {
            var equal = false;

            if(other != null)
            {
                if(ReferenceEquals(this, other))
                {
                    equal = true;
                }
                else
                {
                    equal = LightDevice == other.LightDevice
                        && Name == other.Name
                        && Version == other.Version
                        && Loop == other.Loop
                        && UniqueId == other.UniqueId
                        && segments.SequenceEqual(other.Segments);
                }
            }

            return equal;
        }

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as LightSequence);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return Convert.ToInt32(LightDevice)
                   ^Convert.ToInt32(Version)
                   ^Convert.ToInt32(Loop)
                   ^Convert.ToInt32(DisplayTime);
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        /// <summary>
        /// Adds a segment onto the end of the list of segments.
        /// </summary>
        /// <param name="segment">The segment to add.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="segment"/> is null.
        /// </exception>
        public void AddSegment(Segment segment)
        {
            if(segment == null)
            {
                throw new ArgumentNullException(nameof(segment));
            }

            UniqueId = null;
            segment.FramesChangedEvent += SegmentFrameRemoved;
            segment.SequenceVersion = Version;
            segments.Add(segment);
        }

        /// <summary>
        /// Adds the segment at a specific index in the list.
        /// </summary>
        /// <param name="index">The index to add the segment at.</param>
        /// <param name="segment">The segment to add.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="segment"/> is null.
        /// </exception>
        public void AddSegment(int index, Segment segment)
        {
            if(segment == null)
            {
                throw new ArgumentNullException(nameof(segment));
            }

            UniqueId = null;
            segment.FramesChangedEvent += SegmentFrameRemoved;
            segment.SequenceVersion = Version;
            segments.Insert(index, segment);
        }

        /// <summary>
        /// Removes a segment at the specified index.
        /// </summary>
        /// <param name="index">The index of the segment to remove.</param>
        public void RemoveSegment(int index)
        {
            UniqueId = null;
            if(index < segments.Count)
            {
                segments[index].FramesChangedEvent -= SegmentFrameRemoved;
            }
            segments.RemoveAt(index);
        }

        /// <summary>
        /// When a frame is removed or changed we need to remove the current Unique Id.
        /// </summary>
        /// <param name="obj">Segment that removed the frame.</param>
        /// <param name="args">Arguments for removing a frame.</param>
        private void SegmentFrameRemoved(object obj, EventArgs args)
        {
            UniqueId = null;
        }

        /// <summary>
        /// Saves the light sequence to disk.
        /// </summary>
        /// <param name="file">The file path to save the sequence to.</param>
        public void Save(string file)
        {
            file = GetAbsolutePath(file);
            using(var saveFile = new FileStream(file, FileMode.Create, FileAccess.ReadWrite))
            {
                Save(saveFile);
            }
        }

        /// <summary>
        /// Saves the light sequence to a stream.
        /// </summary>
        /// <param name="stream">The stream to save the sequence to.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="stream"/> is null.
        /// </exception>
        public void Save(Stream stream)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if(Version >= 3)
            {
                var frameProcessor = new KeyFrameProcessor();
                frameProcessor.CreateKeyFrames(this);
                var optimizer = new LightSequenceOptimizer();
                optimizer.OptimizeLightSequence(this);
            }

            Serialize(stream);
        }

        /// <summary>
        /// Reads the hardware type of a light sequence file. It only reads
        /// as far as the hardware type so the cost of this read is much lower
        /// than reading the entire file.
        /// </summary>
        /// <param name="filename">The light sequence to read.</param>
        /// <returns>The hardware type of the sequence.</returns>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the file specified in <paramref name="filename"/> cannot be found.
        /// </exception>
        /// <exception cref="UnsupportedLightSequenceVersionException">
        /// Thrown if the version of the light sequence read is higher than
        /// the version supported by this library.
        /// </exception>
        public static StreamingLightHardware GetHardwareFromFile(string filename)
        {
            filename = GetAbsolutePath(filename);

            if(!File.Exists(filename))
            {
                throw new FileNotFoundException("Unable to find file.", filename);
            }

            using(var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
               return ReadHardwareFromStream(fileStream);
            }
        }

        /// <summary>
        /// Reads the hardware type of a light sequence file. It only reads
        /// as far as the hardware type so the cost of this read is much lower
        /// than reading the entire file.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <returns>The hardware type of the sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        /// <exception cref="UnsupportedLightSequenceVersionException">
        /// Thrown if the version of the light sequence read is higher than
        /// the version supported by this library.
        /// </exception>
        public static StreamingLightHardware ReadHardwareFromStream(Stream stream)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            StreamingLightHardware device;
            // Skip the sequence name.
            ReadNameFromStream(stream);
            var version = CompactSerializer.ReadUshort(stream);
            if(version > CurrentSequenceVersion)
            {
                throw new UnsupportedLightSequenceVersionException(version, CurrentSequenceVersion);
            }

            try
            {
                device = CompactSerializer.ReadEnum<StreamingLightHardware>(stream);
            }
            catch(ArgumentException ex)
            {
                // If the enum value cannot be found, set the light device to unknown.
                if(ex.Message.Contains("not found"))
                {
                    device = StreamingLightHardware.Unknown;
                }
                else
                {
                    throw;
                }
            }

            return device;
        }

        /// <summary>
        /// Reads the sequence name of a light sequence file. It only reads
        /// as far as the name so the cost of this read is much lower
        /// than reading the entire file.
        /// </summary>
        /// <param name="filename">The light sequence to read.</param>
        /// <returns>The name of the sequence.</returns>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the file specified in <paramref name="filename"/> cannot be found.
        /// </exception>
        public static string GetNameFromFile(string filename)
        {
            filename = GetAbsolutePath(filename);

            if(!File.Exists(filename))
            {
                throw new FileNotFoundException("Unable to find file.", filename);
            }

            using(var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                return ReadNameFromStream(fileStream);
            }
        }

        /// <summary>
        /// Reads the sequence name of a light sequence file. It only reads
        /// as far as the name so the cost of this read is much lower
        /// than reading the entire file.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <returns>The name of the sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static string ReadNameFromStream(Stream stream)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return CompactSerializer.ReadString(stream);
        }

        /// <summary>
        /// Sets the game mount point to use when loading files from disk.
        /// </summary>
        /// <param name="mountPoint">The game mount point.</param>
        public static void SetGameMountPoint(string mountPoint)
        {
            gameMountPoint = mountPoint;
        }

        /// <summary>
        /// Tries to get the absolute path if <see cref="gameMountPoint"/> is set and <paramref name="filePath"/>
        /// is not already absolute.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <returns>
        /// <paramref name="filePath"/> if it is absolute or <see cref="gameMountPoint"/> is null.
        /// Otherwise, the absolute path under <see cref="gameMountPoint"/>.
        /// </returns>
        private static string GetAbsolutePath(string filePath)
        {
            if(gameMountPoint != null && !Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(gameMountPoint, filePath);
            }

            return filePath;
        }
    }
}
