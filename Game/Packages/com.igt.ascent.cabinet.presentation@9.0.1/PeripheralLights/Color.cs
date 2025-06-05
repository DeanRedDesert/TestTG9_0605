//-----------------------------------------------------------------------
// <copyright file = "Color.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights
{
    using System;

    /// <summary>
    /// Represents a color.
    /// </summary>
    /// <remarks>
    /// The API for this structure is largely based on the similarly named structure in System.Drawing.
    /// </remarks>
    public struct Color : IEquatable<Color>
    {
        private bool initialized;
        private int argb;

        /// <summary>
        /// The static constructor.
        /// </summary>
        static Color()
        {
            var newColor = FromRgb(1, 1, 1);
            newColor.IsLinkedColor = true;
            LinkedColor = newColor;
        }

        #region Public Properties

        /// <summary>
        /// Indicates if the color structure is empty.
        /// </summary>
        public bool IsEmpty => !initialized;

        /// <summary>
        /// The alpha value of the color.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "A")]
        public byte A
        {
            get;
            private set;
        }

        /// <summary>
        /// The red value of the color.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "R")]
        public byte R
        {
            get;
            private set;
        }
        
        /// <summary>
        /// The green value of the color.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "G")]
        public byte G
        {
            get;
            private set;
        }

        /// <summary>
        /// The blue value of the color.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "B")]
        public byte B
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets if this color represents the linked color.
        /// </summary>
        public bool IsLinkedColor
        {
            get;
            private set;
        }

        #region Predefined Colors

        /// <summary>
        /// Returns an empty color structure.
        /// </summary>
        public static Color Empty => new Color { initialized = false };

        ///<summary>Gets the predefined color AliceBlue.</summary>
        public static Color AliceBlue => FromArgb(255, 240, 248, 255);

        ///<summary>Gets the predefined color AntiqueWhite.</summary>
        public static Color AntiqueWhite => FromArgb(255, 250, 235, 215);

        ///<summary>Gets the predefined color Aqua.</summary>
        public static Color Aqua => FromArgb(255, 0, 255, 255);

        ///<summary>Gets the predefined color Aquamarine.</summary>
        public static Color Aquamarine => FromArgb(255, 127, 255, 212);

        ///<summary>Gets the predefined color Azure.</summary>
        public static Color Azure => FromArgb(255, 240, 255, 255);

        ///<summary>Gets the predefined color Beige.</summary>
        public static Color Beige => FromArgb(255, 245, 245, 220);

        ///<summary>Gets the predefined color Bisque.</summary>
        public static Color Bisque => FromArgb(255, 255, 228, 196);

        ///<summary>Gets the predefined color Black.</summary>
        public static Color Black => FromArgb(255, 0, 0, 0);

        ///<summary>Gets the predefined color BlanchedAlmond.</summary>
        public static Color BlanchedAlmond => FromArgb(255, 255, 235, 205);

        ///<summary>Gets the predefined color Blue.</summary>
        public static Color Blue => FromArgb(255, 0, 0, 255);

        ///<summary>Gets the predefined color BlueViolet.</summary>
        public static Color BlueViolet => FromArgb(255, 138, 43, 226);

        ///<summary>Gets the predefined color Brown.</summary>
        public static Color Brown => FromArgb(255, 165, 42, 42);

        ///<summary>Gets the predefined color BurlyWood.</summary>
        public static Color BurlyWood => FromArgb(255, 222, 184, 135);

        ///<summary>Gets the predefined color CadetBlue.</summary>
        public static Color CadetBlue => FromArgb(255, 95, 158, 160);

        ///<summary>Gets the predefined color Chartreuse.</summary>
        public static Color Chartreuse => FromArgb(255, 127, 255, 0);

        ///<summary>Gets the predefined color Chocolate.</summary>
        public static Color Chocolate => FromArgb(255, 210, 105, 30);

        ///<summary>Gets the predefined color Coral.</summary>
        public static Color Coral => FromArgb(255, 255, 127, 80);

        ///<summary>Gets the predefined color CornflowerBlue.</summary>
        public static Color CornflowerBlue => FromArgb(255, 100, 149, 237);

        ///<summary>Gets the predefined color Cornsilk.</summary>
        public static Color Cornsilk => FromArgb(255, 255, 248, 220);

        ///<summary>Gets the predefined color Crimson.</summary>
        public static Color Crimson => FromArgb(255, 220, 20, 60);

        ///<summary>Gets the predefined color Cyan.</summary>
        public static Color Cyan => FromArgb(255, 0, 255, 255);

        ///<summary>Gets the predefined color DarkBlue.</summary>
        public static Color DarkBlue => FromArgb(255, 0, 0, 139);

        ///<summary>Gets the predefined color DarkCyan.</summary>
        public static Color DarkCyan => FromArgb(255, 0, 139, 139);

        ///<summary>Gets the predefined color DarkGoldenrod.</summary>
        public static Color DarkGoldenrod => FromArgb(255, 184, 134, 11);

        ///<summary>Gets the predefined color DarkGray.</summary>
        public static Color DarkGray => FromArgb(255, 169, 169, 169);

        ///<summary>Gets the predefined color DarkGreen.</summary>
        public static Color DarkGreen => FromArgb(255, 0, 100, 0);

        ///<summary>Gets the predefined color DarkKhaki.</summary>
        public static Color DarkKhaki => FromArgb(255, 189, 183, 107);

        ///<summary>Gets the predefined color DarkMagenta.</summary>
        public static Color DarkMagenta => FromArgb(255, 139, 0, 139);

        ///<summary>Gets the predefined color DarkOliveGreen.</summary>
        public static Color DarkOliveGreen => FromArgb(255, 85, 107, 47);

        ///<summary>Gets the predefined color DarkOrange.</summary>
        public static Color DarkOrange => FromArgb(255, 255, 140, 0);

        ///<summary>Gets the predefined color DarkOrchid.</summary>
        public static Color DarkOrchid => FromArgb(255, 153, 50, 204);

        ///<summary>Gets the predefined color DarkRed.</summary>
        public static Color DarkRed => FromArgb(255, 139, 0, 0);

        ///<summary>Gets the predefined color DarkSalmon.</summary>
        public static Color DarkSalmon => FromArgb(255, 233, 150, 122);

        ///<summary>Gets the predefined color DarkSeaGreen.</summary>
        public static Color DarkSeaGreen => FromArgb(255, 143, 188, 139);

        ///<summary>Gets the predefined color DarkSlateBlue.</summary>
        public static Color DarkSlateBlue => FromArgb(255, 72, 61, 139);

        ///<summary>Gets the predefined color DarkSlateGray.</summary>
        public static Color DarkSlateGray => FromArgb(255, 47, 79, 79);

        ///<summary>Gets the predefined color DarkTurquoise.</summary>
        public static Color DarkTurquoise => FromArgb(255, 0, 206, 209);

        ///<summary>Gets the predefined color DarkViolet.</summary>
        public static Color DarkViolet => FromArgb(255, 148, 0, 211);

        ///<summary>Gets the predefined color DeepPink.</summary>
        public static Color DeepPink => FromArgb(255, 255, 20, 147);

        ///<summary>Gets the predefined color DeepSkyBlue.</summary>
        public static Color DeepSkyBlue => FromArgb(255, 0, 191, 255);

        ///<summary>Gets the predefined color DimGray.</summary>
        public static Color DimGray => FromArgb(255, 105, 105, 105);

        ///<summary>Gets the predefined color DodgerBlue.</summary>
        public static Color DodgerBlue => FromArgb(255, 30, 144, 255);

        ///<summary>Gets the predefined color Firebrick.</summary>
        public static Color Firebrick => FromArgb(255, 178, 34, 34);

        ///<summary>Gets the predefined color FloralWhite.</summary>
        public static Color FloralWhite => FromArgb(255, 255, 250, 240);

        ///<summary>Gets the predefined color ForestGreen.</summary>
        public static Color ForestGreen => FromArgb(255, 34, 139, 34);

        ///<summary>Gets the predefined color Fuchsia.</summary>
        public static Color Fuchsia => FromArgb(255, 255, 0, 255);

        ///<summary>Gets the predefined color Gainsboro.</summary>
        public static Color Gainsboro => FromArgb(255, 220, 220, 220);

        ///<summary>Gets the predefined color GhostWhite.</summary>
        public static Color GhostWhite => FromArgb(255, 248, 248, 255);

        ///<summary>Gets the predefined color Gold.</summary>
        public static Color Gold => FromArgb(255, 255, 215, 0);

        ///<summary>Gets the predefined color Goldenrod.</summary>
        public static Color Goldenrod => FromArgb(255, 218, 165, 32);

        ///<summary>Gets the predefined color Gray.</summary>
        public static Color Gray => FromArgb(255, 128, 128, 128);

        ///<summary>Gets the predefined color Green.</summary>
        public static Color Green => FromArgb(255, 0, 128, 0);

        ///<summary>Gets the predefined color GreenYellow.</summary>
        public static Color GreenYellow => FromArgb(255, 173, 255, 47);

        ///<summary>Gets the predefined color Honeydew.</summary>
        public static Color Honeydew => FromArgb(255, 240, 255, 240);

        ///<summary>Gets the predefined color HotPink.</summary>
        public static Color HotPink => FromArgb(255, 255, 105, 180);

        ///<summary>Gets the predefined color IndianRed.</summary>
        public static Color IndianRed => FromArgb(255, 205, 92, 92);

        ///<summary>Gets the predefined color Indigo.</summary>
        public static Color Indigo => FromArgb(255, 75, 0, 130);

        ///<summary>Gets the predefined color Ivory.</summary>
        public static Color Ivory => FromArgb(255, 255, 255, 240);

        ///<summary>Gets the predefined color Khaki.</summary>
        public static Color Khaki => FromArgb(255, 240, 230, 140);

        ///<summary>Gets the predefined color Lavender.</summary>
        public static Color Lavender => FromArgb(255, 230, 230, 250);

        ///<summary>Gets the predefined color LavenderBlush.</summary>
        public static Color LavenderBlush => FromArgb(255, 255, 240, 245);

        ///<summary>Gets the predefined color LawnGreen.</summary>
        public static Color LawnGreen => FromArgb(255, 124, 252, 0);

        ///<summary>Gets the predefined color LemonChiffon.</summary>
        public static Color LemonChiffon => FromArgb(255, 255, 250, 205);

        ///<summary>Gets the predefined color LightBlue.</summary>
        public static Color LightBlue => FromArgb(255, 173, 216, 230);

        ///<summary>Gets the predefined color LightCoral.</summary>
        public static Color LightCoral => FromArgb(255, 240, 128, 128);

        ///<summary>Gets the predefined color LightCyan.</summary>
        public static Color LightCyan => FromArgb(255, 224, 255, 255);

        ///<summary>Gets the predefined color LightGoldenrodYellow.</summary>
        public static Color LightGoldenrodYellow => FromArgb(255, 250, 250, 210);

        ///<summary>Gets the predefined color LightGreen.</summary>
        public static Color LightGreen => FromArgb(255, 144, 238, 144);

        ///<summary>Gets the predefined color LightGray.</summary>
        public static Color LightGray => FromArgb(255, 211, 211, 211);

        ///<summary>Gets the predefined color LightPink.</summary>
        public static Color LightPink => FromArgb(255, 255, 182, 193);

        ///<summary>Gets the predefined color LightSalmon.</summary>
        public static Color LightSalmon => FromArgb(255, 255, 160, 122);

        ///<summary>Gets the predefined color LightSeaGreen.</summary>
        public static Color LightSeaGreen => FromArgb(255, 32, 178, 170);

        ///<summary>Gets the predefined color LightSkyBlue.</summary>
        public static Color LightSkyBlue => FromArgb(255, 135, 206, 250);

        ///<summary>Gets the predefined color LightSlateGray.</summary>
        public static Color LightSlateGray => FromArgb(255, 119, 136, 153);

        ///<summary>Gets the predefined color LightSteelBlue.</summary>
        public static Color LightSteelBlue => FromArgb(255, 176, 196, 222);

        ///<summary>Gets the predefined color LightYellow.</summary>
        public static Color LightYellow => FromArgb(255, 255, 255, 224);

        ///<summary>Gets the predefined color Lime.</summary>
        public static Color Lime => FromArgb(255, 0, 255, 0);

        ///<summary>Gets the predefined color LimeGreen.</summary>
        public static Color LimeGreen => FromArgb(255, 50, 205, 50);

        ///<summary>Gets the predefined color Linen.</summary>
        public static Color Linen => FromArgb(255, 250, 240, 230);

        ///<summary>Gets the predefined color Magenta.</summary>
        public static Color Magenta => FromArgb(255, 255, 0, 255);

        ///<summary>Gets the predefined color Maroon.</summary>
        public static Color Maroon => FromArgb(255, 128, 0, 0);

        ///<summary>Gets the predefined color MediumAquamarine.</summary>
        public static Color MediumAquamarine => FromArgb(255, 102, 205, 170);

        ///<summary>Gets the predefined color MediumBlue.</summary>
        public static Color MediumBlue => FromArgb(255, 0, 0, 205);

        ///<summary>Gets the predefined color MediumOrchid.</summary>
        public static Color MediumOrchid => FromArgb(255, 186, 85, 211);

        ///<summary>Gets the predefined color MediumPurple.</summary>
        public static Color MediumPurple => FromArgb(255, 147, 112, 219);

        ///<summary>Gets the predefined color MediumSeaGreen.</summary>
        public static Color MediumSeaGreen => FromArgb(255, 60, 179, 113);

        ///<summary>Gets the predefined color MediumSlateBlue.</summary>
        public static Color MediumSlateBlue => FromArgb(255, 123, 104, 238);

        ///<summary>Gets the predefined color MediumSpringGreen.</summary>
        public static Color MediumSpringGreen => FromArgb(255, 0, 250, 154);

        ///<summary>Gets the predefined color MediumTurquoise.</summary>
        public static Color MediumTurquoise => FromArgb(255, 72, 209, 204);

        ///<summary>Gets the predefined color MediumVioletRed.</summary>
        public static Color MediumVioletRed => FromArgb(255, 199, 21, 133);

        ///<summary>Gets the predefined color MidnightBlue.</summary>
        public static Color MidnightBlue => FromArgb(255, 25, 25, 112);

        ///<summary>Gets the predefined color MintCream.</summary>
        public static Color MintCream => FromArgb(255, 245, 255, 250);

        ///<summary>Gets the predefined color MistyRose.</summary>
        public static Color MistyRose => FromArgb(255, 255, 228, 225);

        ///<summary>Gets the predefined color Moccasin.</summary>
        public static Color Moccasin => FromArgb(255, 255, 228, 181);

        ///<summary>Gets the predefined color NavajoWhite.</summary>
        public static Color NavajoWhite => FromArgb(255, 255, 222, 173);

        ///<summary>Gets the predefined color Navy.</summary>
        public static Color Navy => FromArgb(255, 0, 0, 128);

        ///<summary>Gets the predefined color OldLace.</summary>
        public static Color OldLace => FromArgb(255, 253, 245, 230);

        ///<summary>Gets the predefined color Olive.</summary>
        public static Color Olive => FromArgb(255, 128, 128, 0);

        ///<summary>Gets the predefined color OliveDrab.</summary>
        public static Color OliveDrab => FromArgb(255, 107, 142, 35);

        ///<summary>Gets the predefined color Orange.</summary>
        public static Color Orange => FromArgb(255, 255, 165, 0);

        ///<summary>Gets the predefined color OrangeRed.</summary>
        public static Color OrangeRed => FromArgb(255, 255, 69, 0);

        ///<summary>Gets the predefined color Orchid.</summary>
        public static Color Orchid => FromArgb(255, 218, 112, 214);

        ///<summary>Gets the predefined color PaleGoldenrod.</summary>
        public static Color PaleGoldenrod => FromArgb(255, 238, 232, 170);

        ///<summary>Gets the predefined color PaleGreen.</summary>
        public static Color PaleGreen => FromArgb(255, 152, 251, 152);

        ///<summary>Gets the predefined color PaleTurquoise.</summary>
        public static Color PaleTurquoise => FromArgb(255, 175, 238, 238);

        ///<summary>Gets the predefined color PaleVioletRed.</summary>
        public static Color PaleVioletRed => FromArgb(255, 219, 112, 147);

        ///<summary>Gets the predefined color PapayaWhip.</summary>
        public static Color PapayaWhip => FromArgb(255, 255, 239, 213);

        ///<summary>Gets the predefined color PeachPuff.</summary>
        public static Color PeachPuff => FromArgb(255, 255, 218, 185);

        ///<summary>Gets the predefined color Peru.</summary>
        public static Color Peru => FromArgb(255, 205, 133, 63);

        ///<summary>Gets the predefined color Pink.</summary>
        public static Color Pink => FromArgb(255, 255, 192, 203);

        ///<summary>Gets the predefined color Plum.</summary>
        public static Color Plum => FromArgb(255, 221, 160, 221);

        ///<summary>Gets the predefined color PowderBlue.</summary>
        public static Color PowderBlue => FromArgb(255, 176, 224, 230);

        ///<summary>Gets the predefined color Purple.</summary>
        public static Color Purple => FromArgb(255, 128, 0, 128);

        ///<summary>Gets the predefined color Red.</summary>
        public static Color Red => FromArgb(255, 255, 0, 0);

        ///<summary>Gets the predefined color RosyBrown.</summary>
        public static Color RosyBrown => FromArgb(255, 188, 143, 143);

        ///<summary>Gets the predefined color RoyalBlue.</summary>
        public static Color RoyalBlue => FromArgb(255, 65, 105, 225);

        ///<summary>Gets the predefined color SaddleBrown.</summary>
        public static Color SaddleBrown => FromArgb(255, 139, 69, 19);

        ///<summary>Gets the predefined color Salmon.</summary>
        public static Color Salmon => FromArgb(255, 250, 128, 114);

        ///<summary>Gets the predefined color SandyBrown.</summary>
        public static Color SandyBrown => FromArgb(255, 244, 164, 96);

        ///<summary>Gets the predefined color SeaGreen.</summary>
        public static Color SeaGreen => FromArgb(255, 46, 139, 87);

        ///<summary>Gets the predefined color Seashell.</summary>
        public static Color Seashell => FromArgb(255, 255, 245, 238);

        ///<summary>Gets the predefined color Sienna.</summary>
        public static Color Sienna => FromArgb(255, 160, 82, 45);

        ///<summary>Gets the predefined color Silver.</summary>
        public static Color Silver => FromArgb(255, 192, 192, 192);

        ///<summary>Gets the predefined color SkyBlue.</summary>
        public static Color SkyBlue => FromArgb(255, 135, 206, 235);

        ///<summary>Gets the predefined color SlateBlue.</summary>
        public static Color SlateBlue => FromArgb(255, 106, 90, 205);

        ///<summary>Gets the predefined color SlateGray.</summary>
        public static Color SlateGray => FromArgb(255, 112, 128, 144);

        ///<summary>Gets the predefined color Snow.</summary>
        public static Color Snow => FromArgb(255, 255, 250, 250);

        ///<summary>Gets the predefined color SpringGreen.</summary>
        public static Color SpringGreen => FromArgb(255, 0, 255, 127);

        ///<summary>Gets the predefined color SteelBlue.</summary>
        public static Color SteelBlue => FromArgb(255, 70, 130, 180);

        ///<summary>Gets the predefined color Tan.</summary>
        public static Color Tan => FromArgb(255, 210, 180, 140);

        ///<summary>Gets the predefined color Teal.</summary>
        public static Color Teal => FromArgb(255, 0, 128, 128);

        ///<summary>Gets the predefined color Thistle.</summary>
        public static Color Thistle => FromArgb(255, 216, 191, 216);

        ///<summary>Gets the predefined color Tomato.</summary>
        public static Color Tomato => FromArgb(255, 255, 99, 71);

        ///<summary>Gets the predefined color Turquoise.</summary>
        public static Color Turquoise => FromArgb(255, 64, 224, 208);

        ///<summary>Gets the predefined color Violet.</summary>
        public static Color Violet => FromArgb(255, 238, 130, 238);

        ///<summary>Gets the predefined color Wheat.</summary>
        public static Color Wheat => FromArgb(255, 245, 222, 179);

        ///<summary>Gets the predefined color White.</summary>
        public static Color White => FromArgb(255, 255, 255, 255);

        ///<summary>Gets the predefined color WhiteSmoke.</summary>
        public static Color WhiteSmoke => FromArgb(255, 245, 245, 245);

        ///<summary>Gets the predefined color Yellow.</summary>
        public static Color Yellow => FromArgb(255, 255, 255, 0);

        ///<summary>Gets the predefined color YellowGreen.</summary>
        public static Color YellowGreen => FromArgb(255, 154, 205, 50);

        #endregion

        /// <summary>
        /// Gets the linked color.
        /// </summary>
        public static Color LinkedColor
        {
            get;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a color from a 32-bit number.
        /// </summary>
        /// <param name="alpha">The alpha level of the color.</param>
        /// <param name="red">The red level of the color.</param>
        /// <param name="green">The green level of the color.</param>
        /// <param name="blue">The blue level of the color.</param>
        /// <returns>A new color from the color levels specified.</returns>
        public static Color FromArgb(byte alpha, byte red, byte green, byte blue)
        {
            var color = new Color
            {
                A = alpha,
                R = red,
                G = green,
                B = blue,
                initialized = true,
                argb = (alpha << 24) | (red << 16) | (green << 8) | blue
            };

            return color;
        }

        /// <summary>
        /// Creates a color from a 24-bit number.
        /// </summary>
        /// <param name="red">The red level of the color.</param>
        /// <param name="green">The green level of the color.</param>
        /// <param name="blue">The blue level of the color.</param>
        /// <returns>A new color from the color levels specified.</returns>
        public static Color FromRgb(byte red, byte green, byte blue)
        {
            return FromArgb(255, red, green, blue);
        }

        /// <summary>
        /// Tests if two color structures are the same.
        /// </summary>
        /// <param name="left">The Color that is to the left of the equality operator.</param>
        /// <param name="right">The Color that is to the right of the equality operator.</param>
        /// <returns>True if they are the same otherwise false.</returns>
        public static bool operator ==(Color left, Color right)
        {
            return left.argb == right.argb && left.initialized == right.initialized;
        }

        /// <summary>
        /// Tests if two color structures are different.
        /// </summary>
        /// <param name="left">The Color that is to the left of the inequality operator.</param>
        /// <param name="right">The Color that is to the right of the inequality operator.</param>
        /// <returns>True if they are the different otherwise false.</returns>
        public static bool operator !=(Color left, Color right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public override bool Equals (object obj)
        {
            if(obj is Color)
            {
                return this == (Color)obj;
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode ()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        public bool Equals(Color other)
        {
            return this == other;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return IsEmpty ? "(Empty)" : $"{A},{R},{G},{B}";
        }

        #endregion
    }
}
