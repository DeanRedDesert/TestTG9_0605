//-----------------------------------------------------------------------
// <copyright file = "MoneyInInformation.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using System.IO;
    using System.Text;
    using Ascent.Communication.Platform.Interfaces;
    using Cloneable;
    using CompactSerialization;
    using Session;

    /// <summary>
    /// This class gives the information of money in.
    /// </summary>
    /// <remarks>
    /// This class is used for the payload in the game service.
    /// </remarks>
    [Serializable]
    public class MoneyInInformation : ICompactSerializable, IDeepCloneable
    {
        /// <summary>
        /// Get the source that the money is received from.
        /// </summary>
        public MoneyInSource MoneyInSource { get; private set; }

        /// <summary>
        /// Get money in amount value in base units.
        /// </summary>
        public long Amount { get; private set; }

        /// <summary>
        /// Get money in session. Before using the payload,
        /// game service consumer need check if it is a new session id.
        /// </summary>
        public UniqueIdentifier MoneyInSession { get; private set; }

        /// <summary>
        /// Initialize an instance of <see cref="MoneyInInformation"/> with a unique identifier
        /// which indicates that a given amount of money has been added to the machine by a specific source.
        /// </summary>
        /// <param name="moneyInSource">The source that the money is received from.</param>
        /// <param name="amountValue">
        /// Amount value in base units that is received from money in source.
        /// </param>
        /// <param name="uniqueIdentifier">The unique identifier identifying this money in occurrence.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="amountValue"/> is less than zero.
        /// </exception>
        public MoneyInInformation(MoneyInSource moneyInSource, long amountValue, UniqueIdentifier uniqueIdentifier)
        {
            // Check the parameter.
            if(amountValue < 0)
            {
                throw new ArgumentOutOfRangeException("amountValue",
                    "Amount value is less than zero, please verify it.");
            }

            MoneyInSource = moneyInSource;
            Amount = amountValue;
            MoneyInSession = uniqueIdentifier;
        }

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <remarks>
        /// The default constructor which is used for deserialization purpose and not supposed to be invoked
        /// by the user code.
        /// </remarks>
        public MoneyInInformation()
            : this(MoneyInSource.Invalid, 0, null)
        {
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("MoneyInInformation -");
            builder.AppendLine("\t Money In Source = " + MoneyInSource);
            builder.AppendLine("\t Amount = " + Amount);
            builder.AppendLine(MoneyInSession == null ? "\t  MoneyInSession is null" : MoneyInSession.ToString());
            return builder.ToString();
        }

        #region IDeepCloneable Members

        /// <inheritDoc/>
        public object DeepClone()
        {
            // This type is supposed to be immutable. However, the invoking to Deserialize() of an existing instance could 
            // corrupt its immutibility. Thus, we must disallow such invoking for an instance already in use.
            return this;
        }

        #endregion

        #region ICompactSerializable Members

        /// <inheritDoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, MoneyInSource);
            CompactSerializer.Write(stream, Amount);
            CompactSerializer.Write(stream, MoneyInSession);
        }

        /// <inheritDoc/>
        public void Deserialize(Stream stream)
        {
            MoneyInSource = CompactSerializer.ReadEnum<MoneyInSource>(stream);
            Amount = CompactSerializer.ReadLong(stream);
            MoneyInSession = CompactSerializer.ReadSerializable<UniqueIdentifier>(stream);
        }

        #endregion
    }
}
