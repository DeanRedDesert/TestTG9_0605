//-----------------------------------------------------------------------
// <copyright file = "RNGSeederValueProvider.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// An implementation of <see cref="IPrepickedValueProvider"/> that provides values in the order that they
    /// were set on the provider. This class in particular is used for the RNG Seeder tool. It will not throw an
    /// exception in the case of being out of range or duplicates, but will inform the user of the RNG Seeder tool 
    /// through a message box.
    /// </summary>
    /// <remarks>
    /// This provider uses a <see cref="Queue{T}"/> of <see cref="int"/> values that is cleared every time the
    /// <see cref="SetValues"/> method is called.
    /// 
    /// When <see cref="GetPrepickedValues(RandomValueRequest)"/> is called,
    /// the values in the queue are checked against the incoming <see cref="RandomValueRequest"/> to ensure that
    /// the values being provided are in the ranges specified by the request.
    /// </remarks>
    public class RNGSeederValueProvider : IPrepickedValueProvider
    {
        /// <summary>
        /// Provides the values stored from the RNG Seeder Tool
        /// </summary>
        private readonly Queue<int> valuesToProvide = new Queue<int>();

        /// <summary>
        /// The communication line for the the RNG Seeder Tool. 
        /// This should only be used to inform the RNG Seeder if a value is out of range or a duplicate.
        /// </summary>
        private readonly IGameLogicAutomationService gameLogicAutomationService;

        #region Implementation of IPrepickedValueProvider

        /// <summary>
        /// Initializes a new instance of the RNGSeederValueProvider class with the given <see cref="IGameLogicAutomationService"/>
        /// object.
        /// </summary>
        /// <param name="gameLogicAutomationService"> Automation service connected to the RNG Seeder.</param>
        public RNGSeederValueProvider(IGameLogicAutomationService gameLogicAutomationService)
        {
            this.gameLogicAutomationService = gameLogicAutomationService; 
        }

        /// <inheritdoc/>
        public PrepickResult GetPrepickedValues(RandomValueRequest randomValueRequest)
        {
            var updatedPrepickedValues = new List<int>();
            if(randomValueRequest.PrePickedNumbers != null)
            {
                updatedPrepickedValues.AddRange(randomValueRequest.PrePickedNumbers);
            }

            for(int index = updatedPrepickedValues.Count; index < randomValueRequest.Count; ++index)
            {
                if(valuesToProvide.Count > 0)
                {
                    var prepickedValue = valuesToProvide.Dequeue();
                    if(prepickedValue < randomValueRequest.RangeMin || prepickedValue > randomValueRequest.RangeMax)
                    {
                        var message = string.Format("The value must be between {0} and {1}, inclusive. This number has been removed.",
                                                    randomValueRequest.RangeMin,
                                                    randomValueRequest.RangeMax);
                        //Inform the RNG Seeder tool that the value is not in range.
                        gameLogicAutomationService.SendErrorMessage(InterceptorError.UnknownMessageReceived, message);
                            
                    }
                    else if(updatedPrepickedValues.Count(x => x == prepickedValue) == randomValueRequest.MaxDuplicates + 1)
                    {
                        var message = string.Format("There are already {0} duplicates in the collection. This number has been removed.",
                                                    randomValueRequest.MaxDuplicates);
                        //Inform the RNG Seeder tool of the duplicates.
                        gameLogicAutomationService.SendErrorMessage(InterceptorError.UnknownMessageReceived, message);
                    }
                    else
                    {
                        //Add a value if it is in the correct range and not a duplicate.
                        updatedPrepickedValues.Add(prepickedValue);
                    }

                }
            }

            return new PrepickResult(randomValueRequest, updatedPrepickedValues);
        }

        /// <inheritdoc/>
        public ICollection<PrepickResult> GetPrepickedValues(IEnumerable<RandomValueRequest> requests)
        {
            return requests.Select(GetPrepickedValues).ToList();
        }

        /// <inheritdoc/>
        public void SetValues(IEnumerable<int> values)
        {
            valuesToProvide.Clear();
            foreach(var item in values)
            {
                valuesToProvide.Enqueue(item);
            }
        }

        #endregion
    }
}
