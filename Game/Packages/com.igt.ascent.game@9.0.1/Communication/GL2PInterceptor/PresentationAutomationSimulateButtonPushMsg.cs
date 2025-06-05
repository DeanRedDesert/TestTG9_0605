//-----------------------------------------------------------------------
// <copyright file = "PresentationAutomationSimulateButtonPushMsg.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib.SocketCommunication
{
    using System;
    using System.Text;

    /// <summary>
    /// Message object that is comprised of the parameters of the IPresentationAutomationClient
    /// interface function SimulateButtonPush.
    /// </summary>
    [Serializable]
    public class PresentationAutomationSimulateButtonPushMsg : AutomationGenericMsg
    {
        /// <summary>
        /// Empty Constructor required for certain types of serialization.
        /// </summary>
        private PresentationAutomationSimulateButtonPushMsg () { }

        /// <summary>
        /// Constructor for creating PresentationAutomationSimulateButtonPushMsg.
        /// </summary>
        /// <param name="buttonPushed">Name of button to simulate being pressed.</param>
        /// <exception cref="ArgumentException">Thrown if buttonPushed is empty or null.</exception>
        public PresentationAutomationSimulateButtonPushMsg(string buttonPushed)
        {
            if (string.IsNullOrEmpty(buttonPushed))
            {
                throw new ArgumentException("buttonPushed cannot be a null or empty string.", "buttonPushed");
            }

            ButtonPushed = buttonPushed;
        }

        /// <summary>
        /// Display contents of object as string.
        /// </summary>
        /// <returns>string representation of object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            builder.AppendLine("\tButtonPushed:" + ButtonPushed);

            return builder.ToString();
        }

        /// <summary>
        /// Gets name of button to simulate being pressed.
        /// </summary>
        public string ButtonPushed { get; private set; }
    }
}
