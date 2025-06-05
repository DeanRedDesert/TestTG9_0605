// -----------------------------------------------------------------------
// <copyright file = "DisplayControlEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Text;

    /// <inheritdoc cref="PlatformEventArgs"/>
    /// <summary>
    /// Event indicating a change of the display control state.
    /// </summary>
    [Serializable]
    public sealed class DisplayControlEventArgs : PlatformEventArgs, ITransactionDowngrade<DisplayControlEventArgs>
    {
        #region Properties

        /// <summary>
        /// Gets the display control state the application should use for its presentation.
        /// </summary>
        public DisplayControlState DisplayControlState { get; private set; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        /// <summary>
        /// Initializes a new instance of <see cref="DisplayControlEventArgs"/>
        /// with heavyweight transaction.
        /// </summary>
        /// <param name="displayControlState">
        /// The display control state the application should use for its presentation.
        /// </param>
        public DisplayControlEventArgs(DisplayControlState displayControlState)
            : this(displayControlState, TransactionWeight.Heavy)
        {
        }

        /// <inheritdoc/>
        /// <summary>
        /// Initializes a new instance of <see cref="DisplayControlEventArgs"/>
        /// with specified transaction weight.
        /// </summary>
        /// <param name="displayControlState">
        /// The display control state the application should use for its presentation.
        /// </param>
        /// <param name="transactionWeight">
        /// The transaction weight of the event.
        /// </param>
        private DisplayControlEventArgs(DisplayControlState displayControlState, TransactionWeight transactionWeight)
            : base(transactionWeight)
        {
            DisplayControlState = displayControlState;
        }

        #endregion

        #region ITransactionDowngrade Implementation

        /// <inheritdoc/>
        public DisplayControlEventArgs Downgrade(TransactionWeight newTransactionWeight)
        {
            if(newTransactionWeight >= TransactionWeight)
            {
                throw new InvalidOperationException(
                    $"New transaction weight {newTransactionWeight} is no less than the current value {TransactionWeight}");
            }

            return new DisplayControlEventArgs(DisplayControlState, newTransactionWeight);
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("DisplayControlEventArgs -");
            builder.Append(base.ToString());
            builder.AppendLine("\t DisplayControlState: " + DisplayControlState);

            return builder.ToString();
        }

        #endregion

    }
}