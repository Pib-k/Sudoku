using System.Collections.Generic;
using System.Drawing;

namespace Sudoku_Namespace.Models
{
    /// <summary>
    /// Represents the dimensionxdimension Sudoku grid and provides helper functions for grid manipulation.
    /// Follows the [row,col] ([y, x]) config.
    /// </summary>
    public class CoreGrid
    {
        /// <summary>
        /// 2D array of cells representing the board.
        /// </summary>
        public Cell[,] GridArray { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreGrid"/> class.
        /// </summary>
        /// <param name="dimension">The dimension of the grid.</param>
        public CoreGrid(int dimension)
        {
            GridArray = new Cell[dimension, dimension];

            for (int r = 0; r < dimension; r++)
            {
                for (int c = 0; c < dimension; c++)
                    GridArray[r, c] = new Cell();
            }
        }

        /// <summary>
        /// Sets a value and fixed state for a specific cell.
        /// </summary>
        public void SetValue(int row, int col, int value, bool isFixed)
        {
            GridArray[row, col].Value = value;
            GridArray[row, col].Fixed = isFixed;
        }

        public int GetValue(int row, int col) => GridArray[row, col].Value;

        public bool IsFixed(int row, int col) => GridArray[row, col].Fixed;

        /// <summary>
        /// Clears the value and fixed status of the cell.
        /// </summary>
        public void ClearValue(int row, int col)
        {
            GridArray[row, col].Value = 0;
            GridArray[row, col].Fixed = false;
        }

        public void ClearNotations(int row, int col) => GridArray[row, col].Notations.Clear();

        /// <summary>
        /// Adds a notation (pencil mark) to the cell if it doesn't already exist.
        /// </summary>
        public void AddNotation(int row, int col, int notation)
        {
            if (!GridArray[row, col].Notations.Contains(notation))
            {
                GridArray[row, col].Notations.Add(notation);
            }
        }

        /// <summary>
        /// Checks if a move causes any immediate conflicts in the row, column, or block.
        /// </summary>
        public bool IsValidMove(int row, int col, int value)
        {
            if (GridArray[row, col].Value != 0)
                return false;

            return GetConflictPositions(row, col, value).Count == 0;
        }


        /// <summary>
        /// Identifies conflicting cells for a given value at a specific position.
        /// </summary>
        public HashSet<Point> GetConflictPositions(int row, int col, int value)
        {
            HashSet<Point> conflicts = new HashSet<Point>();
            Point self = new Point(col, row);

            if (value == 0)
            {
                return conflicts;
            }

            int size = GridArray.GetLength(0); // almost always 9, but in case we would like to make bigger/smaller board sizes this will still work

            // Check the row
            for (int c = 0; c < size; c++)
            {
                if (GridArray[row, c].Value == value)
                    conflicts.Add(new Point(c, row));
            }

            // Check the column
            for (int r = 0; r < size; r++)
            {
                if (GridArray[r, col].Value == value)
                    conflicts.Add(new Point(col, r));
            }

            // Check the 3x3 block for the same number
            int startRow = (row / 3) * 3;
            int startCol = (col / 3) * 3;

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (GridArray[startRow + r, startCol + c].Value == value)
                    {
                        conflicts.Add(new Point(startCol + c, startRow + r));
                    }
                }
            }

            conflicts.Remove(self);

            return conflicts;
        }

        /// <summary>
        /// Returns a simple 2D integer array of the current grid values.
        /// </summary>
        public int[,] GetValueArray()
        {
            int[,] valueGrid = new int[9, 9];

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    valueGrid[r, c] = GridArray[r, c].Value;
                }
            }
            return valueGrid;
        }

        /// <summary>
        /// Returns a jagged array copy of the grid.    
        /// </summary>
        public Cell[][] GetJaggedGrid()
        {
            Cell[][] jaggedArray = new Cell[9][];

            for (int i = 0; i < jaggedArray.Length; i++)
            {
                jaggedArray[i] = new Cell[jaggedArray.Length];

                for (int j = 0; j < jaggedArray[i].Length; j++)
                {
                    jaggedArray[i][j] = GridArray[i, j];
                }
            }
            return jaggedArray;
        }

        /// <summary>
        /// Resets the grid.
        /// </summary>
        /// <param name="solveMode">If true, only non-fixed (user-entered) values are cleared.</param>
        public void Reset(bool solveMode)
        {
            foreach (Cell cell in GridArray)
            {
                // if user is not solving the sudoku, reset all the values
                if (solveMode == false)
                {
                    cell.Value = 0;
                    cell.Notations.Clear();
                    cell.Fixed = false;
                }
                // if user is solving the sudoku, reset all the non fixed values.
                else if (cell.Fixed != solveMode)
                {
                    cell.Value = 0;
                    cell.Notations.Clear();
                }
            }
        }

        /// <summary>
        /// Converts an integer value board to a Cell array.
        /// </summary>
        public Cell[,] ValuesToCellArray(int[,] board)
        {

            Cell[,] cells = new Cell[9, 9];
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    cells[r, c] = new Cell();
                    int value = board[r, c];
                    cells[r, c].Value = value;
                    if (value != 0)
                    {
                        cells[r, c].Fixed = true;
                    }
                }
            }
            return cells;
        }
    }
}