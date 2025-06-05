//-----------------------------------------------------------------------
// <copyright file = "IButtonPanelConfiguration.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This interface contains the button panel configurations.
    /// </summary>
    public interface IButtonPanelConfiguration
    {
        /// <summary>
        /// Gets the place where the button panel is physically located. 
        /// </summary>
        ButtonPanelLocation PanelLocation { get; }

        /// <summary>
        /// Gets the type of the button panel.
        /// </summary>
        ButtonPanelType PanelType { get; }

        /// <summary>
        /// Gets the all the button configurations on the panel.
        /// </summary>
        IList<IButtonConfiguration> Buttons { get; }

        /// <summary>
        /// Gets the unique integer identifier for the button panel.
        /// </summary>
        /// <remarks>
        /// Value of 0xFFFFFFFF indicates that this field is not reported by Foundation
        /// and should not be used.
        /// </remarks>
        uint PanelIdentifier { get; }
        
        /// <summary>
        /// Gets the device Id of the button panel.
        /// </summary>
        /// <remarks>
        /// The device id would be null if running on F series or early Foundation. 
        /// </remarks>
        string DeviceId { get; }


        /// <summary>
        /// Gets the specified button on the button panel.
        /// </summary>
        /// <param name="buttonIdentifier">
        /// The button identifier of specified button on the button panel.
        /// </param>
        /// <returns>
        /// The button configuration of specified button; returns null if the specified button does not exist on
        /// the target button panel.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="buttonIdentifier"/> is null.
        /// </exception>
        IButtonConfiguration GetButton(ButtonIdentifier buttonIdentifier);

        /// <summary>
        /// Gets the specified button on from the main button panel.
        /// </summary>
        /// <param name="buttonId">
        /// The button id of specified button on the button panel.
        /// </param>
        /// <returns>
        /// The button configuration of specified button; returns null if the specified button id does not exist on
        /// the main button panel.
        /// </returns>
        /// <remarks>
        /// This function is querying <separamref name="buttonId"/> from Main Button Panel.
        /// Use the overloaded one with <see cref="ButtonIdentifier"/> 
        /// if you want to query all Button Panels.
        /// </remarks>
        IButtonConfiguration GetButton(int buttonId);

        /// <summary>
        /// Checks if specified button is on the button panel.
        /// </summary>
        /// <param name="buttonIdentifier">
        /// The button identifier of specified button on the button panel.
        /// </param>
        /// <returns>
        /// True if specified button is on the button panel. Otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="buttonIdentifier"/> is null.
        /// </exception>
        bool HasButton(ButtonIdentifier buttonIdentifier);

        /// <summary>
        /// Checks if specified button is on the button panel.
        /// </summary>
        /// <param name="buttonId">
        /// The button id of specified button on the button panel.
        /// </param>
        /// <returns>
        /// True if specified button is on the button panel. Otherwise, false.
        /// </returns>
        /// <remarks>
        /// This function is querying <paramref name="buttonId"/> from Main Button Panel.
        /// Use the overloaded one with <see cref="ButtonIdentifier"/> 
        /// if you want to query all Button Panels.
        /// </remarks>
        bool HasButton(int buttonId);

        /// <summary>
        /// Checks if there are buttons matching a custom condition on the button panel.
        /// </summary>
        /// <param name="predicate">The custom callback used to validate the match.</param>
        /// <returns>
        /// True if there's at least one button matching the custom condition on the button panel. Otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="predicate"/> is null.
        /// </exception>
        bool HasButton(Predicate<IButtonConfiguration> predicate);
    }
}
