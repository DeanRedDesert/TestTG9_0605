//-----------------------------------------------------------------------
// <copyright file = "SequenceFile.Extension.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using PeripheralLights;
    using PeripheralLights.Streaming;

    public partial class SequenceFile
    {
        /// <summary>
        /// Gets the device this sequence file is for.
        /// </summary>
        [XmlIgnore]
        public StreamingLightHardware LightDevice
        {
            get;
            private set;
        }

        /// <summary>
        /// The absolute path to the file specified by this SequenceFile.
        /// </summary>
        [XmlIgnore]
        public string FileName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the display time of the light sequence in milliseconds.
        /// </summary>
        [XmlIgnore]
        public long DisplayTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes this instance with the data from disk.
        /// This method loads the display time, device type, and sets the absolute path to the file.
        /// </summary>
        /// <param name="gameMountPoint">The mount point of the game.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="gameMountPoint"/> is null.
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Thrown if the referenced by this entry cannot be found on disk.
        /// </exception>
        internal void Initialize(string gameMountPoint)
        {
            if(gameMountPoint == null)
            {
                throw new ArgumentNullException(nameof(gameMountPoint));
            }

            FileName = Path.Combine(gameMountPoint, Value);
            if(!File.Exists(FileName))
            {
                throw new FileNotFoundException($"Unable to find sequence file {FileName}.", FileName);
            }

            var lightSequence = new LightSequence(FileName);
            LightDevice = lightSequence.LightDevice;
            DisplayTime = Convert.ToInt64(lightSequence.DisplayTime);
        }
    }
}
