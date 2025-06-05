//-----------------------------------------------------------------------
// <copyright file = "PresentationAutomationSendActiveButtonsMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;

    /// <summary>
    /// Message object that is comprised of the parameters of the IPresentationAutomationService
    /// interface function SendActiveButtons.
    /// </summary>
    [Serializable]
    public class PresentationAutomationSendActiveButtonsMsg : AutomationGenericMsg
    {
        /// <summary>
        /// Empty Constructor required for certain types of serialization.
        /// </summary>
        private PresentationAutomationSendActiveButtonsMsg() { }

        /// <summary>
        /// Constructor for creating PresentationAutomationSendActiveButtonsMsg.
        /// </summary>
        /// <param name="availableButtons">List of active button names.</param>
        /// <exception cref="ArgumentException">Thrown if an entry in availableButtons is empty or null.</exception>
        public PresentationAutomationSendActiveButtonsMsg(IList<string> availableButtons)
        {
            if (availableButtons != null)
            {
                if (availableButtons.Any(availableButton => string.IsNullOrEmpty(availableButton)))
                {
                    throw new ArgumentException("Elements in availableButtons cannot be null or empty", "availableButtons");
                }
            }

            AvailableButtons = availableButtons;
        }

        /// <summary>
        /// Display contents of object as string.
        /// </summary>
        /// <returns>string representation of object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            if (AvailableButtons != null)
            {
                foreach (var button in AvailableButtons)
                {
                    builder.AppendLine("\tAvailableButton:" + button);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets list of button names that are currently active.
        /// </summary>
        public IList<string> AvailableButtons { get; private set; }
    }
}
