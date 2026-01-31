using Sudoku_Namespace.Algorithms;
using System.Collections.Generic;
using System.Drawing;

namespace Sudoku_Namespace.Utilities
{
    public enum DifficultyLevel
    {
        Easy,    // Naked, hidden singles
        Medium,  // Pairs, Pointing pairs
        Hard,    // Triples, X-wing
        Expert,  // Y-wing, Swordfish
        Extreme, // Requires backtracking/guessing
        Solved,  // Already solved 
        Custom   // Custom puzzles with unspecified difficulty
    }

    /// <summary>
    /// Provides functionality to evaluate the difficulty level of a Sudoku puzzle based on the solving techniques
    /// required.
    /// </summary>
    public class DifficultyAnalyzer
    {
        /// <summary>
        /// Evaluates the difficulty of the given Sudoku board.
        /// </summary>
        /// <param name="currentBoard"> The 9x9 integer array representing the Sudoku grid.</param>
        /// <returns>
        /// A <see cref="DifficultyLevel"/> indicating the hardest technique required to solve the puzzle, 
        /// or null if the board is invalid/unsolvable.
        /// </returns>
        public DifficultyLevel? EvaluateDifficulty(int[,] currentBoard)
        {
            int[,] boardCopy = (int[,])currentBoard.Clone();
            BitmaskSolver solver = new BitmaskSolver(boardCopy);

            if (IsBoardFull(currentBoard))
            {
                return DifficultyLevel.Solved;
            }

            if (!solver.InitializeBitmaskGrids())
            {
                return null;
            }

            DifficultyLevel highestTechniqueUsed = DifficultyLevel.Easy;
            bool solved = false;
            bool stuck = false;

            while (!solved && !stuck)
            {
                if (IsBoardFull(boardCopy))
                {
                    solved = true;
                    break;
                }

                // EASY

                (Point, int)? ns = solver.FindNakedSingle();
                if (ns != null)
                {
                    // Item 1 is target, Item 2 is the mask.
                    solver.ApplyNakedOrHiddenSingle(ns.Value.Item1, ns.Value.Item2);
                    continue;
                }


                (Point, int)? hs = solver.FindHiddenSingle();
                if (hs != null)
                {
                    // Item 1 is target, Item 2 is the mask.
                    solver.ApplyNakedOrHiddenSingle(hs.Value.Item1, hs.Value.Item2);
                    continue;
                }

                // MEDIUM

                (List<Point>, List<Point>, int)? pp = solver.FindPointingPairOrTriple();
                if (pp != null)
                {
                    // Item2 is targets list, Item3 is the mask
                    solver.ApplyPointingPairOrTriple(pp.Value.Item2, pp.Value.Item3);
                    if (highestTechniqueUsed < DifficultyLevel.Medium)
                    {
                        highestTechniqueUsed = DifficultyLevel.Medium;
                    }
                    continue;
                }

                (List<Point>, List<Point>, int)? np = solver.FindNakedPair();
                if (np != null)
                {
                    // Item2 is targets list, Item3 is the mask
                    solver.ApplyNakedPairOrTriple(np.Value.Item2, np.Value.Item3);
                    if (highestTechniqueUsed < DifficultyLevel.Medium)
                    {
                        highestTechniqueUsed = DifficultyLevel.Medium;
                    }
                    continue;
                }

                (List<Point>, int)? hp = solver.FindHiddenPair();
                if (hp != null)
                {

                    // Item1 is targets list, Item2 is the mask
                    solver.ApplyHiddenPairOrTriple(hp.Value.Item1, hp.Value.Item2);
                    if (highestTechniqueUsed < DifficultyLevel.Medium)
                    {
                        highestTechniqueUsed = DifficultyLevel.Medium;
                    }
                    continue;
                }

                // HARD

                (List<Point>, List<Point>, int)? nt = solver.FindNakedTriple();
                if (nt != null)
                {

                    // Item2 is targets list, Item3 is the mask.
                    solver.ApplyNakedPairOrTriple(nt.Value.Item2, nt.Value.Item3);
                    if (highestTechniqueUsed < DifficultyLevel.Hard)
                    {
                        highestTechniqueUsed = DifficultyLevel.Hard;
                    }
                    continue;
                }

                (List<Point>, int)? ht = solver.FindHiddenTriple();
                if (ht != null)
                {
                    // Item1 is targets list, Item2 is the mask
                    solver.ApplyHiddenPairOrTriple(ht.Value.Item1, ht.Value.Item2);
                    if (highestTechniqueUsed < DifficultyLevel.Hard)
                    {
                        highestTechniqueUsed = DifficultyLevel.Hard;
                    }
                    continue;
                }

                (List<Point>, List<Point>, int)? xw = solver.FindXWing();
                if (xw != null)
                {
                    solver.ApplyXWing(xw.Value.Item2, xw.Value.Item3);
                    if (highestTechniqueUsed < DifficultyLevel.Hard)
                    {
                        highestTechniqueUsed = DifficultyLevel.Hard;
                    }
                    continue;
                }

                // EXPERT

                (List<Point>, List<Point>, int)? yw = solver.FindYWing();
                if (yw != null)
                {
                    solver.ApplyYWing(yw.Value.Item2, yw.Value.Item3);
                    if (highestTechniqueUsed < DifficultyLevel.Expert)
                    {
                        highestTechniqueUsed = DifficultyLevel.Expert;
                    }
                    continue;
                }

                (List<Point>, List<Point>, int)? sf = solver.FindSwordfish();
                if (sf != null)
                {
                    solver.ApplySwordfish(sf.Value.Item2, sf.Value.Item3);
                    if (highestTechniqueUsed < DifficultyLevel.Expert)
                    {
                        highestTechniqueUsed = DifficultyLevel.Expert;
                    }
                    continue;
                }

                stuck = true;
            }

            // EXTREME 

            if (stuck)
            {
                if (solver.Solve() != null)
                {
                    return DifficultyLevel.Extreme;
                }

                return null; // If the board can not be solved by backtracking, it is unsolvable.
            }

            return highestTechniqueUsed;
        }

        /// <summary>
        /// Helper method to determine if the grid is filled.
        /// </summary>
        /// <param name="grid"> The current grid.</param>
        private bool IsBoardFull(int[,] grid)
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (grid[r, c] == 0) return false;
                }
            }
            return true;
        }
    }
}
