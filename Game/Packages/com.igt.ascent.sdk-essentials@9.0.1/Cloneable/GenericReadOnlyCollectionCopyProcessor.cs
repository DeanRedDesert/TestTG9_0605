// -----------------------------------------------------------------------
// <copyright file = "GenericReadOnlyCollectionCopyProcessor.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Cloneable
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    /// <summary>
    /// This class is used to deep copy the data of generic read only collection types.
    /// <see cref="ReadOnlyCollection{T}"/>.
    /// </summary>
    public sealed class GenericReadOnlyCollectionCopyProcessor : IDataCopyProcessor
    {
        #region Fields

        /// <summary>
        /// The copy processor used to copy the element in the collection.
        /// </summary>
        private readonly IDataCopyProcessor copyProcessor;

        /// <summary>
        /// The cached generic copy methods indexed by the real collection type.
        /// </summary>
        private readonly Dictionary<Type, MethodInfo> copyMethodImpl = new Dictionary<Type, MethodInfo>();

        /// <summary>
        /// The cached copy method generic definition.
        /// </summary>
        private static MethodInfo genericCloneMethodDefinition;

        #endregion

        #region Constructors

        /// <summary>
        /// The static constructor.
        /// </summary>
        static GenericReadOnlyCollectionCopyProcessor()
        {
            ReflectGenericMethodDefinition();
        }

        /// <summary>
        /// Construct the instance with the given element copy processor.
        /// </summary>
        /// <param name="copyProcessor">The copy processor is used to copy the element in the list.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="copyProcessor"/> is null.</exception>
        public GenericReadOnlyCollectionCopyProcessor(IDataCopyProcessor copyProcessor)
        {
            this.copyProcessor = copyProcessor ?? throw new ArgumentNullException(nameof(copyProcessor));
        }

        #endregion

        #region Implementation of IDeepCopyProcessor

        /// <inheritdoc />
        public object DeepCopy(object source)
        {
            if(source == null)
            {
                return null;
            }

            var implType = source.GetType();
            if(implType.IsGenericType)
            {
                var genericDefinition = implType.GetGenericTypeDefinition();
                if(genericDefinition == typeof(ReadOnlyCollection<>))
                {
                    if(!copyMethodImpl.ContainsKey(implType))
                    {
                        copyMethodImpl[implType] =
                            genericCloneMethodDefinition.MakeGenericMethod(implType.GetGenericArguments());
                    }
                    return copyMethodImpl[implType].Invoke(this, new[] {source});
                }
            }
            throw new ArgumentException("The source type is not supported.", nameof(source));
        }

        #endregion

        /// <summary>
        /// Reflect the generic copy method definition.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the generic copy method cannot be found.
        /// </exception>
        private static void ReflectGenericMethodDefinition()
        {
            if(genericCloneMethodDefinition == null)
            {
                genericCloneMethodDefinition =
                    typeof(GenericReadOnlyCollectionCopyProcessor)
                        .GetMethod("DeepCopyReadOnlyCollection", BindingFlags.Instance | BindingFlags.NonPublic);
                if(genericCloneMethodDefinition == null)
                {
                    throw new InvalidOperationException(
                        "Cannot find method GenericReadOnlyCollectionCopyProcessor.DeepCopyReadOnlyCollection");
                }
            }
        }

        /// <summary>
        /// Deep copy the collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection.</typeparam>
        /// <param name="source">The source collection to copy.</param>
        /// <returns>The copied collection.</returns>
        /// <remarks>
        /// Fixing PFR-898.
        /// Using instance method instead of static method to avoid mono crash when unloading the runtime.
        /// </remarks>
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        private ReadOnlyCollection<T> DeepCopyReadOnlyCollection<T>(ReadOnlyCollection<T> source)
        {
            var list = new List<T>(source.Count);
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var element in source)
            {
                list.Add((T)copyProcessor.DeepCopy(element));
            }
            return new ReadOnlyCollection<T>(list);
        }
    }
}