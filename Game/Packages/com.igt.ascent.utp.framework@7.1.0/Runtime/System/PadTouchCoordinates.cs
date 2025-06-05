// -----------------------------------------------------------------------
// <copyright file = "PadTouchCoordinates.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Utp.Framework
{
    /// <summary>
    /// Used to represent X,Y coordinates for a PAD touch screen location.
    /// </summary>
    public class PadTouchCoordinates
    {
        /// <summary>
        /// x coordinate.
        /// </summary>
        public readonly double X;

        /// <summary>
        /// y coordinate.
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// Position defined by x and y coordinates.
        /// </summary>
        /// <param name="x">x coordinate.</param>
        /// <param name="y">y coordinate.</param>
        public PadTouchCoordinates(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}