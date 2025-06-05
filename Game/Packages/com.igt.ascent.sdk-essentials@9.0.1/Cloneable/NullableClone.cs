// -----------------------------------------------------------------------
// <copyright file = "NullableClone.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Cloneable
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// This class provides with methods that clone the most used types whose value could be null.
    /// </summary>
    public static class NullableClone
    {
        /// <summary>
        /// Deep clone the object.
        /// </summary>
        /// <typeparam name="T">The type of the object to clone.</typeparam>
        /// <param name="source">The object to clone.</param>
        /// <returns>The cloned object; returns null if the source is null.</returns>
        public static T DeepClone<T>(T source) where T : IDeepCloneable
        {
            if(source == null)
            {
                return default;
            }

            return (T)source.DeepClone();
        }

        /// <summary>
        /// Deep clone the list. The elements are deep cloned.
        /// </summary>
        /// <typeparam name="T">The type of the element in the list.</typeparam>
        /// <param name="source">The list to clone.</param>
        /// <returns>The cloned list; returns null if the source is null.</returns>
        public static List<T> DeepCloneList<T>(List<T> source) where T : IDeepCloneable
        {
            if(source == null)
            {
                return null;
            }

            var copy = new List<T>(source.Count);
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var element in source)
            {
                copy.Add(DeepClone(element));
            }
            return copy;
        }

        /// <summary>
        /// Shallow clone the list. If the element is reference type, the cloned list has the identical references
        /// as the source list.
        /// </summary>
        /// <typeparam name="T">The type of the element in the list.</typeparam>
        /// <param name="source">The list to clone.</param>
        /// <returns>The cloned list; returns null if the source is null.</returns>
        public static List<T> ShallowCloneList<T>(List<T> source)
        {
            return source == null ? null : new List<T>(source);
        }

        /// <summary>
        /// Shallow clone the dictionary.
        /// If the key is reference type, the cloned dictionary has the identical key references
        /// as the source dictionary;
        /// If the value is reference type, the cloned dictionary has the identical value references
        /// as the source dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The dictionary to clone.</param>
        /// <returns>The cloned dictionary; returns null if the source is null.</returns>
        public static Dictionary<TKey, TValue> ShallowCloneDictionary<TKey, TValue>(Dictionary<TKey, TValue> source)
        {
            return source == null ? null : new Dictionary<TKey, TValue>(source);
        }

        /// <summary>
        /// Clone the dictionary with deep cloning the values.
        /// If the key is reference type, the cloned dictionary has the identical key references
        /// as the source dictionary;
        /// The values are deep cloned.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The dictionary to clone.</param>
        /// <returns>The cloned dictionary; returns null if the source is null.</returns>
        public static Dictionary<TKey, TValue> CloneDictionaryDeepCloneValue<TKey, TValue>(Dictionary<TKey, TValue> source)
            where TValue : IDeepCloneable
        {
            if(source == null)
            {
                return null;
            }

            var copy = new Dictionary<TKey, TValue>(source.Count);
            foreach(var pair in source)
            {
                copy.Add(pair.Key, DeepClone(pair.Value));
            }
            return copy;
        }

        /// <summary>
        /// Clone the dictionary with deep cloning the keys.
        /// The keys are deep cloned;
        /// If the value is reference type, the cloned dictionary has the identical value references
        /// as the source dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The dictionary to clone.</param>
        /// <returns>The cloned dictionary; returns null if the source is null.</returns>
        public static Dictionary<TKey, TValue> CloneDictionaryDeepCloneKey<TKey, TValue>(Dictionary<TKey, TValue> source)
            where TKey : IDeepCloneable
        {
            if(source == null)
            {
                return null;
            }

            var copy = new Dictionary<TKey, TValue>(source.Count);
            foreach(var pair in source)
            {
                copy.Add(DeepClone(pair.Key), pair.Value);
            }
            return copy;
        }

        /// <summary>
        /// Deep clone the dictionary. The keys, as well as the values, are deep cloned.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The dictionary to clone.</param>
        /// <returns>The cloned dictionary; returns null if the source is null.</returns>
        public static Dictionary<TKey, TValue> DeepCloneDictionary<TKey, TValue>(Dictionary<TKey, TValue> source)
            where TKey : IDeepCloneable
            where TValue : IDeepCloneable
        {
            if(source == null)
            {
                return null;
            }

            var copy = new Dictionary<TKey, TValue>(source.Count);
            foreach(var pair in source)
            {
                copy.Add(DeepClone(pair.Key), DeepClone(pair.Value));
            }
            return copy;
        }

        /// <summary>
        /// Deep clone the readonly collection. The elements are deep cloned.
        /// </summary>
        /// <typeparam name="T">The type of the element in the readonly collection.</typeparam>
        /// <param name="source">The readonly collection to clone.</param>
        /// <returns>The cloned collection; returns null if the source is null.</returns>
        public static ReadOnlyCollection<T> DeepCloneReadOnlyCollection<T>(ReadOnlyCollection<T> source)
            where T : IDeepCloneable
        {
            if(source == null)
            {
                return null;
            }

            var copy = new List<T>(source.Count);
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var element in source)
            {
                copy.Add(DeepClone(element));
            }
            return new ReadOnlyCollection<T>(copy);
        }
    }
}