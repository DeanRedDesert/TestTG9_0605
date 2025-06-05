//-----------------------------------------------------------------------
// <copyright file = "Choreography.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable BaseObjectGetHashCodeCallInGetHashCode
namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming.Choreography
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CompactSerialization;

    /// <summary>
    /// Represents a choreography sequence.
    /// </summary>
    public class Choreography : IEquatable<Choreography>, IEnumerable<Step>
    {
        /// <summary>
        /// The current/highest file format version supported.
        /// </summary>
        public const ushort CurrentVersion = 1;

        private const float Epsilon = 0.00001f;

        #region Properties

        /// <summary>
        /// The version of the file format.
        /// </summary>
        public ushort Version
        {
            get;
            private set;
        }

        /// <summary>
        /// The name of the choreography.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The total duration of the choreography in seconds.
        /// </summary>
        public float Duration
        {
            get;
            set;
        }

        /// <summary>
        /// The steps of the choreography file.
        /// </summary>
        public List<Step> Steps
        {
            get;
            private set;
        }

        #endregion
        
        /// <summary>
        /// Construct new instance.
        /// </summary>
        public Choreography()
        {
            Steps = new List<Step>();
            Name = "";
            Version = CurrentVersion;
        }

        /// <summary>
        /// Construct new instance from a file.
        /// </summary>
        /// <param name="file">The file to load.</param>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the file specified cannot be found.
        /// </exception>
        public Choreography(string file)
            : this()
        {
            if(!File.Exists(file))
            {
                throw new FileNotFoundException("Unable to file choreography file.", file);
            }

            using(var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                Deserialize(fileStream);
            }
        }


        #region Equality

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var equals = false;

            if(obj != null)
            {
                if(ReferenceEquals(this, obj))
                {
                    equals = true;
                }
                else
                {
                    equals = Equals(obj as Choreography);
                }
            }

            return equals;
        }

        /// <inheritdoc />
        public bool Equals(Choreography other)
        {
            var equals = false;

            if(other != null)
            {
                if(ReferenceEquals(this, other))
                {
                    equals = true;
                }
                else
                {
                    equals = Name == other.Name
                        && Version == other.Version
                        && Duration.Equals(other.Duration)
                        && Steps.SequenceEqual(other.Steps);
                }
            }

            return equals;
        }

        #endregion

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Saves the choreography sequence to disk.
        /// </summary>
        /// <param name="file">The file path to save the sequence to.</param>
        /// <exception cref="InvalidDataException">
        /// Thrown if any steps have a time of zero. Or if the duration is zero.
        /// </exception>
        public void Save(string file)
        {
            // Step times of zero are not allowed.
            if(Steps.Any(step => step.Time < Epsilon))
            {
                throw new InvalidDataException("A step time of 0 is not allowed.");
            }

            // The duration cannot be zero.
            if(Duration < Epsilon)
            {
                throw new InvalidDataException("The duration of the choreography file cannot be 0.");
            }

            using(var saveFile = new FileStream(file, FileMode.Create, FileAccess.ReadWrite))
            {
                Serialize(saveFile);
            }
        }

        /// <summary>
        /// Reads the header of a choreography file and returns the total duration in seconds.
        /// </summary>
        /// <param name="fileName">The full path to the choreography file to read.</param>
        /// <returns>The total duration of the choreography in seconds.</returns>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the file specified in <paramref name="fileName"/> cannot be found.
        /// </exception>
        public static float GetDurationFromFile(string fileName)
        {
            float duration;

            if(!File.Exists(fileName))
            {
                throw new FileNotFoundException("Unable to find file.", fileName);
            }

            using(var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                CompactSerializer.ReadUshort(fileStream);
                CompactSerializer.ReadString(fileStream);
                duration = CompactSerializer.ReadFloat(fileStream);
            }

            return duration;
        }

        #region IEnumerable Implementation

        /// <inheritdoc />
        public IEnumerator<Step> GetEnumerator()
        {
            return Steps.GetEnumerator();
        }

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Steps.GetEnumerator();
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serializes the class using the compact serializer.
        /// </summary>
        /// <param name="stream">The stream to write the data to.</param>
        private void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, Version);
            CompactSerializer.Write(stream, Name);
            CompactSerializer.Write(stream, Duration);
            CompactSerializer.WriteList(stream, Steps);
        }

        /// <summary>
        /// Deserializes the class using the compact serializer.
        /// </summary>
        /// <param name="stream">The stream to read the data from.</param>
        private void Deserialize(Stream stream)
        {
            Version = CompactSerializer.ReadUshort(stream);
            Name = CompactSerializer.ReadString(stream);
            Duration = CompactSerializer.ReadFloat(stream);
            Steps = CompactSerializer.ReadListSerializable<Step>(stream);
        }

        #endregion
    }
}
