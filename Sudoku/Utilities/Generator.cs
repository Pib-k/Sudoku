using Sudoku_Namespace.Algorithms;
using System;
using System.Linq;

namespace Sudoku_Namespace.Utilities
{
    /// <summary>
    /// Responsible for generating valid Sudoku puzzles of specific difficulty levels.
    /// </summary>
    public class Generator
    {
        private const int DIMENSION = 9;
        private Random random = new Random();
        private int[,] sudoku = new int[DIMENSION, DIMENSION];
        private int[] rows = new int[DIMENSION];
        private int[] cols = new int[DIMENSION];
        private int[] blocks = new int[DIMENSION];

        /// <summary>
        /// Generates a Sudoku puzzle matching the specified difficulty level.
        /// </summary>
        /// <param name="difficultyNeeded">The target <see cref="DifficultyLevel"/> for the generated puzzle.</param>
        /// <returns>
        /// A 9x9 integer array representing the puzzle, where 0 indicates an empty cell. 
        /// Returns <c>null</c> if a puzzle matching the criteria could not be generated within the attempt limit.
        /// </returns>
        public int[,] GenerateSudoku(DifficultyLevel difficultyNeeded)
        {
            DifficultyAnalyzer da = new DifficultyAnalyzer();

            int attempts = 5000;

            int targetGivens = difficultyNeeded switch
            {
                DifficultyLevel.Easy => 46,
                DifficultyLevel.Medium => 34,
                DifficultyLevel.Hard => 28,
                DifficultyLevel.Expert => 22,
                DifficultyLevel.Extreme => 17,
                _ => 81,
            };
            for (int i = 0; i < attempts; i++)
            {
                int[,] seed = SeedGenerator();
                int[,] workSudoku = (int[,])seed.Clone();

                int givens = 81;
                var places = Enumerable.Range(0, 81).OrderBy(x => random.Next()).ToArray();

                foreach (int p in places)
                {

                    if (givens <= targetGivens)
                    {
                        break;
                    }

                    int r = p / DIMENSION;
                    int c = p % DIMENSION;
                    int oldValue = workSudoku[r, c];
                    workSudoku[r, c] = 0;

                    int[,] solverGrid = (int[,])workSudoku.Clone();
                    BitmaskSolver solver = new BitmaskSolver(solverGrid);
                    solver.InitializeBitmaskGrids();


                    if (solver.HasUniqueSolution())
                    {
                        givens--;
                    }
                    else
                    {
                        workSudoku[r, c] = oldValue;
                    }
                }

                if (difficultyNeeded == da.EvaluateDifficulty(workSudoku))
                {
                    return workSudoku;
                }
            }
            return null;
        }

        /// <summary>
        /// Generates a completely filled, valid 9x9 Sudoku grid to serve as a starting point (seed).
        /// </summary>
        /// /// <returns>A 9x9 integer array containing a valid Sudoku solution.</returns>
        public int[,] SeedGenerator()
        {
            sudoku = new int[DIMENSION, DIMENSION];
            Array.Clear(rows, 0, rows.Length);
            Array.Clear(cols, 0, cols.Length);
            Array.Clear(blocks, 0, blocks.Length);

            if (FillSudoku(0) == true)
            {
                return sudoku;
            }
            return sudoku;
        }

        /// <summary>
        /// Recursively fills the Sudoku grid using a backtracking algorithm.
        /// </summary>
        /// <param name="index">The linear index (0-80) of the current cell being filled.</param>
        /// <returns><c>true</c> if the grid is successfully filled; otherwise, <c>false</c> triggers backtracking.</returns>
        private bool FillSudoku(int index)
        {
            if (index >= 81)
            {
                return true;
            }

            int r = index / DIMENSION;
            int c = index % DIMENSION;
            int b = (r / 3) * 3 + c / 3;

            var candidates = Enumerable.Range(0, 9).OrderBy(x => random.Next()).ToArray();

            foreach (int number in candidates)
            {
                int mask = 1 << number;

                if (((rows[r] | cols[c] | blocks[b]) & mask) == 0)
                {
                    sudoku[r, c] = number + 1;
                    rows[r] |= mask;
                    cols[c] |= mask;
                    blocks[b] |= mask;

                    if (FillSudoku(index + 1))
                    {
                        return true;
                    }

                    rows[r] &= ~mask;
                    cols[c] &= ~mask;
                    blocks[b] &= ~mask;
                    sudoku[r, c] = 0;
                }
            }
            return false;
        }
    }
}
