//-----------------------------------------------------------------------
// <copyright file = "LogicBlock.cs" company = "IGT">
//     Copyright (c) IGT 2017.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Providers
{
    using System.Collections.Generic;
    using System.Linq;
    using CompactSerialization;

    /// <summary>
    /// Class that defines a block consisting of symbols. 
    /// A block contains one or more symbols and some common properties. 
    /// E.g., Breakable, Non Breakable. Linked, Non-Linked.
    /// </summary>
    public class SymbolBlock: ICompactSerializable
    {
        /// <summary>
        /// Break type of a block.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public enum BlockBreakType
        {
            /// <summary>
            /// Block can be broken if this is required.
            /// </summary>
            Breakable = 0,
            /// <summary>
            /// Block should never be broken.
            /// </summary>
            NotBreakable = 1
        }

        /// <summary>
        /// Defines if a block is linked to another reel or not.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public enum BlockLinkType
        {
            /// <summary>
            /// Block is not linked to another reel.
            /// </summary>
            NotLinked = 0,
            /// <summary>
            /// Block should be automatically linked to another reel if the reel is linked (or the master)
            /// and on the same reel position there is also a linked block.
            /// </summary>
            Linked = 1
        }

        #region Properties

        /// <summary>
        /// The break type of this block.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public BlockBreakType BreakType { get; private set; }

        /// <summary>
        /// The linked type of this block.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public BlockLinkType LinkType { get; private set;  }

        /// <summary>
        /// The list of symbols that belong to this block.
        /// </summary>
        public IList<string> Symbols { get; private set; }

        #endregion

        /// <summary>
        /// Construct a Breakable non linked Block with a empty list of symbols.
        /// </summary>
        public SymbolBlock() : this(new List<string>())
        {
        }

        /// <summary>
        /// Construct a Breakable non linked Block as a list of symbols.
        /// </summary>
        /// <param name="symbols">The symbol IDs the block consists of.</param>
        public SymbolBlock(IList<string> symbols) : this(symbols, BlockBreakType.Breakable, BlockLinkType.NotLinked )
        {
        }

        /// <summary>
        /// Construct a non linked block with given symbols and break type.
        /// </summary>
        /// <param name="symbols">>The symbol IDs the block consists of.</param>
        /// <param name="breakType">The break type if the symbol block.</param>
        public SymbolBlock(IList<string> symbols, BlockBreakType breakType) 
            : this(symbols, breakType, BlockLinkType.NotLinked)
        {
        }

        /// <summary>
        /// Construct a block with given symbols, break and link type.
        /// </summary>
        /// <param name="symbols">>The symbol IDs the block consists of.</param>
        /// <param name="breakType">The break type if the symbol block.</param>
        /// <param name="linkType">The link type if the symbol block.</param>
        public SymbolBlock(IList<string> symbols, BlockBreakType breakType, BlockLinkType linkType)
        {
            Symbols = symbols.ToList();
            BreakType = breakType;
            LinkType = linkType;
        }

        #region ICompactSerializable

        /// <inheritdoc />
        public void Serialize(System.IO.Stream stream)
        {
            CompactSerializer.Write(stream, LinkType);
            CompactSerializer.Write(stream, BreakType);
            CompactSerializer.WriteList(stream, Symbols);
        }

        /// <inheritdoc />
        public void Deserialize(System.IO.Stream stream)
        {
            LinkType = CompactSerializer.ReadEnum<BlockLinkType>(stream);
            BreakType = CompactSerializer.ReadEnum<BlockBreakType>(stream);
            Symbols = CompactSerializer.ReadListString(stream);
        }

        #endregion
    }
}