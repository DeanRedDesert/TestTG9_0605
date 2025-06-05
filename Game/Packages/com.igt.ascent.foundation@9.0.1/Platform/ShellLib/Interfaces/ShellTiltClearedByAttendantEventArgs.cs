// -----------------------------------------------------------------------
// <copyright file = "ShellTiltClearedByAttendantEventArgs.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using Platform.Interfaces;
    using Platform.Interfaces.TiltControl;

    /// <summary>
    /// This class represents an event that occurs when a tilt posted by Shell application is cleared by an attendant.
    /// </summary>
    [Serializable]
    public class ShellTiltClearedByAttendantEventArgs : TiltClearedByAttendantEventArgs,
        ITransactionDowngrade<ShellTiltClearedByAttendantEventArgs>
    {
        /// <summary>
        /// Gets the Id for the coplayer that posted the tilt that is being cleared,
        /// null if the tilt was posted by the shell.
        /// </summary>
        public int? CoplayerId { get; private set; }

        #region Constructors

        /// <summary>
        /// Construct an instance of this class with the given name and coplayer id. The transaction weight will be heavy.
        /// </summary>
        /// <param name="tiltName">
        /// The name of the cleared tilt.
        /// </param>
        /// <param name="coplayerId">
        /// The id of the coplayer that posted this tilt, null if the shell posted the tilt.
        /// </param>
        public ShellTiltClearedByAttendantEventArgs(string tiltName, int? coplayerId = null)
            : base(tiltName, TransactionWeight.Heavy)
            => CoplayerId = coplayerId;

        /// <summary>
        /// Construct an instance of this class with the given name, coplayer id, and transaction weight.
        /// </summary>
        /// <param name="tiltName">
        /// The name of the cleared tilt.
        /// </param>
        /// <param name="coplayerId">
        /// The id of the coplayer that posted this tilt, null if the shell posted the tilt.
        /// </param>
        /// <param name="weight">
        /// The transaction weight of this event, heavy by default.
        /// </param>
        // ReSharper disable once MemberCanBePrivate.Global
        protected ShellTiltClearedByAttendantEventArgs(string tiltName, int? coplayerId = null, TransactionWeight weight = TransactionWeight.Heavy)
            : base(tiltName, weight)
            => CoplayerId = coplayerId;

        #endregion

        #region ITransactionDowngrade Implementation

        /// <inheritdoc/>
        public ShellTiltClearedByAttendantEventArgs Downgrade(TransactionWeight newTransactionWeight)
        {
            if(newTransactionWeight >= TransactionWeight)
            {
                throw new InvalidOperationException($"New transaction weight {newTransactionWeight} is no less than the current value {TransactionWeight}");
            }

            return new ShellTiltClearedByAttendantEventArgs(TiltName, CoplayerId, newTransactionWeight);
        }

        #endregion
    }
}