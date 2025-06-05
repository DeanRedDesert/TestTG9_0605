//-----------------------------------------------------------------------
// <copyright file = "EnrollResponseEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.IO;
    using System.Text;
    using Game.Core.CompactSerialization;

    /// <summary>
    /// Event which represents an enroll response.
    /// </summary>
    [Serializable]
    public class EnrollResponseEventArgs : EventArgs, ICompactSerializable
    {
        /// <summary>
        /// Property indicating is enrollment was successful.
        /// </summary>
        public bool EnrollSuccess
        {
            get;
            private set;
        }

        /// <summary>
        /// Property containing enrollment system specific data.
        /// EnrollmentData may be null and must be checked.
        /// </summary>
        public byte[] EnrollmentData
        {
            get;
            private set;
        }

        /// <summary>
        /// Create an EnrollResponseEvent with the given parameters.
        /// </summary>
        /// <param name="enrollSuccess">Flag indicating if enrollment was successful.</param>
        /// <param name="enrollmentData">Enrollment system specific data.</param>
        public EnrollResponseEventArgs(bool enrollSuccess, byte[] enrollmentData)
        {
            EnrollSuccess = enrollSuccess;
            EnrollmentData = enrollmentData;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("EnrollResponseEvent -");
            builder.AppendLine("\t Success = " + EnrollSuccess);

            builder.Append("\t Enrollment Data = ");
            if(EnrollmentData != null)
            {
                foreach(var b in EnrollmentData) { builder.Append(b.ToString("X") + " "); }
            }

            builder.AppendLine();

            return builder.ToString();
        }

        #region ICompactSerializable Members

        /// <summary>
        /// This parameter-less constructor is required by ICompactSerializable
        /// interface, and should not be used for any purpose other than
        /// deserialization.
        /// </summary>
        public EnrollResponseEventArgs()
        {
        }

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, EnrollSuccess);
            CompactSerializer.Write(stream, EnrollmentData);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            EnrollSuccess = CompactSerializer.ReadBool(stream);
            EnrollmentData = CompactSerializer.ReadByteArray(stream);
        }

        #endregion
    }
}
