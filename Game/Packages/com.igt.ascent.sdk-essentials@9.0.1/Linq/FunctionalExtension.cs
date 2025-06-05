//-----------------------------------------------------------------------
// <copyright file = "FunctionalExtension.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Linq
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Functional extension methods.
    /// </summary>
    public static class FunctionalExtension
    {
        /// <summary>
        /// Combine two lists together with a selector that takes an element from each list and outputs a new
        /// element. {1, 2, 3}.Zip({2, 4, 6}, (a, b) => a + b) returns {3, 6, 9}.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
        /// <typeparam name="TFirst">Type of the first enumerable.</typeparam>
        /// <typeparam name="TSecond">Type of the second enumerable.</typeparam>
        /// <typeparam name="TResult">Type of the output enumerable.</typeparam>
        /// <param name="first">First enumerable.</param>
        /// <param name="second">Second enumerable.</param>
        /// <param name="selector">Function that combines elements from each enumerable into the output.</param>
        /// <returns>An enumerable of the return type with the zipped results.</returns>
        /// <remarks>
        /// If the two enumerations are different lengths, the resulting enumerable will only have as many elements
        /// as the shortest of the two. All remaining elements of the longer enumeration are left un-enumerated.
        /// </remarks>
        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> selector)
        {
            if(first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }
            if(second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }
            if(selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }
            var firstEnum = first.GetEnumerator();
            var secondEnum = second.GetEnumerator();
            while(firstEnum.MoveNext() && secondEnum.MoveNext())
            {
                yield return selector(firstEnum.Current, secondEnum.Current);
            }
            firstEnum.Dispose();
            secondEnum.Dispose();
        }

        /// <summary>
        /// Combine three lists together with a selector that takes an element from each list and outputs a new element.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
        /// <typeparam name="TFirst">Type of the first enumerable.</typeparam>
        /// <typeparam name="TSecond">Type of the second enumerable.</typeparam>
        /// <typeparam name="TThird">Type of the third enumerable.</typeparam>
        /// <typeparam name="TResult">Type of the output enumerable.</typeparam>
        /// <param name="first">First enumerable.</param>
        /// <param name="second">Second enumerable.</param>
        /// <param name="third">Third enumerable.</param>
        /// <param name="selector">Function that combines elements from each enumerable into the output.</param>
        /// <returns>An enumerable of the return type with the zipped results.</returns>
        public static IEnumerable<TResult> Zip<TFirst, TSecond, TThird, TResult>(this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second, IEnumerable<TThird> third, Func<TFirst, TSecond, TThird, TResult> selector)
        {
            if(first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }
            if(second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }
            if(third == null)
            {
                throw new ArgumentNullException(nameof(third));
            }
            if(selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }
            var firstEnum = first.GetEnumerator();
            var secondEnum = second.GetEnumerator();
            var thirdEnum = third.GetEnumerator();
            while(firstEnum.MoveNext() && secondEnum.MoveNext() && thirdEnum.MoveNext())
            {
                yield return selector(firstEnum.Current, secondEnum.Current, thirdEnum.Current);
            }
            firstEnum.Dispose();
            secondEnum.Dispose();
            thirdEnum.Dispose();
        }

        /// <summary>
        /// Combine four lists together with a selector that takes an element from each list and outputs a new element.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
        /// <typeparam name="TFirst">Type of the first enumerable.</typeparam>
        /// <typeparam name="TSecond">Type of the second enumerable.</typeparam>
        /// <typeparam name="TThird">Type of the third enumerable.</typeparam>
        /// <typeparam name="TFourth">Type of the fourth enumerable.</typeparam>
        /// <typeparam name="TResult">Type of the output enumerable.</typeparam>
        /// <param name="first">First enumerable.</param>
        /// <param name="second">Second enumerable.</param>
        /// <param name="third">Third enumerable.</param>
        /// <param name="fourth">Fourth enumerable.</param>
        /// <param name="selector">Function that combines elements from each enumerable into the output.</param>
        /// <returns>An enumerable of the return type with the zipped results.</returns>
        public static IEnumerable<TResult> Zip<TFirst, TSecond, TThird, TFourth, TResult>(this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second, IEnumerable<TThird> third, IEnumerable<TFourth> fourth,
            Func<TFirst, TSecond, TThird, TFourth, TResult> selector)
        {
            if(first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }
            if(second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }
            if(third == null)
            {
                throw new ArgumentNullException(nameof(third));
            }
            if(fourth == null)
            {
                throw new ArgumentNullException(nameof(fourth));
            }
            if(selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }
            var firstEnum = first.GetEnumerator();
            var secondEnum = second.GetEnumerator();
            var thirdEnum = third.GetEnumerator();
            var fourthEnum = fourth.GetEnumerator();
            while(firstEnum.MoveNext() && secondEnum.MoveNext() && thirdEnum.MoveNext() && fourthEnum.MoveNext())
            {
                yield return selector(firstEnum.Current, secondEnum.Current, thirdEnum.Current, fourthEnum.Current);
            }
            firstEnum.Dispose();
            secondEnum.Dispose();
            thirdEnum.Dispose();
            fourthEnum.Dispose();
        }

        /// <summary>Iterate across a collection forever.</summary>
        /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
        /// <typeparam name="TCollection">Type of the collection.</typeparam>
        /// <param name="collection">Collection to iterate over.</param>
        /// <param name="repeatForever">If false, only iterate once.</param>
        /// <returns>A single or infinite iteration across the collection.</returns>
        public static IEnumerable<TCollection> RepeatForever<TCollection>(this ICollection<TCollection> collection, bool repeatForever)
        {
            if(collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            do
            {
                foreach(var obj in collection)
                {
                    yield return obj;
                }
                // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            } while(repeatForever);
        }

        /// <summary>Iterate across a collection a set number of times.</summary>
        /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
        /// <typeparam name="TCollection">Type of the collection.</typeparam>
        /// <param name="collection">Collection to iterate over.</param>
        /// <param name="repeatCount">Number of times to repeat the iteration.</param>
        /// <returns>An iteration across the collection <paramref name="repeatCount"/> times.</returns>
        public static IEnumerable<TCollection> Repeat<TCollection>(this ICollection<TCollection> collection, int repeatCount)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            for(var iteration = 0; iteration < repeatCount; iteration++)
            {
                foreach (var obj in collection)
                {
                    yield return obj;
                }
            }
        }

        /// <summary>Convert an enumeration into an enumeration of {Current, Next} pairs.</summary>
        /// <exception cref="ArgumentNullException">Thrown if any argument is null.</exception>
        /// <typeparam name="TEnumeration">Type of the enumeration.</typeparam>
        /// <param name="enumeration">Enumeration to convert.</param>
        /// <returns>{A, B, C, D} => {{A, B}, {B, C}, {C, D}}.</returns>
        public static IEnumerable<Pair<TEnumeration, TEnumeration>> Pairs<TEnumeration>(
            this IEnumerable<TEnumeration> enumeration)
        {
            if(enumeration == null)
            {
                throw new ArgumentNullException(nameof(enumeration));
            }
            var enumerator = enumeration.GetEnumerator();
            if(enumerator.MoveNext())
            {
                var current = enumerator.Current;
                while(enumerator.MoveNext())
                {
                    var next = enumerator.Current;
                    yield return new Pair<TEnumeration, TEnumeration> {First = current, Second = next};
                    current = next;
                }
            }
            enumerator.Dispose();
        }
    }
}
