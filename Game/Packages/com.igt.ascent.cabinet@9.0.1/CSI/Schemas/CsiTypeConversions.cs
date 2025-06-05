//-----------------------------------------------------------------------
// <copyright file = "CsiTypeConversions.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace IGT.Game.Core.Communication.Cabinet.CSI.Schemas
{
    /// <summary>
    /// Partial class that extends the functionality of the DesktopRectangle1 type
    /// to allow conversion from the DesktopRectangle type.
    /// </summary>
    /// <remarks>
    /// Both CsiWindow and CsiMonitor schemas define a data structure named DesktopRectangle,
    /// therefore the conflicting names of the generated classes.
    /// The two classes are identical, so implicit operators are implemented to help with coding.
    /// In the future, efforts should be made to remove the needs of the implicit operators.
    /// </remarks>
    public partial class DesktopRectangle1
    {
        /// <summary>
        /// Converts the DesktopRectangle type to the DesktopRectangle1 type.
        /// </summary>
        /// <param name="rectangle">The rectangle to convert.</param>
        /// <returns>The converted rectangle.</returns>
        public static implicit operator DesktopRectangle1(DesktopRectangle rectangle)
        {
            return rectangle == null
                ? null
                : new DesktopRectangle1
                {
                    h = rectangle.h,
                    w = rectangle.w,
                    x = rectangle.x,
                    y = rectangle.y
                };
        }
    }

    /// <summary>
    /// Partial class that extends the functionality of the DesktopRectangle type
    /// to allow conversion from the DesktopRectangle1 type.
    /// </summary>
    /// <remarks>
    /// Both CsiWindow and CsiMonitor schemas define a data structure named DesktopRectangle,
    /// therefore the conflicting names of the generated classes.
    /// The two classes are identical, so implicit operators are implemented to help with coding.
    /// In the future, efforts should be made to remove the needs of the implicit operators.
    /// </remarks>
    public partial class DesktopRectangle1
    {
        /// <summary>
        /// Converts the DesktopRectangle1 type to the DesktopRectangle type.
        /// </summary>
        /// <param name="rectangle1">The rectangle to convert.</param>
        /// <returns>Converted rectangle.</returns>
        public static implicit operator DesktopRectangle(DesktopRectangle1 rectangle1)
        {
            return rectangle1 == null
                ? null
                : new DesktopRectangle
                {
                    h = rectangle1.h,
                    w = rectangle1.w,
                    x = rectangle1.x,
                    y = rectangle1.y
                };
        }
    }

    /// <summary>
    /// Addition to the Monitor class to add default constants.
    /// </summary>
    public partial class Monitor
    {
        /// <summary>
        /// Default color profile ID. Can be used in conversions where an ID is not available, or for standalone
        /// implementations.
        /// </summary>
        /// <remarks>Public so that it can be used as a default in other locations.</remarks>
        public const int DefaultColorProfileId = 72;

        /// <summary>
        /// Default monitor device ID. Can be used in conversions where an ID is not available, or for standalone
        /// implementations.
        /// </summary>
        /// <remarks>Public so that it can be used as a default in other locations.</remarks>
        public const string DefaultMonitorId = "0";
    }
}
