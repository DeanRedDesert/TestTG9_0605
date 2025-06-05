//-----------------------------------------------------------------------
// <copyright file = "WinCycleEntryList.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Game.Core.Logic.Evaluator.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Class which contains the win cycle entry list and total win amount for that list.
    /// </summary>
    [Serializable]
    public class WinCycleEntryList : ICompactSerializable, IEquatable<WinCycleEntryList>,
        IDeepCloneable
    {
        /// <summary>
        /// Creates an instance of this class.
        /// </summary>
        public WinCycleEntryList() : this(new List<WinCycleEntry>())
        {
        }

        /// <summary>
        /// The Constructor of WinCycleEntryList.
        /// </summary>
        /// <param name="list">A list of the win cycle entry.</param>
        public WinCycleEntryList(List<WinCycleEntry> list)
        {
            WinEntryList = list;
        }

        /// <summary>
        /// The list of win cycle entries.
        /// </summary>
        public List<WinCycleEntry> WinEntryList { get; set; }

        /// <summary>
        /// Total win amount of all the entries in the entry list.
        /// </summary>
        public long TotalWin
        {
            get
            {
                if(WinEntryList == null)
                {
                    return 0;
                }

                return WinEntryList.Sum(winCycleEntry => winCycleEntry.TotalWinAmount);
            }
        }

        #region ICompactSerializable

        /// <inheritdoc />
        public void Serialize(System.IO.Stream stream)
        {
            CompactSerializer.WriteList(stream, WinEntryList);
        }

        /// <inheritdoc />
        public void Deserialize(System.IO.Stream stream)
        {
            WinEntryList = CompactSerializer.ReadListSerializable<WinCycleEntry>(stream);
        }

        #endregion

        #region IDeepClonable

        /// <inheritdoc />
        public object DeepClone()
        {
            var entryListCopy = NullableClone.DeepCloneList(WinEntryList);
            var copy = new WinCycleEntryList(entryListCopy);
            return copy;
        }

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var equals = false;
            if(obj is WinCycleEntryList listObj)
            {
                equals = Equals(listObj);
            }

            return equals;
        }

        /// <inheritdoc />
        public bool Equals(WinCycleEntryList other)
        {
            return other != null &&
                (ReferenceEquals(this, other)
                || WinEntryList.SequenceEqual(other.WinEntryList));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return WinEntryList.GetHashCode();
        }
    }
}
