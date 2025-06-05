// -----------------------------------------------------------------------
// <copyright file = "EnrollResponseData.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// This class defines the data of enroll response retrieved from the Foundation.
    /// </summary>
    [Serializable]
    public class EnrollResponseData
    {
        #region Properties

        /// <summary>
        /// Gets the boolean flag that indicates if the data is ready, which is after the EnrollResponse has been sent.
        /// If the data is not ready the enrollment data will not be included.
        /// </summary>
        public bool IsReady { get; private set; }

        /// <summary>
        /// Gets the boolean flag that indicates if the enrollment was successful. Omitted if enrollment has not yet completed.
        /// </summary>
        public bool? EnrollSuccess { get; private set; }

        /// <summary>
        /// Gets the binary host enrollment data.
        /// </summary>
        public byte[] HostEnrollmentData { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates an instance of <see cref="EnrollResponseData"/>;
        /// </summary>
        /// <param name="isReady">The boolean flag that indicates if the data is ready.</param>
        /// <param name="enrollSuccess">The boolean flag that indicates if the enrollment was successful.</param>
        /// <param name="hostEnrollmentData">The binary host enrollment data.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="isReady"/> conflicts with <paramref name="enrollSuccess"/> value.
        /// </exception>
        public EnrollResponseData(bool isReady, bool? enrollSuccess, byte[] hostEnrollmentData)
        {
            if(isReady && enrollSuccess == null || !isReady && enrollSuccess != null)
            {
                throw new ArgumentException(
                    $"isReady flag({isReady}) conflicts with the nullable enrollSuccess value({enrollSuccess}).");
            }

            IsReady = isReady;
            EnrollSuccess = enrollSuccess;
            HostEnrollmentData = hostEnrollmentData;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("EnrollResponseData -");
            builder.AppendLine("\t IsReady = " + IsReady);
            if(IsReady)
            {
                builder.AppendLine("\t EnrollSuccess = " + EnrollSuccess);
            }

            builder.AppendLine("\t HostEnrollmentData: " + (HostEnrollmentData == null ? "n/a" : HostEnrollmentData.Length + " bytes"));
            if(HostEnrollmentData != null && HostEnrollmentData.Length > 0)
            {
                builder.Append("\t\t");
                foreach(var byteData in HostEnrollmentData)
                {
                    builder.AppendFormat("0x{0:X} ", byteData);
                }
                builder.AppendLine();
            }

            return builder.ToString();
        }

        #endregion
    }
}