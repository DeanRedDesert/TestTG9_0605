//-----------------------------------------------------------------------
// <copyright file = "WinCycleEntry.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("IGT.Game.Core.Logic.Evaluator.Providers.Tests")]
namespace IGT.Game.Core.Logic.Evaluator.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Cloneable;
    using CompactSerialization;
    using Schemas;

    /// <summary>
    /// Win outcome item that contains the needed presentation information to be
    /// presented as part of a win cycle.
    /// </summary>
    [Serializable]
    public class WinCycleEntry : ICompactSerializable, IEquatable<WinCycleEntry>, IComparable<WinCycleEntry>,
        IDeepCloneable
    {
        /// <summary>
        /// Describes the type of win.
        /// </summary>
        public enum WinEntryType
        {
            /// <summary>
            /// Indicates that the type of this win is not known.
            /// </summary>
            Unknown,

            /// <summary>
            /// Indicates that the entry is for a line win.
            /// </summary>
            LineWin,

            /// <summary>
            /// Indicates that the entry is for a scatter win.
            /// </summary>
            Scatter,

            /// <summary>
            /// Indicates that the entry is for a multiway win.
            /// </summary>
            Multiway,

            /// <summary>
            /// Indicates that the entry is for a foundation adjusted win.
            /// </summary>
            Foundation
        }

        /// <summary>
        /// The type of this win entry.
        /// </summary>
        public WinEntryType EntryType { get; private set; }

        /// <summary>
        /// A list of indexes that are specific to the entry type. For instance if the entry type is line, then this list of indexes would reference a list of line numbers.
        /// </summary>
        public List<int> EntryTypeIndexes { get; private set; }

        /// <summary>
        /// Win amount associated with the win entry. For a grouped win entry, this is the win amount
        /// for each win in the group.
        /// </summary>
        /// <remarks>
        /// A grouped win entry contains multiple wins. The win amount is the amount won on a win.
        /// </remarks>
        public long WinAmount { get; private set; }

        /// <summary>
        /// Win amount associated with the win entry. For a grouped win entry, this is the total win amount
        /// for all the wins in the group.
        /// </summary>
        /// <remarks>
        /// A grouped win entry contains multiple wins. The total win amount is the total amount won
        /// on all of wins grouped. This is calculated by multiplying the win amount by the number of wins
        /// in the group. For a non-grouped win entry, the win amount is the same as the total win amount.
        /// </remarks>
        public long TotalWinAmount { get; private set; }

        /// <summary>
        /// Indicates if the win entry triggers a bonus
        /// </summary>
        public bool BonusTriggered { get; set; }

        /// <summary>
        /// Names of the bonuses that contributed to this win.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public List<string> BonusNames { get; protected internal set; }

        /// <summary>
        /// Symbols locations of the win.
        /// </summary>
        public List<Cell> Symbols { get; private set; }

        /// <summary>
        /// Reason associated with this award.
        /// </summary>
        /// <remarks>This field would generally be used to indicate the reason for a Foundation added win.</remarks>
        public string DisplayReason { get; private set; }

        /// <summary>
        /// The win outcome item name associated with this win entry.
        /// </summary>
        /// <remarks>
        /// This doesn't apply to grouped win entries since multiple win outcome items
        /// with different names can be grouped together.
        /// </remarks>
        public string WinOutcomeItemName { get; protected internal set; }

        /// <summary>
        /// Gets the list of progressive level strings (if any) associated with this win.
        /// </summary>
        public List<string> ProgressiveLevelStrings { get; private set; }

        /// <summary>
        /// Gets the list of progressive prize strings (if any) associated with this win.
        /// </summary>
        /// <remarks>
        /// Not all progressive levels have prize string defined.  Therefore, the index of an item in the list
        /// does NOT imply the progressive level with which the prize string is associated.
        /// </remarks>
        public List<string> ProgressivePrizeStrings { get; private set; }

        /// <summary>
        /// Gets if this represents a win from a progressive.
        /// </summary>
        public bool IsProgressivePrize => ProgressiveLevelStrings.Any();

        /// <summary>
        /// Create an instance of the win cycle entry with uninitialized data.
        /// </summary>
        public WinCycleEntry()
        {
            // Make sure all collections are not null.
            // Note that for purpose of efficiency, this constructor is not called
            // as a base for other constructors.
            EntryTypeIndexes = new List<int>();
            ProgressiveLevelStrings = new List<string>();
            ProgressivePrizeStrings = new List<string>();
            Symbols = new List<Cell>();
            BonusNames = new List<string>();
        }

        /// <summary>
        /// Create an instance of the win cycle entry with basic information.
        /// </summary>
        /// <param name="entryType">The type of the entry.</param>
        /// <param name="entryTypeIndexes">The type specific indexes of the entry.</param>
        /// <param name="winAmount">Win amount associated with the entry.</param>
        public WinCycleEntry(WinEntryType entryType, IList<int> entryTypeIndexes, long winAmount)
            : this(entryType, entryTypeIndexes, winAmount, null, null, null)
        {
        }

        /// <summary>
        /// Create an instance of the win cycle entry with additional display reason.
        /// </summary>
        /// <param name="entryType">
        /// The type of the entry.
        /// </param>
        /// <param name="entryTypeIndexes">
        /// The type specific indexes of the entry.
        /// </param>
        /// <param name="winAmount">
        /// Win amount associated with the entry.
        /// </param>
        /// <param name="displayReason">
        /// String indicating the purpose of this win. If present it should be displayed.
        /// </param>
        public WinCycleEntry(WinEntryType entryType, IList<int> entryTypeIndexes, long winAmount, string displayReason)
            : this(entryType, entryTypeIndexes, winAmount, displayReason, null, null)
        {
        }

        /// <summary>
        /// Create an instance of the win cycle entry with additional progressive information.
        /// </summary>
        /// <param name="entryType">
        /// The type of the entry.
        /// </param>
        /// <param name="entryTypeIndexes">
        /// The type specific indexes of the entry.
        /// </param>
        /// <param name="winAmount">
        /// Win amount associated with the entry.
        /// </param>
        /// <param name="displayReason">
        /// String indicating the purpose of this win. If present it should be displayed.
        /// </param>
        /// <param name="progressiveLevelStrings">
        /// The list of strings describing the progressive levels awarded.
        /// </param>
        /// <param name="progressivePrizeStrings">
        /// The list of valid progressive prize strings.  Not all progressive levels have prize string defined.
        /// </param>
        public WinCycleEntry(WinEntryType entryType, IList<int> entryTypeIndexes, long winAmount, string displayReason,
                             IEnumerable<string> progressiveLevelStrings, IEnumerable<string> progressivePrizeStrings)
        {
            EntryType = entryType;
            EntryTypeIndexes = entryTypeIndexes != null
                                   ? entryTypeIndexes.ToList()
                                   : new List<int>();
            WinAmount = winAmount;
            TotalWinAmount = WinAmount * EntryTypeIndexes.Count;
            DisplayReason = displayReason;

            ProgressiveLevelStrings = progressiveLevelStrings != null
                                          ? progressiveLevelStrings.ToList()
                                          : new List<string>();

            ProgressivePrizeStrings = progressivePrizeStrings != null
                                          ? progressivePrizeStrings.ToList()
                                          : new List<string>();

            Symbols = new List<Cell>();

            BonusNames = new List<string>();
        }

        /// <inheritdoc />
        public override bool Equals(object compareObject)
        {
            return compareObject is WinCycleEntry winCycleEntryObj && Equals(winCycleEntryObj);
        }

        /// <summary>
        /// Equals override that is used instead of the object version when a WinCycleEntry object
        /// is compared using the WinCycleEntry type.
        /// </summary>
        /// <param name="compareWinCycleEntry">The WinCycleEntry Object to compare to.</param>
        /// <returns>Returns true if the objects are equal otherwise false.</returns>
        public bool Equals(WinCycleEntry compareWinCycleEntry)
        {
            if(compareWinCycleEntry == null)
            {
                return false;
            }

            if(ReferenceEquals(this, compareWinCycleEntry))
            {
                return true;
            }

            if(!BonusNames.SequenceEqual(compareWinCycleEntry.BonusNames))
            {
                return false;
            }

            return EntryType == compareWinCycleEntry.EntryType &&
                EntryTypeIndexes.SequenceEqual(compareWinCycleEntry.EntryTypeIndexes) &&
                WinAmount == compareWinCycleEntry.WinAmount &&
                TotalWinAmount == compareWinCycleEntry.TotalWinAmount &&
                DisplayReason == compareWinCycleEntry.DisplayReason &&
                AreSymbolsEqual(Symbols, compareWinCycleEntry.Symbols) &&
                WinOutcomeItemName == compareWinCycleEntry.WinOutcomeItemName &&
                ProgressiveLevelStrings.SequenceEqual(compareWinCycleEntry.ProgressiveLevelStrings) &&
                ProgressivePrizeStrings.SequenceEqual(compareWinCycleEntry.ProgressivePrizeStrings) &&
                BonusTriggered == compareWinCycleEntry.BonusTriggered;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashIdentifier = 23;
            hashIdentifier = hashIdentifier * 37 + EntryType.GetHashCode();
            hashIdentifier = EntryTypeIndexes.Aggregate(hashIdentifier, (current, index) => current * 37 + index.GetHashCode());
            hashIdentifier = hashIdentifier * 37 + WinAmount.GetHashCode();
            hashIdentifier = hashIdentifier * 14 + TotalWinAmount.GetHashCode();

            return hashIdentifier;
        }

        /// <inheritDoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, EntryType);
            CompactSerializer.WriteList(stream, EntryTypeIndexes);
            CompactSerializer.Write(stream, WinAmount);
            CompactSerializer.Write(stream, TotalWinAmount);
            CompactSerializer.WriteList(stream, Symbols);
            CompactSerializer.Write(stream, DisplayReason);
            CompactSerializer.Write(stream, WinOutcomeItemName);
            CompactSerializer.WriteList(stream, ProgressiveLevelStrings);
            CompactSerializer.WriteList(stream, ProgressivePrizeStrings);
            CompactSerializer.Write(stream, BonusTriggered);
            CompactSerializer.WriteList(stream, BonusNames);
        }

        /// <inheritDoc/>
        public void Deserialize(Stream stream)
        {
            EntryType = CompactSerializer.ReadEnum<WinEntryType>(stream);
            EntryTypeIndexes = CompactSerializer.ReadListInt(stream);
            WinAmount = CompactSerializer.ReadLong(stream);
            TotalWinAmount = CompactSerializer.ReadLong(stream);
            Symbols = CompactSerializer.ReadListSerializable<Cell>(stream);
            DisplayReason = CompactSerializer.ReadString(stream);
            WinOutcomeItemName = CompactSerializer.ReadString(stream);
            ProgressiveLevelStrings = CompactSerializer.ReadListString(stream);
            ProgressivePrizeStrings = CompactSerializer.ReadListString(stream);
            BonusTriggered = CompactSerializer.ReadBool(stream);
            BonusNames = CompactSerializer.ReadListString(stream);
        }

        #region IDeepClonable

        /// <inheritDoc/>
        public object DeepClone()
        {
            var copy = new WinCycleEntry
                       {
                           EntryType = EntryType,
                           EntryTypeIndexes = NullableClone.ShallowCloneList(EntryTypeIndexes),
                           WinAmount = WinAmount,
                           TotalWinAmount = TotalWinAmount,
                           Symbols = NullableClone.DeepCloneList(Symbols),
                           DisplayReason = DisplayReason,
                           WinOutcomeItemName = WinOutcomeItemName,
                           ProgressiveLevelStrings = NullableClone.ShallowCloneList(ProgressiveLevelStrings),
                           ProgressivePrizeStrings = NullableClone.ShallowCloneList(ProgressivePrizeStrings),
                           BonusTriggered = BonusTriggered,
                           BonusNames = NullableClone.ShallowCloneList(BonusNames)
                       };
            return copy;
        }

        #endregion

        /// <summary>
        /// Write the content to a string.
        /// </summary>
        /// <returns>A string representation.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(" EntryType: {0}\n", EntryType);
            sb.AppendLine(" EntryTypeIndexes:");
            sb.AppendLine(string.Join(", ", EntryTypeIndexes.Select(index => index.ToString(CultureInfo.InvariantCulture)).ToArray()));
            sb.AppendFormat(" WinAmount: {0}\n", WinAmount);
            sb.AppendFormat(" TotalWinAmount: {0}\n", TotalWinAmount);
            sb.AppendLine(" Symbols:");
            sb.AppendLine(string.Join("\n", Symbols.Select(symbol => symbol.ToString()).ToArray()));
            sb.AppendFormat(" DisplayReason: {0}\n", DisplayReason);
            sb.AppendLine(" Name " + WinOutcomeItemName);
            sb.AppendLine(" ProgressiveLevelStrings:");
            sb.AppendLine(string.Join(", ", ProgressiveLevelStrings.ToArray()));
            sb.AppendLine(" ProgressivePrizeStrings ");
            sb.AppendLine(string.Join(", ", ProgressivePrizeStrings.ToArray()));
            sb.AppendFormat(" Is Progressive Prize: {0}\n", IsProgressivePrize);
            sb.AppendLine(" BonusNameStrings ");
            sb.AppendLine(string.Join(", ", BonusNames.ToArray()));
            return sb.ToString();
        }

        #region IComparable Implementation

        /// <inheritDoc/>
        public virtual int CompareTo(WinCycleEntry other)
        {
            // First sort by win entry type. Scatter win entry type is displayed first over other win entry types.
            if(EntryType == WinEntryType.Scatter && other.EntryType != WinEntryType.Scatter)
            {
                return -1;
            }
            if(EntryType != WinEntryType.Scatter && other.EntryType == WinEntryType.Scatter)
            {
                return 1;
            }

            // Bonus wins (Scatter with bonus names provided) are displayed before Scatter wins without bonus win.
            if(BonusNames.Count > 0 && other.BonusNames.Count == 0)
            {
                return -1;
            }
            if(BonusNames.Count == 0 && other.BonusNames.Count > 0)
            {
                return 1;
            }

            // Wins with progressive prize strings go before wins without them,
            // including wins with progressive levels but no prize strings.
            if(ProgressivePrizeStrings.Count > 0 && other.ProgressivePrizeStrings.Count == 0)
            {
                return -1;
            }
            if(ProgressivePrizeStrings.Count == 0 && other.ProgressivePrizeStrings.Count > 0)
            {
                return 1;
            }

            // Wins with progressive level strings go before wins without them. 
            if(ProgressiveLevelStrings.Count > 0 && other.ProgressiveLevelStrings.Count == 0)
            {
                return -1;
            }
            if(ProgressiveLevelStrings.Count == 0 && other.ProgressiveLevelStrings.Count > 0)
            {
                return 1;
            }

            // Next sort by the total win amount regardless of the entry type.
            if(TotalWinAmount > other.TotalWinAmount)
            {
                return -1;
            }
            if(TotalWinAmount < other.TotalWinAmount)
            {
                return 1;
            }

            // Then sort by grouped wins. The entry with a higher number of grouped wins is displayed first.
            if(EntryTypeIndexes.Count > other.EntryTypeIndexes.Count)
            {
                return -1;
            }
            if(EntryTypeIndexes.Count < other.EntryTypeIndexes.Count)
            {
                return 1;
            }

            // Finally sort by indexes in ascending order.
            if(EntryTypeIndexes.Count > 0)
            {
                if(EntryTypeIndexes.Min() < other.EntryTypeIndexes.Min())
                {
                    return -1;
                }
                if(EntryTypeIndexes.Min() > other.EntryTypeIndexes.Min())
                {
                    return 1;
                }
            }

            return 0;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check if two lists of symbols are equal.
        /// </summary>
        /// <param name="first">The first symbol list to compare.</param>
        /// <param name="second">The second symbol list to compare.</param>
        /// <returns>True if two lists are equal;</returns>
        private bool AreSymbolsEqual(IList<Cell> first, IList<Cell> second)
        {
            var result = first.Count == second.Count;

            if(result)
            {
                // Converting the below statements to LINQ causes the function to take twice
                // as long to execute.
                for(var symbolCounter = 0; symbolCounter < first.Count; symbolCounter++)
                {
                    if(!Symbols[symbolCounter].Equals(second[symbolCounter]))
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
