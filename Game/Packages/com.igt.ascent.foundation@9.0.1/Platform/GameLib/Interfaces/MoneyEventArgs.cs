//-----------------------------------------------------------------------
// <copyright file = "MoneyEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Text;
    using Platform.Interfaces;

    /// <summary>
    /// Event represents a money operation that causes one or more of the
    /// player meters to change..
    /// </summary>
    [Serializable]
    public class MoneyEventArgs : EventArgs
    {
        /// <summary>
        /// Type of the money event.
        /// </summary>
        public MoneyEventType Type { get; private set; }

        /// <summary>
        /// The origination of the money movement.
        /// Will be Unknown if it's meaningless for the event type.
        /// </summary>
        public MoneyLocation From { get; private set; }

        /// <summary>
        /// The destination of the money movement.
        /// Will be Unknown if it's meaningless for the event type.
        /// </summary>
        public MoneyLocation To { get; private set; }

        /// <summary>
        /// Get the source that the money is received from.
        /// </summary>
        public MoneyInSource MoneyInSource { get; private set; }

        /// <summary>
        /// Get the source that the money is taken away from.
        /// </summary>
        public MoneyOutSource MoneyOutSource { get; private set; }

        /// <summary>
        /// The amount of the money involved in the operation
        /// in units of the specified denomination.
        /// </summary>
        public long Value { get; private set; }

        /// <summary>
        /// The denomination of the money value.
        /// </summary>
        public long Denomination { get; private set; }

        /// <summary>
        /// The values of the set of Player Meters after the money change,
        /// in base units.
        /// </summary>
        public PlayerMeters Meters { get; private set; }

        /// <summary>
        /// Gets the funds transfer account source of the received money. 
        /// </summary>
        /// <remarks>
        /// This information is set when the <see cref="MoneyInSource"/> is <see fref="MoneyInSource.FundsTransfer"/>. 
        /// In all other cases this value is not set and null will be returned.
        /// </remarks>
        public FundsTransferAccountSource? FundsTransferAccountSource { get; private set; }

        /// <summary>
        /// Constructs a MoneyEventArgs using default from and to locations and a 0 amount.
        /// </summary>
        /// <remarks>Intended for events which modify the meters, but do not have an associated amount.</remarks>
        /// <param name="type">Type of the money event.</param>
        /// <param name="meters">The values of the set of Player Meters after the
        ///                      money change, in base units.</param>
        public MoneyEventArgs(MoneyEventType type, PlayerMeters meters)
            : this(type, 0, 1, meters)
        {
        }

        /// <summary>
        /// Constructs a MoneyEventArgs of MoneyIn type that indicates a given amount of money 
        /// has been added to the EGM by a specific source.
        /// </summary>
        /// <param name="moneyInSource">The source that the money is received from.</param>
        /// <param name="value">The amount of the money involved in the operation
        ///                     in units of the specified denomination.</param>
        /// <param name="denomination">The denomination of the money value.</param>
        /// <param name="meters">The values of the set of Player Meters after the
        ///                      money change, in base units.</param>
        public MoneyEventArgs(MoneyInSource moneyInSource, long value, long denomination, PlayerMeters meters)
            : this(MoneyEventType.MoneyIn, value, denomination, meters, moneyInSource)
        {
        }

        /// <summary>
        /// Constructs a MoneyEventArgs of MoneyIn type that indicates a given amount of money 
        /// has been added to the EGM by a specific source and a specified transfer account source.
        /// </summary>
        /// <remarks>
        /// Do not use this constructor if the transfer account source is not specified.
        /// </remarks>
        /// <param name="moneyInSource">The source that the money is received from.</param>
        /// <param name="transferAccountSource">The transfer accounting source.</param>
        /// <param name="value">The amount of the money involved in the operation
        ///                     in units of the specified denomination.</param>
        /// <param name="denomination">The denomination of the money value.</param>
        /// <param name="meters">The values of the set of Player Meters after the
        ///                      money change, in base units.</param>
        public MoneyEventArgs(MoneyInSource moneyInSource, FundsTransferAccountSource transferAccountSource, long value, long denomination, PlayerMeters meters)
            : this(MoneyEventType.MoneyIn, value, denomination, meters, moneyInSource, fundsTransferAccountSource:transferAccountSource)
        {
        }

        /// <summary>
        /// Constructs a MoneyEventArgs of MoneyOut type that indicates a given amount of money in machine
        /// has been taken away from a specific source.
        /// </summary>
        /// <param name="moneyOutSource">The source that the money is taken away from.</param>
        /// <param name="value">The amount of the money involved in the operation
        ///                     in units of the specified denomination.</param>
        /// <param name="denomination">The denomination of the money value.</param>
        /// <param name="meters">The values of the set of Player Meters after the
        ///                      money change, in base units.</param>
        public MoneyEventArgs(MoneyOutSource moneyOutSource, long value, long denomination, PlayerMeters meters)
            : this(MoneyEventType.MoneyOut, value, denomination, meters, moneyOutSource:moneyOutSource)
        {
        }

        /// <summary>
        /// Constructs a MoneyEventArgs using given parameters.
        /// </summary>
        /// <param name="type">
        /// Type of the money event.
        /// </param>
        /// <param name="value">
        /// The amount of the money involved in the operation
        /// in units of the specified denomination.
        /// </param>
        /// <param name="denomination">
        /// The denomination of the money value.
        /// </param>
        /// <param name="meters">
        /// The values of the set of Player Meters after the money change, in base units.
        /// </param>
        /// <param name="moneyInSource">
        /// The source that the money is received from.
        /// This parameter is optional.  If not specified, it is default to MoneyInSource.Invalid.
        /// </param>
        /// <param name="moneyOutSource">
        /// The source that the money is taken away from.
        /// This parameter is optional.  If not specified, it is default to MoneyOutSource.Invalid.
        /// </param>
        /// <param name="fundsTransferAccountSource">
        /// The funds transfer accounting source.
        /// This parameter is optional.  If not specified, it is default to null.
        /// </param>
        /// <param name="from">
        /// The origination of the money movement.
        /// This parameter is optional.  If not specified, it is default to MoneyLocation.Unknown.
        /// </param>
        /// <param name="to">
        /// The destination of the money movement.
        /// This parameter is optional.  If not specified, it is default to MoneyLocation.Unknown.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when money amount is less than 0, or denomination is less than 1
        /// </exception>
        public MoneyEventArgs(MoneyEventType type,
                              long value, long denomination, PlayerMeters meters,
                              MoneyInSource moneyInSource = MoneyInSource.Invalid,
                              MoneyOutSource moneyOutSource = MoneyOutSource.Invalid,
                              FundsTransferAccountSource? fundsTransferAccountSource = null,
                              MoneyLocation from = MoneyLocation.Unknown,
                              MoneyLocation to = MoneyLocation.Unknown)
        {
            if(value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be less than 0.");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(denomination), "Denomination must be greater than 0");
            }

            Type = type;
            From = from;
            To = to;
            MoneyInSource = moneyInSource;
            MoneyOutSource = moneyOutSource;
            Value = value;
            Denomination = denomination;
            Meters = meters;
            FundsTransferAccountSource = fundsTransferAccountSource;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("MoneyEvent -");
            builder.AppendLine("\t Type = " + Type);
            builder.AppendLine("\t From = " + From);
            builder.AppendLine("\t To = " + To);
            builder.AppendLine("\t Money In Source = " + MoneyInSource);
            builder.AppendLine("\t Money Out Source = " + MoneyOutSource);
            builder.AppendLine("\t Value = " + Value);
            builder.AppendLine("\t Denomination = " + Denomination);
            builder.AppendLine("\t Player Meters = " + Meters);
            if(FundsTransferAccountSource != null)
            {
                builder.AppendLine("\t FundsTransferAccountSource = " + FundsTransferAccountSource);
            }

            return builder.ToString();
        }
    }
}
