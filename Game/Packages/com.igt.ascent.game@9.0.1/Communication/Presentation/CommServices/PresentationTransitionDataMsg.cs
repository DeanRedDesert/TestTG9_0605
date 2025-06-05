//-----------------------------------------------------------------------
// <copyright file = "PresentationTransitionDataMsg.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Presentation.CommServices
{
    using System;
    using System.Text;

    /// <summary>
    /// Message object that is compromised of the parameters of the 
    /// Presentation Transition Interface Functions.
    /// </summary>
    [Serializable]
    public class PresentationTransitionDataMsg : PresentationGenericMsg
    {
        /// <summary>
        /// Constructor for creating PresentationTransitionDataMsg.
        /// </summary>
        /// <param name="presentationTransition">Presentation transition type.</param>
        public PresentationTransitionDataMsg(PresentationTransition presentationTransition)
        {
            PresentationTransition = presentationTransition;
        }

        /// <summary>Override base implementation to provide better information.</summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            builder.AppendLine("\tPresentationTransition = " + PresentationTransition);

            return builder.ToString();
        }

        /// <summary>
        /// Presentation transition type.
        /// </summary>
        public PresentationTransition PresentationTransition { get; private set; }
    }
}
