//-----------------------------------------------------------------------
// <copyright file = "CabinetButtonPressedEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;
    using Timing;

    /// <summary>
    /// Class indicating a button event from the foundation.
    /// </summary>
    public class CabinetButtonPressedEventArgs : TimeStampedEventArgs
    {
        /// <summary>
        /// Id of the pressed button.
        /// </summary>
        public int ButtonId => ButtonIdentifier.Identifier;

        /// <summary>
        /// Identifier of the pressed button.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public ButtonIdentifier ButtonIdentifier { get; }

        /// <summary>
        /// Function list of the button.
        /// </summary>
        public List<ButtonFunction> Functions { get; }

        /// <summary>
        /// Pressed status of the button. 
        /// </summary>
        public bool Pressed { get; }

        /// <summary>
        /// Construct an instance of the CabinetButtonPressEventArgs class with the given key.
        /// </summary>
        /// <param name="buttonId">Button Id which was pressed.</param>
        /// <param name="pressed">If true the button is pressed, if false it is not pressed.</param>
        /// <param name="functions">The function list of the pressed button.</param>
        /// <remarks>
        /// This event goes to main button panel, as no Button Panel Location designated.
        /// </remarks>
        public CabinetButtonPressedEventArgs(int buttonId, bool pressed, List<ButtonFunction> functions)
            : this(new ButtonIdentifier(ButtonPanelLocation.Main, (byte)buttonId), pressed, functions)
        {
        }

        /// <summary>
        /// Construct an instance of the CabinetButtonPressEventArgs class with the given key.
        /// </summary>
        /// <param name="buttonId">Button Identifier which was pressed.</param>
        /// <param name="pressed">If true the button is pressed, if false it is not pressed.</param>
        /// <param name="functions">The function list of the pressed button.</param>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when <paramref name="buttonId"/> is null.
        /// </exception>
        public CabinetButtonPressedEventArgs(ButtonIdentifier buttonId, bool pressed, List<ButtonFunction> functions)
        {
            if(buttonId == null)
            {
                throw new ArgumentNullException(nameof(buttonId));
            }

            ButtonIdentifier = buttonId;
            Functions = functions;
            Pressed = pressed;
        }

        /// <summary>
        /// Construct an instance of the CabinetButtonPressEventArgs class with the given key.
        /// </summary>
        /// <param name="buttonId">Button Id which was pressed.</param>
        /// <param name="pressed">If true the button is pressed, if false it is not pressed.</param>
        /// <param name="functions">The function list of the pressed button.</param>
        /// <param name="timeStamp">The time stamp from when the button event occurred.</param>
        /// <remarks>
        /// This event goes to main button panel, as no Button Panel Location designated.
        /// </remarks>
        public CabinetButtonPressedEventArgs(int buttonId, bool pressed, List<ButtonFunction> functions, long timeStamp)
            : this(new ButtonIdentifier(ButtonPanelLocation.Main, (byte)buttonId), pressed, functions, timeStamp)
        {
        }

        /// <summary>
        /// Construct an instance of the CabinetButtonPressEventArgs class with the given key.
        /// </summary>
        /// <param name="buttonId">Button Identifier which was pressed.</param>
        /// <param name="pressed">If true the button is pressed, if false it is not pressed.</param>
        /// <param name="functions">The function list of the pressed button.</param>
        /// <param name="timeStamp">The time stamp from when the button event occurred.</param>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when <paramref name="buttonId"/> is null.
        /// </exception>
        /// <remarks>
        /// The <paramref name="timeStamp"/> in these events is provided by the foundation, and is currently calculated using
        /// QueryPerformanceCounter * 1000 / QueryPerformanceFrequency.
        /// </remarks>
        public CabinetButtonPressedEventArgs(ButtonIdentifier buttonId, bool pressed, List<ButtonFunction> functions,
            long timeStamp) : base(timeStamp)
        {
            if(buttonId == null)
            {
                throw new ArgumentNullException(nameof(buttonId));
            }

            ButtonIdentifier = buttonId;
            Functions = functions;
            Pressed = pressed;
        }
    }
}