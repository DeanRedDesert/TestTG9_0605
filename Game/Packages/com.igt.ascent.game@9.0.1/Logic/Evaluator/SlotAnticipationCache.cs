//-----------------------------------------------------------------------
// <copyright file = "SlotAnticipationCache.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("IGT.Game.Core.Logic.Evaluator.Tests")]
namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Schemas;

    /// <summary>
    /// Caches the created paytable sections by the SlotAnticipationEvaluator for faster
    /// evaluations with the same data.
    /// </summary>
    public class SlotAnticipationCache : IEquatable<SlotAnticipationCache>
    {
        private readonly Dictionary<SlotAnticipationCacheKey, SlotPaytableSection> dataCache;

        /// <summary>
        /// Constructs a new slot anticipation cache instance.
        /// </summary>
        public SlotAnticipationCache()
        {
            dataCache = new Dictionary<SlotAnticipationCacheKey, SlotPaytableSection>();
        }

        /// <summary>
        /// Gets the number of items cached.
        /// </summary>
        public int Count
        {
            get
            {
                return dataCache.Count;
            }
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
        {
            dataCache.Clear();
        }

        /// <summary>
        /// Gets a cached paytable section.
        /// </summary>
        /// <param name="originalPaytableSection">The original game paytable.</param>
        /// <param name="anticipationPrizes">The list of prizes to anticipate.</param>
        /// <returns>The cached paytable if it exists otherwise null.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="originalPaytableSection"/> is null.
        /// Thrown if <paramref name="anticipationPrizes"/> is null.
        /// </exception>
        internal SlotPaytableSection GetCachedPaytableSection(SlotPaytableSection originalPaytableSection,
            List<SlotAnticipationPrize> anticipationPrizes)
        {
            if(originalPaytableSection == null)
            {
                throw new ArgumentNullException("originalPaytableSection");
            }

            if(anticipationPrizes == null)
            {
                throw new ArgumentNullException("anticipationPrizes");
            }

            var cacheKey = new SlotAnticipationCacheKey(originalPaytableSection, anticipationPrizes);
            SlotPaytableSection paytableSection;
            dataCache.TryGetValue(cacheKey, out paytableSection);

            return paytableSection;
        }

        /// <summary>
        /// Saves a new generated paytable section to the cache.
        /// </summary>
        /// <param name="originalPaytableSection">The original game paytable.</param>
        /// <param name="anticipationPrizes">The list of prizes to anticipate.</param>
        /// <param name="anticipationPaytableSection">The generated paytable section.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="originalPaytableSection"/> is null.
        /// Thrown if <paramref name="anticipationPrizes"/> is null.
        /// Thrown if <paramref name="anticipationPaytableSection"/> is null.
        /// </exception>
        internal void SaveToCache(SlotPaytableSection originalPaytableSection,
            List<SlotAnticipationPrize> anticipationPrizes,
            SlotPaytableSection anticipationPaytableSection)
        {
            if(originalPaytableSection == null)
            {
                throw new ArgumentNullException("originalPaytableSection");
            }

            if(anticipationPrizes == null)
            {
                throw new ArgumentNullException("anticipationPrizes");
            }

            if(anticipationPaytableSection == null)
            {
                throw new ArgumentNullException("anticipationPaytableSection");
            }

            var cacheKey = new SlotAnticipationCacheKey(originalPaytableSection, anticipationPrizes);

            if(dataCache.ContainsKey(cacheKey))
            {
                dataCache[cacheKey] = anticipationPaytableSection;
            }
            else
            {
                dataCache.Add(cacheKey, anticipationPaytableSection);
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var cacheObj = obj as SlotAnticipationCache;
            if(cacheObj != null)
            {
                return Equals(cacheObj);
            }

            return false;
        }

        /// <summary>
        /// Determines if two instances are equal.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if both instances are equal.</returns>
        public bool Equals(SlotAnticipationCache other)
        {
            return other != null &&
                (object.ReferenceEquals(this, other) || dataCache.SequenceEqual(other.dataCache));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return dataCache.Aggregate(0, (hashCode, item) => hashCode ^ item.GetHashCode());
        }

        /// <summary>
        /// Provides a unique key based on the <see cref="SlotPaytableSection"/> and <see cref="IEnumerable{T}"/> of
        /// <see cref="SlotAnticipationPrize"/> objects contained within it.  The order of the prize objects does not
        /// matter since they are sorted when the key is created.
        /// </summary>
        private struct SlotAnticipationCacheKey : IEquatable<SlotAnticipationCacheKey>
        {
            private readonly SlotPaytableSection slotPaytableSection;
            private readonly List<SlotAnticipationPrize> anticipationPrizes;

            /// <summary>
            /// Initializes a new instance of the <see cref="SlotAnticipationCacheKey"/> struct.
            /// </summary>
            /// <param name="paytableSection">A slot paytable section.</param>
            /// <param name="prizes">A list of <see cref="SlotAnticipationPrize"/> objects.</param>
            /// <remarks>The list of prizes is sorted when the key is created.</remarks>
            public SlotAnticipationCacheKey(SlotPaytableSection paytableSection, IEnumerable<SlotAnticipationPrize> prizes)
            {
                slotPaytableSection = paytableSection;
                anticipationPrizes = new List<SlotAnticipationPrize>(prizes);
                anticipationPrizes.Sort();
            }

            /// <summary>
            /// Gets a hash code based on the contents of this struct.
            /// </summary>
            /// <returns>A hash code that is suitable for use with a dictionary.</returns>
            public override int GetHashCode()
            {
                var result = slotPaytableSection.name != null ? slotPaytableSection.name.GetHashCode() : 0;
                return result ^ anticipationPrizes.Aggregate(0, (hashCode, prize) => hashCode ^ prize.GetHashCode());
            }

            /// <summary>
            /// Determines if the given object is equal to this <see cref="SlotAnticipationCacheKey"/> instance.
            /// </summary>
            /// <param name="obj">The object to compare against.</param>
            /// <returns><b>true</b> if they are considered equal, <b>false</b> otherwise.</returns>
            public override bool Equals(object obj)
            {
                if(ReferenceEquals(null, obj))
                    return false;
                if(obj.GetType() != typeof(SlotAnticipationCacheKey))
                    return false;
                return Equals((SlotAnticipationCacheKey)obj);
            }

            #region Implementation of IEquatable<SlotAnticipationCacheKey>

            /// <summary>
            /// Determines if the given <see cref="SlotAnticipationCacheKey"/> is equal to this one.
            /// </summary>
            /// <param name="other">The key to compare.</param>
            /// <returns><b>true</b> if they are equal, <b>false</b> otherwise.</returns>
            public bool Equals(SlotAnticipationCacheKey other)
            {
                return Equals(other.slotPaytableSection.name, slotPaytableSection.name) && anticipationPrizes.SequenceEqual(other.anticipationPrizes);
            }

            #endregion
        }
    }
}
