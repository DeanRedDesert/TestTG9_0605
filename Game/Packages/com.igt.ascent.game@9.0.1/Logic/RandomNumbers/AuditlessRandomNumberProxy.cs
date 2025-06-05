//-----------------------------------------------------------------------
// <copyright file = "AuditlessRandomNumberProxy.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.RandomNumbers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// An implementation of <see cref="IAuditlessRandomNumberProxy"/> that pulls numbers from a number sequence before pulling
    /// them from the random number source in the provided <see cref="IGameLib"/>, but does not provide auditing capabilities.
    /// </summary>
    internal class AuditlessRandomNumberProxy : IAuditlessRandomNumberProxy
    {
        /// <summary>The <see cref="IGameLib"/> to use to fill random number requests.</summary>
        private readonly IGameLib gameLib;

        /// <summary>Stores predetermined random numbers to be returned before calling the actual random number generator.</summary>
        private readonly NumberSequence numberSequence = new NumberSequence();

        /// <summary>
        /// Initializes the random number proxy with the given <see cref="IGameLib"/> instance.
        /// </summary>
        /// <param name="gameLib">The instance of <see cref="IGameLib"/> to pull random values from.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="gameLib"/> is null.</exception>
        public AuditlessRandomNumberProxy(IGameLib gameLib)
        {
            if(gameLib == null)
            {
                throw new ArgumentNullException("gameLib");
            }

            this.gameLib = gameLib;
        }

        #region IRandomNumbers implementation

        /// <inheritdoc/>
        public virtual ICollection<int> GetRandomNumbers(RandomValueRequest request)
        {
            var filledRequest = new List<int>();
            var sequencedNumbers = numberSequence.GetSequencedNumbers(request);
            var modifiedRequest = GetModifiedRequest(request, sequencedNumbers);

            filledRequest.AddRange(
                modifiedRequest == null ? sequencedNumbers : gameLib.GetRandomNumbers(modifiedRequest));

            return filledRequest;
        }

        /// <inheritdoc/>
        public virtual ICollection<int> GetRandomNumbers(ICollection<RandomValueRequest> requestList)
        {
            var filledRequest = new List<int>();
            var requestsToFill = new Stack<RandomValueRequest>(requestList.Reverse());
            var modifiedRequestFound = false;

            while(!modifiedRequestFound && requestsToFill.Count > 0)
            {
                var currentRequest = requestsToFill.Pop();
                var sequencedNumbers = numberSequence.GetSequencedNumbers(currentRequest);
                var modifiedRequest = GetModifiedRequest(currentRequest, sequencedNumbers);
                if(modifiedRequest == null)
                {
                    filledRequest.AddRange(sequencedNumbers);
                }
                else
                {
                    modifiedRequestFound = true;
                    requestsToFill.Push(modifiedRequest);
                }
            }

            if(requestsToFill.Count > 0)
            {
                var remainingValues = requestsToFill.Count == 1
                    ? gameLib.GetRandomNumbers(requestsToFill.Peek())
                    : gameLib.GetRandomNumbers(requestsToFill.ToArray());
                filledRequest.AddRange(remainingValues);
            }

            return filledRequest;
        }

        #endregion IRandomNumbers implementation

        #region INumberSequence implementation

        /// <inheritdoc/>
        public void AddNumberSequence(IEnumerable<int> sequence)
        {
            numberSequence.AddNumberSequence(sequence);
        }

        #endregion INumberSequence implementation

        /// <summary>
        /// Gets a <see cref="RandomValueRequest"/> that incorporates the given pre-picked values or <b>null</b> if the
        /// pre-picked values collection contains enough values to fill the original request.
        /// </summary>
        /// <param name="request">The request to add the pre-picked values to.</param>
        /// <param name="prepickedValues">The pre-picked values to add to the request.</param>
        /// <returns>
        /// Either a modified <see cref="RandomValueRequest"/> that contains the pre-picked values or <b>null</b> if the
        /// pre-picked values can actually fill the request.
        /// </returns>
        private static RandomValueRequest GetModifiedRequest(
            RandomValueRequest request,
            ICollection<int> prepickedValues)
        {
            if(prepickedValues.Count == request.Count)
            {
                return null;
            }

            if(prepickedValues.Count == 0)
            {
                return request;
            }

            return new RandomValueRequest(
                request.Count,
                request.RangeMin,
                request.RangeMax,
                request.MaxDuplicates,
                prepickedValues,
                request.GeneratorName);
        }
    }
}