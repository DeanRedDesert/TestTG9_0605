//-----------------------------------------------------------------------
// <copyright file = "CompactSerializer.ReadOnlyCollection.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.CompactSerialization
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Provides functions for reading and writing read only collections.
    /// </summary>
    public partial class CompactSerializer
    {
        /// <summary>
        /// Write a read only collection of type <typeparamref name="TItem"/> to the stream.
        /// </summary>
        /// <typeparam name="TItem">The type of the collection item.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="collection">The collection to write.</param>
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static void WriteReadOnlyCollection<TItem>(Stream stream, ReadOnlyCollection<TItem> collection)
        {
            // Since the ReadOnlyCollection inherits from IList<> which the compact serializer already
            // supports, it is best to use that functionality.
            var list = collection as IList<TItem>;
            WriteList(stream, list);
        }

        /// <summary>
        /// Reads a read only collection of type <typeparamref name="TItem"/> from the stream.
        /// </summary>
        /// <typeparam name="TItem">The type of the collection item.</typeparam>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The read only collection of <typeparamref name="TItem"/> items.</returns>
        public static ReadOnlyCollection<TItem> ReadReadOnlyCollection<TItem>(Stream stream)
        {
            // Since the ReadOnlyCollection inherits from IList<> which the compact serializer already
            // supports, it is best to use that functionality.
            var list = Deserialize(stream, typeof(IList<TItem>)) as IList<TItem>;            
            
            if(list == null)
            {
                // The list will be null if a null collection was serialized. The read only
                // collection constructor will not allow null to be passed to it.
                return null;
            }

            return new ReadOnlyCollection<TItem>(list);
        }

        #region Invoking Generic ReadOnly Collection Read

        /// <summary>
        /// The name string of method <see cref="ReadReadOnlyCollection{TItem}"/>.
        /// </summary>
        private const string MethodReadReadOnlyCollection = "ReadReadOnlyCollection";

        /// <summary>
        /// The cached reflection of the generic collection read method <see cref="ReadReadOnlyCollection{TItem}"/>.
        /// </summary>
        /// <remarks>
        /// This static field is local to a thread, thus thread safe.
        /// </remarks>
        [ThreadStatic]
        private static MethodInfo methodReadReadOnlyCollectionReflection;

        /// <summary>
        /// The dynamically emitted, specifically typed methods from the generic method <see cref="ReadReadOnlyCollection{TItem}"/>.
        /// </summary>
        /// <remarks>
        /// This static field is local to a thread, thus thread safe.
        /// </remarks>
        [ThreadStatic]
        private static Dictionary<Type, MethodInfo> methodReadReadOnlyCollectionImpls;

        /// <summary>
        /// Invoke the method <see cref="ReadReadOnlyCollection{TItem}"/> with the given type parameter.
        /// </summary>
        /// <param name="elementType">The type of the collection item.</param>
        /// <param name="arguments">The arguments indicates the stream to read from.</param>
        /// <returns>The read only collection of <paramref name="elementType"/> items.</returns>
        private static object InvokeReadReadOnlyCollection(Type elementType, params object[] arguments)
        {
            if(methodReadReadOnlyCollectionImpls == null)
            {
                methodReadReadOnlyCollectionImpls = new Dictionary<Type, MethodInfo>();
            }

            if(!methodReadReadOnlyCollectionImpls.ContainsKey(elementType))
            {
                if(methodReadReadOnlyCollectionReflection == null)
                {
                    methodReadReadOnlyCollectionReflection = typeof(CompactSerializer).GetMethod(MethodReadReadOnlyCollection);
                }

                methodReadReadOnlyCollectionImpls[elementType] =
                    methodReadReadOnlyCollectionReflection?.MakeGenericMethod(elementType);
            }

            return methodReadReadOnlyCollectionImpls[elementType].Invoke(null, arguments);
        }

        #endregion
    }
}
