//-----------------------------------------------------------------------
// <copyright file = "IFeatureProgressiveAward.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList.Interfaces
{
    /// <summary>
    /// Type used for progressive wins associated with <see cref="IFeatureAward"/>.
    /// </summary>
    /// <remarks>
    /// Used to associate a progressive win/validation request with a payvar win.
    /// May be a child of the <see cref="IFeatureAward"/>.
    /// </remarks>
    public interface IFeatureProgressiveAward : IProgressiveAward
    {
        /// <summary>
        /// Gets the award (in base units) if the progressive is not a hit.
        /// </summary>
        /// <remarks>
        /// The amount value in base units to be added to the <see cref="IFeatureAward"/> amount by
        /// the Foundation if the progressive HitState is not a hit.  This additional
        /// amount is metered as a payvar/paytable win at the win level index of the
        /// associated <see cref="IFeatureAward"/>.
        /// </remarks>
        long ConsolationAmountValue { get; }
    }
}