//-----------------------------------------------------------------------
// <copyright file = "SpinAttributes.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;

    /// <summary>
    /// Class which contains attributes that can be applied to a spin.
    /// </summary>
    [Serializable]
    public class SpinAttributes
    {
        /// <summary>
        /// Flag indicating if the motor should cock before spinning.
        /// </summary>
        public bool Cock { get; private set; }

        /// <summary>
        /// Flag indicating if the motor should bounce when stopping.
        /// </summary>
        public bool Bounce { get; private set; }

        /// <summary>
        /// Amount that the motor should shake.
        /// </summary>
        public ShakeLevel Shake { get; private set; }

        /// <summary>
        /// Amount of hover when a reel stops spinning.
        /// </summary>
        public HoverAttribute Hover { get; private set; }


        /// <summary>
        /// Construct an instance with default values, which
        /// disables cock, bounce, shake, and hover.
        /// </summary>
        public SpinAttributes() : this(false, false, ShakeLevel.Off, HoverAttribute.Off)
        {
        }

        /// <summary>
        /// Construct an instance with the given spin attributes.
        /// </summary>
        /// <param name="cock">Flag indicating if the motor should cock before spinning.</param>
        /// <param name="bounce">Flag indicating if the motor should bounce when stopping.</param>
        /// <param name="shake">Amount that the motor should shake <see cref="ShakeLevel"/>.</param>
        /// <param name="hover">How the reel should hover.</param>
        public SpinAttributes(bool cock, bool bounce, ShakeLevel shake, HoverAttribute hover)
        {
            Cock = cock;
            Bounce = bounce;
            Shake = shake;
            Hover = hover;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Cock ({Cock}) / Bounce ({Bounce}) / Shake ({Shake}) / Hover ({Hover})";
        }
    }
}
