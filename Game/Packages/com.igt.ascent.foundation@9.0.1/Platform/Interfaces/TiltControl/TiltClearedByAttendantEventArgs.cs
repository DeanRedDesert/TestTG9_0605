//-----------------------------------------------------------------------
// <copyright file = "TiltClearedByAttendantEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces.TiltControl
{
    using System;

    /// <summary>
    /// This class represents a tilt cleared by attendant event.
    /// </summary>
    [Serializable]
    public class TiltClearedByAttendantEventArgs : PlatformEventArgs
    {
        /// <summary>
        /// Gets the name of the tilt that was cleared.
        /// </summary>
        public string TiltName { get; private set; }

        /// <summary>
        /// Construct an instance of this class with the given name, the transaction weight will be heavy.
        /// </summary>
        /// <param name="tiltName">
        /// The name of the cleared tilt.
        /// </param>
        public TiltClearedByAttendantEventArgs(string tiltName)
            : base(TransactionWeight.Heavy) => TiltName = tiltName;

        /// <summary>
        /// Construct an instance of this class with the given name and weight, the weight will default to heavy.
        /// </summary>
        /// <param name="tiltName">
        /// The name of the cleared tilt.
        /// </param>
        /// <param name="weight">
        /// The weight of the transactional event.
        /// </param>
        protected TiltClearedByAttendantEventArgs(string tiltName, TransactionWeight weight = TransactionWeight.Heavy)
            : base(weight) => TiltName = tiltName;
    }
}
