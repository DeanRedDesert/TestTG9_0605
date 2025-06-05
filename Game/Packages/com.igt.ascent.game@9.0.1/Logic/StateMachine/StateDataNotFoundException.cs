//-----------------------------------------------------------------------
// <copyright file = "StateDataNotFoundException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;
    using System.Globalization;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// An exception to throw when there is no state data cache.
    /// </summary>
    [Serializable]
    public class StateDataNotFoundException : Exception
    {
        private const string FormatString =
            "No state data was provided by the presentation.  Current Game Context: {0}";
        
        /// <summary>
        /// The <see cref="GameMode"/> that was active when the exception was thrown.
        /// </summary>
        public GameMode CurrentContext { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateDataNotFoundException"/> class.
        /// </summary>
        /// <param name="currentContext">The current <see cref="GameMode"/>.</param>
        public StateDataNotFoundException(GameMode currentContext)
            :base(string.Format(CultureInfo.InvariantCulture, FormatString, currentContext))
        {
            CurrentContext = currentContext;
        }
    }
}
