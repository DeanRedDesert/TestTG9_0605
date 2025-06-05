//-----------------------------------------------------------------------
// <copyright file = "HeadphoneJackChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Event indicating that the headphone jack state has changed.
    /// </summary>
    public class HeadphoneJackChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The new state of the headphone jack.
        /// </summary>
        public HeadphoneJackState HeadphoneJackState { get; }

        /// <summary>
        /// Construct an instance with the given headphone jack state.
        /// </summary>
        /// <param name="state">
        /// The state of the headphone jack.
        /// </param>
        public HeadphoneJackChangedEventArgs(HeadphoneJackState state)
        {
            HeadphoneJackState = state;
        }
    }
}
