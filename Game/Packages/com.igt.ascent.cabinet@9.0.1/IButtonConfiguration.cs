//-----------------------------------------------------------------------
// <copyright file = "IButtonConfiguration.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System.Collections.Generic;

    /// <summary>
    /// This interface contains the information of a button.
    /// </summary>
    public interface IButtonConfiguration
    {
        /// <summary>
        /// Gets the Id of the button.
        /// </summary>
        /// <remarks>
        /// The button Id is to identify an unique button on a button panel.
        /// </remarks>
        byte ButtonId { get; }

        /// <summary>
        /// Gets the unique button identifier.
        /// </summary>
        /// <remarks>
        /// The button identifier is to identify a button on an EGM, which could be used to
        /// support multiple panels.
        /// </remarks>
        ButtonIdentifier ButtonIdentifier { get; }

        /// <summary>
        /// Gets the hardware type of the button.
        /// </summary>
        ButtonType HardwareType { get; }

        /// <summary>
        /// Checks if the button has dynamic display.
        /// </summary>
        bool HasDynamicDisplay { get; }

        /// <summary>
        /// Gets the functions associated with the button.
        /// </summary>
        IList<ButtonFunction> Functions { get; }

        /// <summary>
        /// Checks if there are valid button functions for current button.
        /// </summary>
        /// <returns>
        /// True if there exist at least one valid button function. Otherwise, false.
        /// </returns>
        bool HasValidButtonFunction();
    }
}
