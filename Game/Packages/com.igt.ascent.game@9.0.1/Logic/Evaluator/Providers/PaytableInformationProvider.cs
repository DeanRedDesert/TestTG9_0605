//-----------------------------------------------------------------------
// <copyright file = "PaytableInformationProvider.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Providers
{
    using System;
    using CompactSerialization;
    using Schemas;
    using Services;

    /// <summary>
    /// Provides information about the current paytable.
    /// </summary>
    public class PaytableInformationProvider : ICompactSerializable
    {

        /// <summary>
        /// Default constructor for the PaytableInformationProvider.
        /// </summary>
        public PaytableInformationProvider()
        {

        }

        /// <summary>
        /// Constructor containing the paytable for the PaytableInformationProvider. 
        /// </summary>
        /// <param name="paytable">The current paytable.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="paytable"/> is null.</exception>
        public PaytableInformationProvider(Paytable paytable) : this()
        {
            if(paytable == null)
            {
                throw new ArgumentNullException("paytable");
            }

            MaxPaybackPercentage = Convert.ToSingle(paytable.Abstract.MaxPaybackPercentage);
            MinPaybackPercentage = Convert.ToSingle(paytable.Abstract.MinPaybackPercentage);
            MinPaybackPercentageWithoutProgressiveContribution =
                Convert.ToSingle(paytable.Abstract.MinPaybackPercentageWithoutProgressiveContributions);
            MinPaybackPercentageWithoutProgressiveContributionSpecified =
                paytable.Abstract.MinPaybackPercentageWithoutProgressiveContributionsSpecified;
            GameId = paytable.Abstract.gameID;

        }

        /// <summary>
        /// The maximum paytable payback percentage.
        /// </summary>
        [GameService]
        public float MaxPaybackPercentage
        {
            get;
            private set;
        }

        /// <summary>
        /// The minimum paytable payback percentage.
        /// </summary>
        [GameService]
        public float MinPaybackPercentage
        {
            get;
            private set;
        }

        /// <summary>
        /// The minimum paytable payback percentage without progressive contribution.
        /// </summary>
        [GameService]
        public float MinPaybackPercentageWithoutProgressiveContribution
        {
            get;
            private set;
        }

        /// <summary>
        /// Flag to indicate if minimum pay back percentage without progressive contribution is specified.
        /// </summary>
        [GameService]
        public bool MinPaybackPercentageWithoutProgressiveContributionSpecified
        {
            get;
            private set;
        }

        /// <summary>
        /// The game id for the paytable.
        /// </summary>
        [GameService]
        public string GameId
        {
            get;
            private set;
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Deserialize(System.IO.Stream stream)
        {
            MaxPaybackPercentage = CompactSerializer.ReadFloat(stream);
            MinPaybackPercentage = CompactSerializer.ReadFloat(stream);
            MinPaybackPercentageWithoutProgressiveContribution = CompactSerializer.ReadFloat(stream);
            MinPaybackPercentageWithoutProgressiveContributionSpecified = CompactSerializer.ReadBool(stream);
            GameId = CompactSerializer.ReadString(stream);
        }

        /// <inheritdoc />
        public void Serialize(System.IO.Stream stream)
        {
            CompactSerializer.Write(stream, MaxPaybackPercentage);
            CompactSerializer.Write(stream, MinPaybackPercentage);
            CompactSerializer.Write(stream, MinPaybackPercentageWithoutProgressiveContribution);
            CompactSerializer.Write(stream, MinPaybackPercentageWithoutProgressiveContributionSpecified);
            CompactSerializer.Write(stream, GameId);
        }

        #endregion
    }
}