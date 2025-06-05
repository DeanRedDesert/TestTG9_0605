// -----------------------------------------------------------------------
// <copyright file = "ButtonLampState.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// The class to hold the state of the lamp on the button.
    /// </summary>
    public class ButtonLampState
    {
        /// <summary>
        /// The unique button identifier.
        /// </summary>
        public ButtonIdentifier ButtonId { get; }

        /// <summary> 
        /// The state of the lamp.  True represents lamp on.
        /// </summary>
        public bool State { get; }

        /// <summary>
        /// Construct an instance of ButtonLampState.
        /// </summary>
        /// <param name="buttonId">The unique button identifier.</param>
        /// <param name="state">The state of the lamp.  True represents lamp on.</param>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when <paramref name="buttonId"/> is null.
        /// </exception>
        public ButtonLampState(ButtonIdentifier buttonId, bool state)
        {
            if(buttonId == null)
            {
                throw new ArgumentNullException(nameof(buttonId));
            }

            ButtonId = buttonId;
            State = state;
        }
    }
}