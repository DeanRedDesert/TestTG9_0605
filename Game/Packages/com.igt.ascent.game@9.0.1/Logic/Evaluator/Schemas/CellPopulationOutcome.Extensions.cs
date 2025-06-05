//-----------------------------------------------------------------------
// <copyright file = "CellPopulationOutcome.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the generated CellPopulationOutcome class.
    /// </summary>
    public partial class CellPopulationOutcome : ICompactSerializable
    {
        [NonSerialized]
        private Dictionary<Cell, OutcomeCell> cellMap;

        /// <summary>
        /// Construct an internal map of locations to outcome cells.
        /// </summary>
        private void BuildCellMap()
        {
            cellMap = new Dictionary<Cell, OutcomeCell>();

            foreach(var cell in
                PopulationEntryList.SelectMany(popuationEntry => popuationEntry.OutcomeCellList))
            {
                cellMap[new Cell(cell.Cell)] = cell;
            }
        }

        /// <summary>
        /// Get an outcome cell by its cell location.
        /// </summary>
        /// <remarks>Useful for locating cells in a symbol window.</remarks>
        /// <param name="cell">Cell representing the location of the outcome cell.</param>
        /// <returns>The cell at the specified coordinates.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the cell specified is not in the population outcome.
        /// </exception>
        public OutcomeCell GetOutcomeCellByCell(Cell cell)
        {
            if(cellMap == null)
            {
                BuildCellMap();
            }

            //The cell map will not be null because BuildCellMap will construct one.
            // Resharper warning disabled because BuildCellMap creates the dictionary.
            // ReSharper disable once PossibleNullReferenceException
            if(!cellMap.ContainsKey(cell))
            {
                throw new KeyNotFoundException("The specified cell: " + cell +
                                               " was not found in the cell population outcome: " + name);
            }

            return cellMap[cell];
        }

        /// <summary>
        /// Clones all the cells of the specified layer into the destination layer.
        /// </summary>
        /// <param name="sourceLayer">The source layer to copy cells from.</param>
        /// <param name="destinationLayer">The new layer to copy the cells to.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="sourceLayer"/> and <paramref name="destinationLayer"/> are equal.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the <paramref name="destinationLayer"/> already has cells on it.
        /// </exception>
        public void CloneCellLayer(uint sourceLayer, uint destinationLayer)
        {
            if(sourceLayer == destinationLayer)
            {
                throw new ArgumentOutOfRangeException(
                    string.Format("The destination layer {0} cannot be the same as the source layer {1}.",
                        destinationLayer, sourceLayer));
            }

            foreach(var populationEntry in PopulationEntryList)
            {
                if(populationEntry.OutcomeCellList.Any(outcomeCell => outcomeCell.Cell.layer == destinationLayer))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "Unable to copy to layer {0} because population entry {1} already has cells on that layer.",
                            destinationLayer, populationEntry.name));
                }

                // This new collection is made since the foreach loop changes the populationEntry collection as it goes.
                var cellsToCopy =
                    populationEntry.OutcomeCellList.Where(outcomeCell => outcomeCell.Cell.layer == sourceLayer).ToList();
                foreach(var sourceCell in cellsToCopy)
                {
                    var newOutcomeCell = new OutcomeCell
                    {
                        symbolID = sourceCell.symbolID,
                        stopSpecified = sourceCell.stopSpecified,
                        stop = sourceCell.stop,
                        Cell = new Cell(sourceCell.Cell)
                    };
                    newOutcomeCell.Cell.layer = destinationLayer;
                    populationEntry.OutcomeCellList.Add(newOutcomeCell);
                    if(cellMap != null)
                    {
                        cellMap[new Cell(newOutcomeCell.Cell)] = newOutcomeCell;
                    }
                }
            }
        }

        /// <summary>
        /// Override of toString which provides a readable CellPopulationOutcome arranged as it normally would be in a
        /// symbol window.
        /// </summary>
        /// <returns>String representation of the CellPopulationOutcome</returns>
        public override string ToString()
        {
            var rows = new List<StringBuilder>();

            foreach(var populationEntry in PopulationEntryList)
            {
                for(var cellIndex = 0; cellIndex < populationEntry.OutcomeCellList.Count; cellIndex++)
                {
                    var cell = populationEntry.OutcomeCellList[cellIndex];

                    if(rows.Count <= cellIndex)
                    {
                        rows.Add(new StringBuilder());
                    }

                    rows[cellIndex].Append("Symbol: " + cell.symbolID + " " + cell.Cell);
                    if(cell.stopSpecified)
                    {
                        rows[cellIndex].Append(" Stop: " + cell.stop);
                    }
                    rows[cellIndex].Append("\t");
                }
            }

            var finalString = new StringBuilder();
            foreach(var row in rows)
            {
                finalString.Append(row);
                finalString.Append(Environment.NewLine);
            }

            return finalString.ToString();
        }

        /// <summary>
        /// Get the reel stops for the specified row. This overload assumes that the desired stop positions are for
        /// layer 0.
        /// </summary>
        /// <param name="row">Row to get the stops from in the symbol window.</param>
        /// <returns>The stops in layer 0 for the specified row.</returns>
        public List<uint> GetStops(int row)
        {
            return GetStops(row, 0);
        }

        /// <summary>
        /// Get the reel stops for the specified symbol window.
        /// </summary>
        /// <param name="window">The window to get reel stops for.</param>
        /// <returns>The physical stops at the strip offset index of each cell population.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the given <paramref name="window"/> contains an entry not contained in this
        /// CellPopulationOutcome.
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when a strip offset index is not within range of the corresponding PopulationEntry.
        /// </exception>
        public List<uint> GetStopsForWindow(CellPopulation window)
        {
            var stops = new List<uint>();
            foreach(var populationEntry in window.PopulationEntry)
            {
                var populatedEntry = PopulationEntryList.FirstOrDefault(entry => entry.name == populationEntry.name);

                if(populatedEntry == null)
                {
                    throw new KeyNotFoundException("CellPopulationOutcome does not contain an entry for: " +
                                                   populationEntry.name);
                }

                var cell = populatedEntry.OutcomeCellList.ElementAtOrDefault(populationEntry.stripOffsetIndex);
                if(cell == null)
                {
                    throw new IndexOutOfRangeException(string.Format(CultureInfo.InvariantCulture,
                                                                     "The desired strip offset was out of range of" +
                                                                     " the population entry. Entry: {0}, Strip "
                                                                     + " Offset: {1}",
                                                                     populationEntry.name,
                                                                     populationEntry.stripOffsetIndex));
                }

                stops.Add(cell.stop);
            }

            return stops;
        }

        /// <summary>
        /// Get the reel stops for the specified layer and row.
        /// </summary>
        /// <param name="layer">Layer of the stops in the symbol window.</param>
        /// <param name="row">Row to get the stops from in the symbol window.</param>
        /// <returns>The stops at the given layer and row.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if either the row or layer are not within range of the provided symbol window.
        /// </exception>
        public List<uint> GetStops(int row, int layer)
        {
            var stopList = new List<uint>();

            for(var populationIndex = 0; populationIndex < PopulationEntryList.Count; populationIndex++)
            {
                var populationEntry = PopulationEntryList[populationIndex];
                var layerFound = false;
                var rowFound = false;
                foreach(
                    var outcomeCell in
                        populationEntry.OutcomeCellList.Where(
                            outcomeCell => outcomeCell.Cell.layer == layer))
                {
                    layerFound = true;
                    if(outcomeCell.Cell.row == row)
                    {
                        stopList.Add(outcomeCell.stop);
                        rowFound = true;
                        break;
                    }
                }

                if(!layerFound)
                {
                    throw new ArgumentOutOfRangeException("layer",
                                                          "The specified layer was not in the population entry. Index: " +
                                                          populationIndex);
                }
                if(!rowFound)
                {
                    throw new ArgumentOutOfRangeException("row",
                                      "The specified row was not in the population entry. Index: " +
                                      populationIndex);
                }
            }

            return stopList;
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, name);
            CompactSerializer.WriteList(stream, PopulationEntryList);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            name = CompactSerializer.ReadString(stream);
            PopulationEntryList = CompactSerializer.ReadListSerializable<PopulationEntry>(stream);
        }

        #endregion
    }
}
