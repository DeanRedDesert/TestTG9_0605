//-----------------------------------------------------------------------
// <copyright file = "RandomNumberProxy.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.RandomNumbers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// An implementation of <see cref="IRandomNumberProxy"/> that pulls numbers from a number sequence before pulling
    /// them from the random number source in the provided <see cref="IGameLib"/>.
    /// </summary>
    /// <remarks>
    /// This stores up to the most recent <see cref="MaxAuditNumbers"/> random numbers for audit. When more than <see cref="MaxAuditNumbers"/>
    /// numbers are generated before AuditNumbers() is called, older numbers will be overwritten by newer numbers.
    /// </remarks>
    internal class RandomNumberProxy : IRandomNumberProxy
    {
        /// <summary>
        /// The <see cref="IAuditlessRandomNumberProxy"/> to provide random number proxy functionality.
        /// </summary>
        private readonly IAuditlessRandomNumberProxy auditlessRandomNumberProxy;

        /// <summary>
        /// A <see cref="CircularBuffer{T}"/> for storing audit numbers.
        /// </summary>
        private readonly CircularBuffer<int> auditor;

        /// <summary>
        /// The maximum number of audit numbers to collect.
        /// </summary>
        public const int MaxAuditNumbers = 500;

        /// <summary>
        /// Initializes the random number proxy with the given <see cref="IGameLib"/> instance.
        /// </summary>
        /// <param name="gameLib">The instance of <see cref="IGameLib"/> to pull random values from.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="gameLib"/> is null.</exception>
        public RandomNumberProxy(IGameLib gameLib)
        {
            if(gameLib == null)
            {
                throw new ArgumentNullException("gameLib");
            }

            auditlessRandomNumberProxy = new AuditlessRandomNumberProxy(gameLib);
            auditor = new CircularBuffer<int>(MaxAuditNumbers);
        }

        /// <summary>
        /// Initializes the random number proxy with the given <see cref="IAuditlessRandomNumberProxy"/> instance.
        /// </summary>
        /// <param name="auditlessRandomNumberProxy">
        /// The <see cref="IAuditlessRandomNumberProxy"/> to use within this <see cref="RandomNumberProxy"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="auditlessRandomNumberProxy"/> is null.</exception>
        public RandomNumberProxy(IAuditlessRandomNumberProxy auditlessRandomNumberProxy)
        {
            if(auditlessRandomNumberProxy == null)
            {
                throw new ArgumentNullException("auditlessRandomNumberProxy");
            }

            this.auditlessRandomNumberProxy = auditlessRandomNumberProxy;
            auditor = new CircularBuffer<int>(MaxAuditNumbers);
        }

        #region IRandomNumbers implementation

        /// <inheritdoc/>
        public ICollection<int> GetRandomNumbers(RandomValueRequest request)
        {
            var filledRequest = auditlessRandomNumberProxy.GetRandomNumbers(request);
            auditor.AddEntries(filledRequest);
            return filledRequest;
        }

        /// <inheritdoc/>
        public ICollection<int> GetRandomNumbers(ICollection<RandomValueRequest> requestList)
        {
            var filledRequest = auditlessRandomNumberProxy.GetRandomNumbers(requestList);
            auditor.AddEntries(filledRequest);
            return filledRequest;
        }

        #endregion IRandomNumbers implementation

        #region INumberSequence implementation

        /// <inheritdoc/>
        public void AddNumberSequence(IEnumerable<int> sequence)
        {
            auditlessRandomNumberProxy.AddNumberSequence(sequence);
        }

        #endregion INumberSequence implementation

        #region IAuditNumbers implementation

        /// <inheritdoc/>
        /// <remarks>
        /// AuditNumbers stores up to the last <see cref="MaxAuditNumbers"/> numbers drawn.  If more than
        /// <see cref="MaxAuditNumbers"/> numbers have been drawn since AuditNumbers was last called, only
        /// the most recent <see cref="MaxAuditNumbers"/> numbers will be returned.
        /// </remarks>
        public ReadOnlyCollection<int> AuditNumbers()
        {
            var auditNumbers = new ReadOnlyCollection<int>(auditor.GetEntries());
            auditor.Clear();
            return auditNumbers;
        }

        #endregion IAuditNumbers implementation
    }
}