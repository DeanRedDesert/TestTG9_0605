//-----------------------------------------------------------------------
// <copyright file = "PointerFlashRecord.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;

    /// <summary>
    /// Defines a pointer flash record. A pointer is a light group on the USB nested wheel.
    /// </summary>
    public struct PointerFlashRecord : IEquatable<PointerFlashRecord>
    {
        #region Light Data

        /// <summary>
        /// The number of pointers on each of the USB nested wheels.
        /// </summary>
        private const byte NumberOfPointersSupported = 3;

        #endregion Light Data

        #region Public Properties

        /// <summary>
        /// The zero-based index of the pointer to flash.
        /// </summary>
        public byte PointerIndex { get; }

        /// <summary>
        /// The color for half of the flash duration.
        /// </summary>
        public Color Color1 { get; }

        /// <summary>
        /// The color for the other half of the flash duration.
        /// </summary>
        public Color Color2 { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a pointer flash record.
        /// </summary>
        /// <param name="pointerIndex">The zero based index of the pointer to flash.</param>
        /// <param name="color1">Color for half of the flash duration. All pointer's color 1 is synchronized.</param>
        /// <param name="color2">Color for half of the flash duration. All pointer's color 2 is synchronized.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="pointerIndex"/> is invalid.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="color1"/> or the <paramref name="color2"/> is an empty color.
        /// </exception>
        public PointerFlashRecord(byte pointerIndex, Color color1, Color color2) : this()
        {
            if(pointerIndex > NumberOfPointersSupported)
            {
                throw new ArgumentOutOfRangeException(nameof(pointerIndex));
            }

            if(color1 == Color.Empty)
            {
                throw new ArgumentException("Color 1 cannot be empty.", nameof(color1));
            }

            if(color2 == Color.Empty)
            {
                throw new ArgumentException("Color 2 cannot be empty.", nameof(color2));
            }

            PointerIndex = pointerIndex;
            Color1 = color1;
            Color2 = color2;
        }

        #endregion

        #region IEquatable<PointerFlashRecord>

        /// <inheritdoc />
        public bool Equals(PointerFlashRecord other)
        {
            return other == this;
        }

        #endregion IEquatable<PointerFlashRecord>

        #region Object Overrides

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if(obj is PointerFlashRecord record)
            {
                return this == record;
            }
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (((PointerIndex.GetHashCode() * 39) ^ Color1.R.GetHashCode() +
                    Color1.G.GetHashCode() + Color1.B.GetHashCode() + Color1.A.GetHashCode()) * 23) ^
                    (Color2.R.GetHashCode() + Color2.G.GetHashCode() + Color2.B.GetHashCode() + Color2.A.GetHashCode());
                return hashCode;
            }
        }

        #endregion Object Overrides

        #region Equalities

        /// <summary>
        /// Tests if the two PointerFlashRecord structures are the same.
        /// </summary>
        /// <param name="record1">The record that is to the left of the equality operator.</param>
        /// <param name="record2">The record that is to the right of the equality operator.</param>
        /// <returns>True if they are the same otherwise false.</returns>
        public static bool operator ==(PointerFlashRecord record1, PointerFlashRecord record2)
        {
            return record1.PointerIndex == record2.PointerIndex && record1.Color1 == record2.Color1 &&
                   record1.Color2 == record2.Color2;
        }

        /// <summary>
        /// Tests if the two PointerFlashRecord structures are different.
        /// </summary>
        /// <param name="record1">The record that is to the left of the inequality operator.</param>
        /// <param name="record2">The record that is to the right of the inequality operator.</param>
        /// <returns>True if they are the same otherwise false.</returns>
        public static bool operator !=(PointerFlashRecord record1, PointerFlashRecord record2)
        {
            return !(record1 == record2);
        }

        #endregion Equalities
    }
}
