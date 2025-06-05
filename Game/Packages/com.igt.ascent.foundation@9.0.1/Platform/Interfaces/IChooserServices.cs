//-----------------------------------------------------------------------
// <copyright file = "IChooserServices.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This interface defines APIs for a game to talk to the Foundation in terms of Chooser Services,
    /// such as availability of the More Games button.
    /// </summary>
    public interface IChooserServices
    {
        /// <summary>
        /// Event occurs when any of chooser services properties are updated.
        /// </summary>
        event EventHandler<ChooserPropertiesUpdateEventArgs> ChooserPropertiesUpdateEvent;

        /// <summary>
        /// Make a request to launch the chooser.
        /// </summary>
        void RequestChooser();

        /// <summary>
        /// Retrieves all chooser related properties.
        /// </summary>
        ChooserProperties ChooserProperties { get; }
    }
}
