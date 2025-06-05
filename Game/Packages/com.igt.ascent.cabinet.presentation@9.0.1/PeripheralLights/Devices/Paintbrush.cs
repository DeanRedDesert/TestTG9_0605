//-----------------------------------------------------------------------
// <copyright file = "Paintbrush.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    /// <summary>
    /// Defines a paintbrush.
    /// </summary>
    public struct Paintbrush
    {
        /// <summary>
        /// Constructs a paint brush.
        /// </summary>
        /// <param name="startPosition">The starting position of the brush.</param>
        /// <param name="direction">The direction the brush will move.</param>
        /// <param name="alternateDirection">If the brush should alternate directions after each cycle.</param>
        public Paintbrush(AccentLightStartPosition startPosition, LightDirection direction, bool alternateDirection)
        {
            StartPosition = startPosition;
            Direction = direction;
            AlternateDirection = alternateDirection;
        }

        /// <summary>
        /// The starting position of the brush.
        /// </summary>
        public AccentLightStartPosition StartPosition { get; set; }

        /// <summary>
        /// The direction the brush will move.
        /// </summary>
        public LightDirection Direction { get; set; }

        /// <summary>
        /// If the brush should alternate directions after each cycle.
        /// </summary>
        public bool AlternateDirection { get; set; }

        /// <summary>
        /// Tests if two paintbrush structures are the same.
        /// </summary>
        /// <param name="left">The Paintbrush that is to the left of the equality operator.</param>
        /// <param name="right">The Paintbrush that is to the right of the equality operator.</param>
        /// <returns>True if they are the same otherwise false.</returns>
        public static bool operator ==(Paintbrush left, Paintbrush right)
        {
            return left.StartPosition == right.StartPosition && left.Direction == right.Direction
                && left.AlternateDirection == right.AlternateDirection;
        }

        /// <summary>
        /// Tests if two paintbrush structures are different.
        /// </summary>
        /// <param name="left">The Paintbrush that is to the left of the inequality operator.</param>
        /// <param name="right">The Paintbrush that is to the right of the inequality operator.</param>
        /// <returns>True if they are the different otherwise false.</returns>
        public static bool operator !=(Paintbrush left, Paintbrush right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if(obj is Paintbrush objBrush)
            {
                return StartPosition == objBrush.StartPosition && Direction == objBrush.Direction
                    && AlternateDirection == objBrush.AlternateDirection;
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
