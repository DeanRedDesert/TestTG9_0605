// -----------------------------------------------------------------------
// <copyright file = "GenericListCopyProcessor.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Cloneable
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// This class is used to deep copy the data of generic list type <see cref="List{T}"/>.
    /// </summary>
    public sealed class GenericListCopyProcessor : IDataCopyProcessor
    {
        #region Fields

        /// <summary>
        /// The copy processor used to copy the element in the list.
        /// </summary>
        private readonly IDataCopyProcessor copyProcessor;

        /// <summary>
        /// The cached generic copy methods indexed by the real list type.
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
        static GenericListCopyProcessor()
        {
            ReflectGenericMethodDefinition();
        }

        /// <summary>
        /// Construct the instance with the given element copy processor.
        /// </summary>
        /// <param name="copyProcessor">The copy processor is used to copy the element in the list.</param>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="copyProcessor"/> is null.</exception>
        public GenericListCopyProcessor(IDataCopyProcessor copyProcessor)
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
                if(genericDefinition == typeof(List<>))
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

        #region Private Methods

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
                genericCloneMethodDefinition
                    = typeof(GenericListCopyProcessor).GetMethod("DeepCopyList",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                if(genericCloneMethodDefinition == null)
                {
                    throw new InvalidOperationException("Cannot find method GenericListCopyProcessor.DeepCopyList");
                }
            }
        }

        /// <summary>
        /// Deep copy a list.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <param name="source">The source list to copy.</param>
        /// <returns>The copied list.</returns>
        /// <remarks>
        /// Fixing PFR-898.
        /// Using instance method instead of static method to avoid mono crash when unloading the runtime.
        /// </remarks>
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        private List<T> DeepCopyList<T>(IList<T> source)
        {
            var list = new List<T>(source.Count);
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var element in source)
            {
                list.Add((T)copyProcessor.DeepCopy(element));
            }
            return list;
        }
        
        #endregion
    }
}