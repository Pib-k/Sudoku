using System.Collections.Generic;


namespace Sudoku_Namespace.Models
{
    /// <summary>
    /// Cell object that goes into a 9 x 9 Sudoku array. Hold data for each cell of the Sudoku.
    /// </summary>
    public class Cell
    {
        /// <summary>
        /// Gets or sets the current value of the cell.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicator whether the cell is part of the original puzzle.
        /// </summary>
        public bool Fixed { get; set; }

        /// <summary>
        /// Gets or sets a list of pencil marks.
        /// </summary>
        public List<int> Notations { get; set; } = new List<int>();

        /// <summary>
        /// Converts the Cell object into a custom string notation.
        /// </summary>
        /// <returns>A string combining the Value, Fixed status ("F" or "U"), and joined notations.</returns>
        public override string ToString()
        {
            string stringFixed = Fixed ? "F" : "U";
            return Value.ToString() + stringFixed + string.Join("", Notations);
        }

        /// <summary>
        /// Creates a deep copy of the cell, including a list for notations.
        /// </summary>
        /// <returns>A <see cref="Cell"/> instance with the same values but independent memory references.</returns>
        public Cell Clone()
        {
            return new Cell
            {
                Value = this.Value,
                Fixed = this.Fixed,
                Notations = new List<int>(this.Notations)
            };
        }
    }
}