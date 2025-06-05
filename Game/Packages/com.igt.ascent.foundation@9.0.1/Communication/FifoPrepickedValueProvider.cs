//-----------------------------------------------------------------------
// <copyright file = "FifoPrepickedValueProvider.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// An implementation of <see cref="IPrepickedValueProvider"/> that provides values in the order that they
    /// were set on the provider.
    /// </summary>
    /// <remarks>
    /// This provider uses a <see cref="Queue{T}"/> of <see cref="int"/> values that is cleared every time the
    /// <see cref="SetValues"/> method is called.
    /// 
    /// When <see cref="GetPrepickedValues(RandomValueRequest)"/> is called,
    /// the values in the queue are checked against the incoming <see cref="RandomValueRequest"/> to ensure that
    /// the values being provided are in the ranges specified by the request.
    /// </remarks>
    public class FifoPrepickedValueProvider : IPrepickedValueProvider
    {
        private readonly Queue<int> valuesToProvide = new Queue<int>();

        #region Implementation of IPrepickedValueProvider

        /// <inheritdoc/>
        public PrepickResult GetPrepickedValues(RandomValueRequest randomValueRequest)
        {
            var updatedPrepickedValues = new List<int>();
            if (randomValueRequest.PrePickedNumbers != null)
            {
                updatedPrepickedValues.AddRange(randomValueRequest.PrePickedNumbers);
            }
            for (int i = updatedPrepickedValues.Count; i < randomValueRequest.Count; i++)
            {
                if (valuesToProvide.Count > 0)
                {
                    var prepickedValue = valuesToProvide.Dequeue();
                    if (prepickedValue < randomValueRequest.RangeMin || prepickedValue > randomValueRequest.RangeMax)
                    {
                        var message = string.Format("The value must be between {0} and {1}, inclusive.",
                                                    randomValueRequest.RangeMin,
                                                    randomValueRequest.RangeMax);
                        throw new ArgumentOutOfRangeException("randomValueRequest", prepickedValue, message);
                    }
                    if (updatedPrepickedValues.Count(x => x == prepickedValue) == randomValueRequest.MaxDuplicates + 1)
                    {
                        var message = string.Format("There are already {0} duplicates in the collection.",
                                                    randomValueRequest.MaxDuplicates);
                        throw new ArgumentOutOfRangeException("randomValueRequest", prepickedValue, message);
                    }
                    updatedPrepickedValues.Add(prepickedValue);
                }
            }
            return new PrepickResult(randomValueRequest, updatedPrepickedValues);
        }

        /// <inheritdoc/>
        public ICollection<PrepickResult> GetPrepickedValues(IEnumerable<RandomValueRequest> requests)
        {
            return requests.Select<RandomValueRequest, PrepickResult>(GetPrepickedValues).ToList();
        }

        /// <inheritdoc/>
        public void SetValues(IEnumerable<int> values)
        {
            valuesToProvide.Clear();
            foreach (var item in values)
            {
                valuesToProvide.Enqueue(item);
            }
        }

        #endregion
    }
}
