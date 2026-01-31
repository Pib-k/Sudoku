using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

/* 
 * uses [row, col] = [y, x]
 * OR (|) - sets a bit to 1 if either of the corresponding bits in the operands is 1.
 * AND (&) - sets a bit to 1 if both the corresponding bits in the operands are 1.
 * XOR (^) - sets a bit to 1 if the corresponding bits in the operands are different.
 * NOT (~) - flips the bits in the operand, i.e., sets 0 bits to 1 and 1 bits to 0.
 */
namespace Sudoku_Namespace.Algorithms
{
    /// <summary>
    /// A Sudoku solver using Backtracking optimized with Bitmasking.
    /// <para>
    /// This solver represents the grid constraints (Row, Column, Block) using integer bitmasks 
    /// (9 bits), where the <c>n</c>-th bit being set means the number <c>n+1</c> is used/available.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <br/>
    public class BitmaskSolver
    {
        // shift 1 9 times to the left and subtract 1. Results in 111 111 111 = 511
        private const int ALL = (1 << 9) - 1;
        private int[] rows;
        private int[] cols;
        private int[] blocks;
        private int[,] cells;

        private int bestRow;
        private int bestCol;
        private int currentMask;
        private int[,] candidates;

        private int[,] _userCandidates;

        /// <summary>
        /// Gets or sets the candidates manually notated by the user in the UI. Used to verify if generated hints are relevant to the user's current notes.
        /// </summary>
        public int[,] UserCandidates
        {
            get { return _userCandidates; }
            set
            {
                _userCandidates = value;
            }
        }

        /// <summary>
        /// Delegate used to update the UI board during animated solving.
        /// </summary>
        /// <param name="currentBoard">The current state of the integer grid.</param>
        public delegate void UpdateBoardDelegate(int[,] currentBoard);

        /// <summary>
        /// Initializes a new instance of the <see cref="BitmaskSolver"/> class with an initial grid state.
        /// </summary>
        /// <param name="grid">A 9x9 integer array representing the Sudoku board.</param>
        public BitmaskSolver(int[,] grid)
        {
            rows = new int[9]; // keeps track of values in rows i.e. row[1] = 000 100 110 -> numbers 2, 3 and 6 are in the row
            cols = new int[9];
            blocks = new int[9];
            cells = grid;

            candidates = new int[9, 9];
        }

        /// <summary>
        /// Iterates over the initial grid state to implement the bitmask constraints for Rows, Columns, and Blocks.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the board was initialized successfully; 
        /// <c>false</c> if the initial board contains duplicate numbers in a unit (invalid state).
        /// </returns>
        public bool InitializeBitmaskGrids()
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    int d = cells[r, c];
                    if (d != 0)
                    {
                        // you want the d-1'th position. For example d = 2 -> 000 000 010
                        int m = 1 << (d - 1);
                        int blockNumber = GetBlockNumber(r, c);

                        if (((rows[r] & m) != 0) || ((cols[c] & m) != 0) || ((blocks[blockNumber] & m) != 0))
                        {
                            return false; // Wrong input array (with duplicates)
                        }

                        // Add number to used numbers
                        rows[r] |= m;
                        cols[c] |= m;
                        blocks[blockNumber] |= m;
                        this.cells[r, c] = m;
                        candidates[r, c] = 0; // No candidates for filled cells, FOR IMPLEMENTING BITMASK REPRESENTATION OF CANDIDATES
                    }
                    else
                    {
                        this.cells[r, c] = 0; // If point has no number, we say that can take all values
                        candidates[r, c] = ALL; //All candidates possible at start, FOR IMPLEMENTING BITMASK REPRESENTATION OF CANDIDATES
                    }
                }
            }
            UpdateCandidates();
            return true;
        }

        /// <summary>
        /// Recalculates the possible candidates for each cell in the grid.
        /// </summary>
        public void UpdateCandidates()
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (cells[r, c] == 0)
                    {
                        candidates[r, c] = ~(rows[r] | cols[c] | blocks[GetBlockNumber(r, c)]) & ALL;
                    }
                    else
                    {
                        candidates[r, c] = 0;
                    }
                }
            }
        }

        private void ResetConstraints()
        {
            Array.Clear(rows, 0, rows.Length);
            Array.Clear(cols, 0, cols.Length);
            Array.Clear(blocks, 0, blocks.Length);

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    int mask = cells[r, c];
                    if (mask != 0)
                    {
                        rows[r] |= mask;
                        cols[c] |= mask;
                        blocks[GetBlockNumber(r, c)] |= mask;
                    }
                }
            }
        }

        /// <summary>
        /// Solves the Sudoku puzzle.
        /// </summary>
        /// <returns>A 9x9 integer array containing the solution, or <c>null</c> if unsolvable.</returns>
        public int[,] Solve()
        {
            // Start solving
            bool solved = BruteSolver();
            if (solved)
            {
                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        this.cells[r, c] = BitToNumber(this.cells[r, c]);
                    }
                }
                return this.cells;
            }
            return null;
        }



        /// <summary>
        /// Heuristic: Locates the empty cell with the fewest possible candidates (Minimum Remaining Values).
        /// </summary>
        /// <returns><c>true</c> if an empty cell was found; <c>false</c> if the board is full.</returns>
        private bool FindMinimumRemainingValue()
        {
            // Reset best position
            bestRow = -1;
            bestCol = -1;
            int minCount = int.MaxValue;
            bool found = false;

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (cells[r, c] == 0)
                    {
                        // Possible values not already in the row, column or block
                        int mask = ~(rows[r] | cols[c] | blocks[GetBlockNumber(r, c)]) & ALL;
                        int count = GetPopCount(mask);

                        if (count < minCount)
                        {
                            minCount = count;
                            bestCol = c;
                            bestRow = r;
                            found = true;
                            currentMask = mask;

                            if (count == 1) // We can not go lower than 1 so we choose this cell.
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return found;
        }

        /// <summary>
        /// Recursive brute force backtracking solver. Uses Minimum Remaining Value and bitmasking to try candidates.
        /// </summary>
        /// <returns><c>true</c> if a solution is found.</returns>
        private bool BruteSolver()
        {

            if (!FindMinimumRemainingValue())
            {
                return true;
            }

            // remember for backtracking
            int r = bestRow;
            int c = bestCol;
            int oldMask = currentMask;

            while (oldMask != 0)
            {

                int bit = oldMask & -oldMask; // gets the LSB
                oldMask ^= bit;

                cells[r, c] = bit;
                rows[r] |= bit;
                cols[c] |= bit;
                blocks[GetBlockNumber(r, c)] |= bit;

                if (BruteSolver()) { return true; }

                // backtrack
                cells[r, c] = 0;
                rows[r] ^= bit;
                cols[c] ^= bit;
                blocks[GetBlockNumber(r, c)] ^= bit;
            }
            return false;
        }

        /// <summary>
        /// Initiates the asynchronous animated solver.
        /// </summary>
        public async Task<bool> SolveAsync(UpdateBoardDelegate updateUI, int delayMs)
        {
            bool solved = await BruteSolverAnimated(updateUI, delayMs);
            return solved;
        }

        /// <summary>
        /// An animated version of the brute force backtracking solver that updates the UI between steps.
        /// </summary>
        /// <param name="updateUI">The delegate to call to repaint the screen.</param>
        /// <param name="delayMs">The delay between steps.</param>
        private async Task<bool> BruteSolverAnimated(UpdateBoardDelegate updateUI, int delayMs)
        {
            if (!FindMinimumRemainingValue())
            {
                return true;
            }

            // remember for backtracking
            int r = bestRow;
            int c = bestCol;
            int oldMask = currentMask;

            while (oldMask != 0)
            {

                int bit = oldMask & -oldMask; // gets the LSB
                oldMask ^= bit;

                cells[r, c] = bit;
                rows[r] |= bit;
                cols[c] |= bit;
                blocks[GetBlockNumber(r, c)] |= bit;


                int number = BitToNumber(bit);

                await Task.Delay(delayMs);
                updateUI?.Invoke(ConvertBitmasksToNumbers());

                if (await BruteSolverAnimated(updateUI, delayMs)) { return true; }

                // backtrack
                cells[r, c] = 0;
                rows[r] ^= bit;
                cols[c] ^= bit;
                blocks[GetBlockNumber(r, c)] ^= bit;

                updateUI?.Invoke(ConvertBitmasksToNumbers());
            }
            return false;
        }

        private int[,] ConvertBitmasksToNumbers()
        {
            int[,] animationGrid = new int[9, 9];
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {

                    if (cells[r, c] != 0)
                    {
                        animationGrid[r, c] = BitToNumber(cells[r, c]);
                    }
                }
            }
            return animationGrid;
        }


        /// <summary>
        /// Determines whether the current Sudoku puzzle has exactly one unique solution.
        /// </summary>
        /// <returns>
        /// <c>true</c> if there is exactly one solution; otherwise, <c>false</c>.
        /// </returns>
        public bool HasUniqueSolution()
        {
            ResetConstraints();
            return SolutionCount(2) == 1;
        }

        /// <summary>
        /// Recursively counts the number of valid solutions for the current grid state using backtracking.
        /// </summary>
        /// <param name="max">The maximum number of solutions to find before stopping the search.</param>
        /// <returns>The number of solutions found, cap
        private int SolutionCount(int max)
        {
            // If no empty cells left, we have found a solution.
            if (!FindMinimumRemainingValue())
            {
                return 1;
            }

            // Store current best for backtracking.
            int r = bestRow;
            int c = bestCol;
            int possibleMask = currentMask;
            int totalFound = 0;

            // Loop over every possible candidate
            while (possibleMask != 0)
            {
                // Remove the bit
                int bit = possibleMask & -possibleMask;
                possibleMask ^= bit;

                // Apply values
                cells[r, c] = bit;
                rows[r] |= bit;
                cols[c] |= bit;
                blocks[GetBlockNumber(r, c)] |= bit;

                // Ask next recursion level to find a solution.
                totalFound += SolutionCount(max - totalFound);

                // Undo changes before returning or trying the next number.
                cells[r, c] = 0;
                rows[r] ^= bit;
                cols[c] ^= bit;
                blocks[GetBlockNumber(r, c)] ^= bit;

                // If we hit our max we stop looking.
                if (totalFound >= max)
                {
                    return totalFound;
                }
            }

            return totalFound;
        }

        private int GetBlockNumber(int r, int c) => (r / 3) * 3 + c / 3;

        private int GetPopCount(int bitWord)
        {
            return BitOperations.PopCount((uint)bitWord);
        }

        public int BitToNumber(int bitWord)
        {
            if (bitWord == 0) return 0;
            return 32 - BitOperations.LeadingZeroCount((uint)bitWord);
        }

        private bool IsHintUseful(List<Point> targets, int mask)
        {
            if (_userCandidates == null) { return true; }

            foreach (Point p in targets)
            {
                if ((_userCandidates[p.Y, p.X] & mask) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds a cell that has only one valid candidate remaining.
        /// </summary>
        /// <returns>A tuple containing the Cell coordinates and the Candidate value.</returns>
        public (Point, int)? FindNakedSingle()
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (GetPopCount(candidates[r, c]) == 1)
                    {
                        return (new Point(c, r), candidates[r, c]);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a number that can only be placed in one specific cell within a unit (Row, Col, or Block),
        /// even if that cell has other candidates.
        /// </summary>
        /// <returns>A tuple containing the Cell coordinates and the forced Candidate value.</returns>
        public (Point, int)? FindHiddenSingle()
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    int temporaryMask = candidates[r, c];

                    // Needs to have at least 2 candidates.
                    if (GetPopCount(temporaryMask) <= 1)
                    {
                        continue;
                    }

                    int rowUnion = 0;
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != c)
                        {
                            rowUnion |= candidates[r, i];
                        }
                    }

                    if ((temporaryMask & ~rowUnion) != 0)
                    {
                        return (new Point(c, r), temporaryMask & ~rowUnion);
                    }

                    int colUnion = 0;
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != r)
                        {
                            colUnion |= candidates[i, c];
                        }
                    }

                    if ((temporaryMask & ~colUnion) != 0)
                    {
                        return (new Point(c, r), temporaryMask & ~colUnion);
                    }

                    int blockUnion = 0;
                    int startRow = (r / 3) * 3;
                    int startCol = (c / 3) * 3;

                    for (int br = 0; br < 3; br++)
                    {
                        for (int bc = 0; bc < 3; bc++)
                        {
                            int realR = startRow + br;
                            int realC = startCol + bc;

                            if (realR == r && realC == c) continue;

                            blockUnion |= candidates[realR, realC];
                        }
                    }

                    if ((temporaryMask & ~blockUnion) != 0)
                    {
                        // Found hidden single in the box.
                        return (new Point(c, r), temporaryMask & ~blockUnion);
                    }
                }
            }
            return null;
        }

        // Helper method to find cells in a unit that contain specific candidates (targets), not counting the source cells.
        private List<Point> GetNakedTargets(List<Point> sources, int mask, string type)
        {
            List<Point> targets = new List<Point>();
            Point p1 = sources[0];

            if (type == "row")
            {
                int r = p1.Y;
                for (int c = 0; c < 9; c++)
                {
                    bool isSource = false;
                    for (int i = 0; i < sources.Count; i++)
                    {
                        if (sources[i].X == c) { isSource = true; break; }
                    }
                    if (isSource) { continue; }

                    if ((candidates[r, c] & mask) != 0)
                    {
                        targets.Add(new Point(c, r));
                    }
                }
            }
            else if (type == "col")
            {
                int c = p1.X;
                for (int r = 0; r < 9; r++)
                {
                    bool isSource = false;
                    for (int i = 0; i < sources.Count; i++)
                    {
                        if (sources[i].Y == r) { isSource = true; break; }
                    }
                    if (isSource) { continue; }

                    if ((candidates[r, c] & mask) != 0)
                    {
                        targets.Add(new Point(c, r));
                    }
                }
            }
            else if (type == "block")
            {
                int startRow = (p1.Y / 3) * 3;
                int startCol = (p1.X / 3) * 3;

                for (int i = 0; i < 9; i++)
                {
                    int r = startRow + (i / 3);
                    int c = startCol + (i % 3);

                    bool isSource = false;
                    for (int k = 0; k < sources.Count; k++)
                    {
                        if (sources[k].X == c && sources[k].Y == r) { isSource = true; break; }
                    }
                    if (isSource) { continue; }

                    if ((candidates[r, c] & mask) != 0) { targets.Add(new Point(c, r)); }
                }
            }
            return targets;
        }

        /// <summary>
        /// Finds two cells in a unit (Row/Col/Block) that contain exactly the same two candidates.
        /// These candidates can be removed from all other cells in that unit.
        /// </summary>
        /// <returns>A tuple containing the target coordinates where the candidates need to be removed, the source of the technique and the candidates.</returns>
        public (List<Point>, List<Point>, int)? FindNakedPair()
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    int temporaryMask = candidates[r, c];

                    // Requires exactly two values for naked pair
                    if (GetPopCount(temporaryMask) != 2)
                    {
                        continue;
                    }

                    // Row: we start at c + 1 to avoid self-check and duplicate checking from past checks
                    for (int i = c + 1; i < 9; i++)
                    {
                        if (candidates[r, i] == temporaryMask)
                        {
                            // Only found useful hidden board if it affects the board.
                            List<Point> sources = new List<Point> { new Point(c, r), new Point(i, r) };
                            List<Point> targets = GetNakedTargets(sources, temporaryMask, "row");

                            if (targets.Count > 0 && IsHintUseful(targets, temporaryMask))
                            {
                                return (sources, targets, temporaryMask);
                            }
                        }
                    }

                    // Column: we start at r + 1 to avoid self-check and duplicate checking from past checks
                    for (int i = r + 1; i < 9; i++)
                    {
                        if (candidates[i, c] == temporaryMask)
                        {
                            // Only found useful hidden board if it affects the board.
                            List<Point> sources = new List<Point> { new Point(c, r), new Point(c, i) };
                            List<Point> targets = GetNakedTargets(sources, temporaryMask, "col");

                            if (targets.Count > 0 && IsHintUseful(targets, temporaryMask))
                            {
                                return (sources, targets, temporaryMask);
                            }
                        }
                    }

                    // Block
                    int startRow = (r / 3) * 3;
                    int startCol = (c / 3) * 3;

                    for (int br = 0; br < 3; br++)
                    {
                        for (int bc = 0; bc < 3; bc++)
                        {
                            int realR = startRow + br;
                            int realC = startCol + bc;

                            // Only check on diagonal
                            if (realR <= r || realC == c)
                            {
                                continue;
                            }

                            if (temporaryMask == candidates[realR, realC])
                            {
                                // Only found useful hidden board if it affects the board.
                                List<Point> sources = new List<Point> { new Point(c, r), new Point(realC, realR) };
                                List<Point> targets = GetNakedTargets(sources, temporaryMask, "block");

                                if (targets.Count > 0 && IsHintUseful(targets, temporaryMask))
                                {
                                    return (sources, targets, temporaryMask);
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds two candidates that appear only in two specific cells within a unit.
        /// Any other candidates in these two cells can be removed.
        /// </summary>
        /// <returns>Tuple containing the hidden pair and the two candidates we found.</returns>
        public (List<Point>, int)? FindHiddenPair()
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    int mask1 = candidates[r, c];

                    // Row: we start at c + 1 to avoid self-check and duplicate checking from past checks
                    for (int i = c + 1; i < 9; i++)
                    {
                        int mask2 = candidates[r, i];
                        int possiblePairCandidate = candidates[r, i] & mask1;

                        // Needs at least two of the same candidates.
                        if (GetPopCount(possiblePairCandidate) >= 2)
                        {
                            int noiseMask = 0;
                            for (int j = 0; j < 9; j++)
                            {
                                if (j == i || j == c)
                                {
                                    continue;
                                }

                                noiseMask |= candidates[r, j];
                            }

                            // Found hidden pair
                            if (GetPopCount(possiblePairCandidate & ~noiseMask) == 2)
                            {
                                if (GetPopCount(mask1) > 2 || GetPopCount(mask2) > 2)
                                {
                                    List<Point> sources = new List<Point> { new Point(c, r), new Point(i, r) };
                                    int pairMask = possiblePairCandidate & ~noiseMask;
                                    if (IsHintUseful(sources, ~pairMask & ALL))
                                    {
                                        return (sources, pairMask);
                                    }
                                }

                            }
                        }
                    }

                    // Col: we start at r + 1 to avoid self-check and duplicate checking from past checks
                    for (int i = r + 1; i < 9; i++)
                    {
                        int mask2 = candidates[i, c];
                        int possiblePairCandidate = mask1 & mask2;

                        // Needs at least two of the same candidates.
                        if (GetPopCount(possiblePairCandidate) >= 2)
                        {
                            int noiseMask = 0;

                            for (int j = 0; j < 9; j++)
                            {
                                if (j == i || j == r)
                                {
                                    continue;
                                }

                                noiseMask |= candidates[j, c];
                            }

                            // Found hidden pair.
                            if (GetPopCount(possiblePairCandidate & ~noiseMask) == 2)
                            {
                                if (GetPopCount(mask1) > 2 || GetPopCount(mask2) > 2)
                                {
                                    List<Point> sources = new List<Point> { new Point(c, r), new Point(c, i) };
                                    int pairMask = possiblePairCandidate & ~noiseMask;
                                    if (IsHintUseful(sources, ~pairMask & ALL))
                                    {
                                        return (sources, pairMask);
                                    }
                                }
                            }
                        }
                    }

                    // Block
                    int startRow = (r / 3) * 3;
                    int startCol = (c / 3) * 3;

                    for (int br = 0; br < 3; br++)
                    {
                        for (int bc = 0; bc < 3; bc++)
                        {
                            int realR = startRow + br;
                            int realC = startCol + bc;

                            // Makes sure we check only diagonal and offsets as row and column have already been checked.
                            if (realR <= r || realC == c)
                            {
                                continue;
                            }

                            int mask2 = candidates[realR, realC];
                            int possiblePairCandidate = mask1 & mask2;

                            // Needs at least two of the same candidates.
                            if (GetPopCount(possiblePairCandidate) >= 2)
                            {
                                int noiseMask = 0;

                                for (int br2 = 0; br2 < 3; br2++)
                                {
                                    for (int bc2 = 0; bc2 < 3; bc2++)
                                    {
                                        int noiseR = startRow + br2;
                                        int noiseC = startCol + bc2;

                                        if ((noiseC == c && noiseR == r) || (noiseC == realC && noiseR == realR))
                                        {
                                            continue;
                                        }

                                        noiseMask |= candidates[br2 + startRow, bc2 + startCol];
                                    }
                                }

                                // Found hidden pair.
                                if (GetPopCount(possiblePairCandidate & ~noiseMask) == 2)
                                {
                                    if (GetPopCount(mask1) > 2 || GetPopCount(mask2) > 2)
                                    {
                                        List<Point> sources = new List<Point> { new Point(c, r), new Point(realC, realR) };
                                        int pairMask = possiblePairCandidate & ~noiseMask;
                                        if (IsHintUseful(sources, ~pairMask & ALL))
                                        {
                                            return (sources, pairMask);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Strategy: Finds three cells in a unit containing a subset of the same three candidates.
        /// Examples: (ABC, AC, AB), (ABC, ABC, ABC), (AB, AC, BC).
        /// </summary>
        public (List<Point>, List<Point>, int)? FindNakedTriple()
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {

                    int mask1 = candidates[r, c];

                    // Can only find a naked triple if cell has 2 or 3 candidates.
                    if (GetPopCount(mask1) < 2 || GetPopCount(mask1) > 3)
                    {
                        continue;
                    }

                    // Row
                    for (int i = c + 1; i < 9; i++)
                    {
                        int mask2 = candidates[r, i];

                        if ((mask1 & mask2) == 0)
                        {
                            continue;
                        }

                        if (GetPopCount(mask2) < 2 || GetPopCount(mask2) > 3)
                        {
                            continue;
                        }

                        int union12 = mask2 | mask1;

                        if (GetPopCount(union12) > 3) { continue; }

                        for (int j = i + 1; j < 9; j++)
                        {
                            int mask3 = candidates[r, j];
                            if (GetPopCount(mask3) < 2 || GetPopCount(mask3) > 3)
                            {
                                continue;
                            }

                            int finalMask = union12 | mask3;

                            if (GetPopCount(finalMask) == 3)
                            {
                                List<Point> sources = new List<Point> { new Point(c, r), new Point(i, r), new Point(j, r) };
                                List<Point> targets = GetNakedTargets(sources, finalMask, "row");

                                // Only found useful naked triple if it affects the board.
                                if (targets.Count > 0 && IsHintUseful(targets, finalMask))
                                {
                                    return (sources, targets, finalMask);
                                }
                            }
                        }
                    }

                    // Col
                    for (int i = r + 1; i < 9; i++)
                    {
                        int mask2 = candidates[i, c];

                        if (GetPopCount(mask2) < 2 || GetPopCount(mask2) > 3)
                        {
                            continue;
                        }

                        int union12 = mask2 | mask1;

                        if (GetPopCount(union12) > 3) { continue; }

                        for (int j = i + 1; j < 9; j++)
                        {
                            int mask3 = candidates[j, c];
                            if (GetPopCount(mask3) < 2 || GetPopCount(mask3) > 3)
                            {
                                continue;
                            }

                            int finalMask = union12 | mask3;

                            if (GetPopCount(finalMask) == 3)
                            {
                                List<Point> sources = new List<Point> { new Point(c, r), new Point(c, i), new Point(c, j) };
                                List<Point> targets = GetNakedTargets(sources, finalMask, "col");

                                if (targets.Count > 0 && IsHintUseful(targets, finalMask))
                                {
                                    return (sources, targets, finalMask);
                                }
                            }
                        }
                    }

                    // Block, we check only once for each block
                    if (r % 3 == 0 && c % 3 == 0)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            int r1 = r + i / 3;
                            int c1 = c + i % 3;

                            mask1 = candidates[r1, c1];

                            if (GetPopCount(mask1) < 2 || GetPopCount(mask1) > 3)
                            {
                                continue;
                            }

                            for (int j = i + 1; j < 8; j++)
                            {
                                int r2 = r + (j / 3);
                                int c2 = c + (j % 3);

                                int mask2 = candidates[r2, c2];

                                if (GetPopCount(mask2) < 2 || GetPopCount(mask2) > 3)
                                {
                                    continue;
                                }

                                int union12 = mask1 | mask2;

                                if (GetPopCount(union12) > 3)
                                {
                                    continue;
                                }

                                for (int k = j + 1; k < 9; k++)
                                {
                                    int r3 = r + (k / 3);
                                    int c3 = c + (k % 3);
                                    int mask3 = candidates[r3, c3];

                                    if (GetPopCount(mask3) < 2 || GetPopCount(mask3) > 3) { continue; }

                                    int finalMask = union12 | mask3;

                                    if (GetPopCount(finalMask) == 3)
                                    {
                                        List<Point> sources = new List<Point> { new Point(c1, r1), new Point(c2, r2), new Point(c3, r3) };
                                        List<Point> targets = GetNakedTargets(sources, finalMask, "block");

                                        if (targets.Count > 0 && IsHintUseful(targets, finalMask))
                                        {
                                            return (sources, targets, finalMask);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds three candidates that appear ONLY in three specific cells within a unit.
        /// </summary>
        public (List<Point>, int)? FindHiddenTriple()
        {

            // Row
            for (int r = 0; r < 9; r++)
            {

                List<Point> rowPoints = new List<Point>();

                for (int c = 0; c < 9; c++)
                {
                    rowPoints.Add(new Point(c, r));
                }
                var res = FindHiddenTripleInRegion(rowPoints);
                if (res != null) { return res; }
            }

            // Col
            for (int c = 0; c < 9; c++)
            {
                List<Point> colPoints = new List<Point>();

                for (int r = 0; r < 9; r++)
                {
                    colPoints.Add(new Point(c, r));
                }

                var res = FindHiddenTripleInRegion(colPoints);
                if (res != null) { return res; }
            }

            // Block
            for (int b = 0; b < 9; b++)
            {
                List<Point> points = new List<Point>();
                int startRow = (b / 3) * 3;
                int startCol = (b % 3) * 3;

                for (int r = startRow; r < startRow + 3; r++)
                {
                    for (int c = startCol; c < startCol + 3; c++)
                    {
                        points.Add(new Point(c, r));
                    }
                }

                var res = FindHiddenTripleInRegion(points);
                if (res != null) { return res; }
            }
            return null;
        }

        // We take the view from the values instead of the cells, as this makes the code a lot less complex.
        private (List<Point>, int)? FindHiddenTripleInRegion(List<Point> cells)
        {
            // We make a new mask that shows the cell of a value, instead of the values in a location.
            int[] candidatePos = new int[9];

            for (int i = 0; i < cells.Count; i++)
            {
                Point p = cells[i];

                int mask = candidates[p.Y, p.X];

                for (int v = 1; v <= 9; v++)
                {
                    // is number v a candidate in this cell i's mask? If yes add to i to mask of locations for v.
                    if ((mask & (1 << (v - 1))) != 0)
                    {
                        candidatePos[v - 1] |= (1 << i);
                    }
                }
            }

            // Find 3 values that have 2 or 3 cells. Check whether they share 3 cells.
            for (int i = 0; i < 7; i++)
            {
                int posMaskI = candidatePos[i];

                // A value has to be in 2 or 3 cells.
                if (GetPopCount(posMaskI) < 2 || GetPopCount(posMaskI) > 3)
                {
                    continue;
                }

                for (int j = i + 1; j < 8; j++)
                {
                    int posMaskJ = candidatePos[j];
                    if (GetPopCount(posMaskJ) < 2 || GetPopCount(posMaskJ) > 3) { continue; }

                    int unionIJ = posMaskJ | posMaskI;

                    // Need to find a triple.
                    if (GetPopCount(unionIJ) > 3) { continue; }

                    for (int k = j + 1; k < 9; k++)
                    {
                        int posMaskK = candidatePos[k];
                        if (GetPopCount(posMaskK) < 2 || GetPopCount(posMaskK) > 3)
                        {
                            continue;
                        }

                        int unionIJK = unionIJ | posMaskK;

                        // Need exactly 3 shared cells for triple.
                        if (GetPopCount(unionIJK) == 3)
                        {
                            List<Point> points = new List<Point>();

                            // Turn location mask into acutal points.
                            for (int l = 0; l < 9; l++)
                            {
                                if ((unionIJK & (1 << l)) != 0)
                                {
                                    points.Add(cells[l]);
                                }
                            }

                            int tripleMask = (1 << i) | (1 << j) | (1 << k);

                            // Need a hidden triple that can eliminate candidates.
                            bool isUseful = false;
                            foreach (Point p in points)
                            {
                                int currentCandidates = candidates[p.Y, p.X];

                                if ((currentCandidates & ~tripleMask) != 0)
                                {
                                    isUseful = true;
                                    break;
                                }
                            }

                            if (isUseful && IsHintUseful(points, ~tripleMask & ALL))
                            {
                                return (points, tripleMask);
                            }

                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a a candidate that is restricted to a single row/column within a block.
        /// It can be removed from the rest of that row/column outside the block.
        /// </summary>
        public (List<Point>, List<Point>, int)? FindPointingPairOrTriple()
        {
            int[] rowMasks = { 7, 56, 448 }; // 000000111, 000111000, 111000000
            int[] colMasks = { 73, 146, 292 }; // 001001001, 010010010, 100100100
            int[] candidatePos = new int[9];

            for (int b = 0; b < 9; b++)
            {
                int startRow = (b / 3) * 3;
                int startCol = (b % 3) * 3;


                // Check each value in a block, add it to a location mask.
                for (int v = 0; v < 9; v++)
                {
                    for (int r = startRow; r < startRow + 3; r++)
                    {
                        for (int c = startCol; c < startCol + 3; c++)
                        {
                            if ((candidates[r, c] & (1 << v)) != 0)
                            {
                                int cellIndex = (r - startRow) * 3 + (c - startCol);

                                candidatePos[v] |= (1 << cellIndex);
                            }
                        }
                    }
                }

                // Check values in mask (row and column of the selected cell), if the row or column has multiple of the same values -> continue the program
                for (int v = 0; v < 9; v++)
                {
                    int mask = candidatePos[v];

                    if (GetPopCount(mask) < 2 || GetPopCount(mask) > 3)
                    {
                        continue;
                    }

                    int valueMask = 1 << v;
                    // Rows
                    for (int rowInBlock = 0; rowInBlock < 3; rowInBlock++)
                    {

                        // Check if the candidate-row-mask matches this row inside the 3x3-box, and if true the candidate appears only in this row in 3x3-box
                        if ((mask & rowMasks[rowInBlock]) == mask)
                        {
                            int realRow = startRow + rowInBlock;

                            List<Point> targets = new List<Point>();
                            for (int c = 0; c < 9; c++)
                            {

                                // Skip columns belonging to the current box because we only want to eliminate the candidates outside of the box along this row
                                if (c >= startCol && c < startCol + 3)
                                {
                                    continue;
                                }

                                // If another cell in this row has the same candidate we can eliminate it
                                if ((candidates[realRow, c] & valueMask) != 0)
                                {
                                    targets.Add(new Point(c, realRow));
                                }

                                if (targets.Count > 0 && IsHintUseful(targets, valueMask))
                                {
                                    return (GetPointsFromMask(mask, startRow, startCol), targets, valueMask);
                                }
                            }
                        }
                    }

                    // Column
                    for (int colInBlock = 0; colInBlock < 3; colInBlock++)
                    {
                        // Check if candidate-column-mask matches this column inside the 3x3-box, and if true the candidate only appears in this column in the box
                        if ((mask & colMasks[colInBlock]) == mask)
                        {
                            int realCol = startCol + colInBlock;

                            List<Point> targets = new List<Point>();
                            for (int r = 0; r < 9; r++)
                            {
                                // Skip columns belonging to the current box to because we want to eliminate the candidates outside of the box in this column
                                if (r >= startRow && r < startRow + 3)
                                {
                                    continue;
                                }

                                // If another cell in this columnn has the same candidate we can eliminate it
                                if ((candidates[r, realCol] & valueMask) != 0)
                                {
                                    targets.Add(new Point(realCol, r));
                                }
                            }

                            if (targets.Count > 0 && IsHintUseful(targets, valueMask))
                            {
                                return (GetPointsFromMask(mask, startRow, startCol), targets, valueMask);
                            }
                        }
                    }
                }
                Array.Clear(candidatePos);
            }
            return null;
        }

        /// <summary>
        /// Finds a pattern where a candidate appears exactly twice in two different rows, 
        /// and these occurrences are in the same two columns. This eliminates the candidate from the rest of those columns.
        /// </summary>
        public (List<Point>, List<Point>, int)? FindXWing()
        {
            int[,] rowMask = new int[9, 9];
            int[,] colMask = new int[9, 9];

            // Build row masks
            for (int v = 0; v < 9; v++)
            {
                int valueMask = 1 << v;

                for (int r = 0; r < 9; r++)
                {
                    int mask = 0;

                    for (int c = 0; c < 9; c++)
                    {
                        // Check if candidate v is present in cell, if true set bit c in the mask
                        if ((candidates[r, c] & valueMask) != 0)
                        {
                            mask |= (1 << c);
                        }
                    }

                    rowMask[v, r] = mask;
                }
            }

            // Check for X-Wings in rows
            for (int v = 0; v < 9; v++)
            {
                int valueMask = 1 << v;
                for (int r1 = 0; r1 < 8; r1++)
                {
                    int mask1 = rowMask[v, r1];
                    // Candidate must appear exactly twice in this row
                    if (GetPopCount(mask1) != 2)
                        continue;


                    for (int r2 = r1 + 1; r2 < 9; r2++)
                    {
                        int mask2 = rowMask[v, r2];
                        // Check if the candidate appears in the same columns in the second row
                        if (mask1 != mask2)
                            continue;

                        // Extract the two columns where the candidate appears
                        List<int> cols = new List<int>();
                        for (int c = 0; c < 9; c++)
                        {
                            if ((mask1 & (1 << c)) != 0)
                                cols.Add(c);
                        }

                        int c1 = cols[0];
                        int c2 = cols[1];

                        // Check for eliminations in these columns
                        List<Point> targets = new List<Point>();
                        for (int r = 0; r < 9; r++)
                        {
                            if (r == r1 || r == r2)
                                continue;

                            if ((candidates[r, c1] & valueMask) != 0)
                            {
                                targets.Add(new Point(c1, r));
                            }

                            if ((candidates[r, c2] & valueMask) != 0)
                            {
                                targets.Add(new Point(c2, r));
                            }
                        }

                        if (targets.Count > 0 && IsHintUseful(targets, valueMask))
                        {
                            List<Point> sources = new List<Point> {
                                new Point(c1, r1),
                                new Point(c2, r1),
                                new Point(c1, r2),
                                new Point(c2, r2)
                            };
                            return (sources, targets, valueMask);
                        }
                    }
                }
            }

            // Check for X-Wings in columns
            for (int v = 0; v < 9; v++)
            {
                int valueMask = 1 << v;
                for (int c = 0; c < 9; c++)
                {
                    int mask = 0;

                    for (int r = 0; r < 9; r++)
                    {
                        if ((candidates[r, c] & valueMask) != 0)
                        {
                            mask |= (1 << r);
                        }
                    }

                    colMask[v, c] = mask;
                }
            }

            // Check for X-Wings in columns
            for (int v = 0; v < 9; v++)
            {
                int valueMask = 1 << v;
                for (int c1 = 0; c1 < 8; c1++)
                {
                    // Get the mask for candidate v in column c1
                    int mask1 = colMask[v, c1];
                    if (GetPopCount(mask1) != 2)
                        continue;

                    for (int c2 = c1 + 1; c2 < 9; c2++)
                    {
                        // Get the mask for candidate v in column c2
                        int mask2 = colMask[v, c2];
                        if (mask1 != mask2)
                            continue;

                        List<int> rows = new List<int>();
                        for (int r = 0; r < 9; r++)
                        {
                            // Check if candidate v is present in cell, if true set bit r in the mask
                            if ((mask1 & (1 << r)) != 0)
                                rows.Add(r);
                        }

                        int r1 = rows[0];
                        int r2 = rows[1];

                        // Check for eliminations
                        List<Point> targets = new List<Point>();

                        // Check other columns for candidate presence
                        for (int c = 0; c < 9; c++)
                        {
                            if (c == c1 || c == c2)
                                continue;

                            // Check if candidate v is present in either of the two rows in this column
                            if ((candidates[r1, c] & valueMask) != 0)
                            {
                                targets.Add(new Point(c, r1));
                            }

                            if ((candidates[r2, c] & valueMask) != 0)
                            {
                                targets.Add(new Point(c, r2));
                            }
                        }

                        // If eliminations are possible, return the points and mask
                        if (targets.Count > 0 && IsHintUseful(targets, valueMask))
                        {
                            List<Point> sources = new List<Point>
                            {
                                new Point(c1, r1),
                                new Point(c2, r1),
                                new Point(c1, r2),
                                new Point(c2, r2)
                            };

                            return (sources, targets, valueMask);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a Pivot cell (AB) and two Pincer cells (AC and BC).
        /// The Pivot must see both Pincers. The candidate C can be eliminated from any cell that sees both Pincers.
        /// </summary>
        public (List<Point>, List<Point>, int)? FindYWing()
        {
            List<(Point p, int mask)> twoCandidateCells = new List<(Point, int)>();

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    // Check for two-candidate cells
                    int mask = candidates[r, c];
                    if (GetPopCount(mask) == 2)
                        twoCandidateCells.Add((new Point(c, r), mask));
                }
            }


            // Try each cell as pivot
            for (int i = 0; i < twoCandidateCells.Count; i++)
            {
                // Get pivot cell and its candidates
                var (pivotPoint, pivotMask) = twoCandidateCells[i];
                int[] pivotDigits = GetDigitsFromMask(pivotMask);
                int a = pivotDigits[0];
                int b = pivotDigits[1];

                // Find pincer A (shares digit a)
                for (int j = 0; j < twoCandidateCells.Count; j++)
                {
                    if (i == j)
                        continue;

                    var (pA, maskA) = twoCandidateCells[j];

                    if (!CanSee(pivotPoint, pA))
                    {
                        continue;
                    }

                    if ((maskA & (1 << a)) == 0)
                    {
                        continue;
                    }

                    int c = GetOtherDigit(maskA, a);

                    // If c is equal to be, we have found a naked pair, not a Y-wing
                    if (c == b)
                    {
                        continue;
                    }

                    // Find pincer B (shares digit b and c)
                    for (int k = 0; k < twoCandidateCells.Count; k++)
                    {
                        if (k == i || k == j)
                        {
                            continue;
                        }

                        var (pB, maskB) = twoCandidateCells[k];

                        if (!CanSee(pivotPoint, pB))
                        {
                            continue;
                        }

                        if (CanSee(pA, pB))
                        {
                            continue;
                        }

                        if ((maskB & (1 << b)) == 0)
                        {
                            continue;
                        }

                        if ((maskB & (1 << c)) == 0)
                        {
                            continue;
                        }

                        // Check for elimination targets
                        List<Point> targets = new List<Point>();

                        for (int r = 0; r < 9; r++)
                        {
                            for (int col = 0; col < 9; col++)
                            {
                                Point p = new Point(col, r);

                                if (p == pivotPoint || p == pA || p == pB)
                                {
                                    continue;
                                }

                                // Skip pivot and pincers
                                if (CanSee(p, pA) && CanSee(p, pB))
                                {
                                    if ((candidates[r, col] & (1 << c)) != 0)
                                        targets.Add(p);
                                }
                            }
                        }

                        // If we found targets, return the pincers and the digit to eliminate
                        if (targets.Count > 0 && IsHintUseful(targets, ~(1 << c) & ALL))
                        {
                            return (new List<Point> { pivotPoint, pA, pB }, targets, 1 << c);
                        }

                    }
                }
            }
            return null;
        }

        private int[] GetDigitsFromMask(int mask)
        {
            List<int> digits = new List<int>();

            for (int d = 0; d < 9; d++)
            {
                // Check if digit d is in the mask
                if ((mask & (1 << d)) != 0)
                    digits.Add(d);
            }

            return digits.ToArray();
        }

        private int GetOtherDigit(int mask, int knownDigit)
        {
            for (int d = 0; d < 9; d++)
            {
                // Check if digit d is in the mask and is not the known digit
                if (d != knownDigit && (mask & (1 << d)) != 0)
                    return d;
            }
            return 0;
        }

        private bool CanSee(Point a, Point b)
        {
            // Cells can see each other if they share the same row, column, or block
            return a.X == b.X || a.Y == b.Y || GetBlockNumber(a.X, a.Y) == GetBlockNumber(b.X, b.Y);
        }

        /// <summary>
        /// In three different rows (or columns), a candidate appears exactly 2 or 3 times, 
        /// and these occurrences align within the same set of 3 columns (or rows).
        /// </summary>
        public (List<Point>, List<Point>, int)? FindSwordfish()
        {
            int[,] rowMask = new int[9, 9];
            int[,] colMask = new int[9, 9];

            // Build row masks and column masks
            for (int v = 0; v < 9; v++)
            {
                for (int r = 0; r < 9; r++)
                {
                    int mask = 0;
                    for (int c = 0; c < 9; c++)
                    {
                        if ((candidates[r, c] & (1 << v)) != 0)
                            mask |= (1 << c);
                    }
                    rowMask[v, r] = mask;
                }

                for (int c = 0; c < 9; c++)
                {
                    int mask = 0;
                    for (int r = 0; r < 9; r++)
                    {
                        if ((candidates[r, c] & (1 << v)) != 0)
                            mask |= (1 << r);
                    }
                    colMask[v, c] = mask;
                }
            }

            // Row-based Swordfish
            for (int v = 0; v < 9; v++)
            {
                int valueMask = 1 << v;

                List<int> goodRows = new List<int>();

                for (int r = 0; r < 9; r++)
                {
                    // Check if candidate v appears 2 or 3 times in row r
                    int pc = GetPopCount(rowMask[v, r]);
                    if (pc == 2 || pc == 3)
                        goodRows.Add(r);
                }

                for (int i = 0; i < goodRows.Count; i++)
                {
                    for (int j = i + 1; j < goodRows.Count; j++)
                    {
                        for (int k = j + 1; k < goodRows.Count; k++)
                        {
                            int r1 = goodRows[i], r2 = goodRows[j], r3 = goodRows[k];
                            int m1 = rowMask[v, r1], m2 = rowMask[v, r2], m3 = rowMask[v, r3];

                            int union = m1 | m2 | m3;

                            // Check if union has 2 or 3 columns
                            if (GetPopCount(union) < 2 || GetPopCount(union) > 3) { continue; }

                            List<Point> targets = new List<Point>();

                            for (int c = 0; c < 9; c++)
                            {
                                if ((union & (1 << c)) != 0)
                                {
                                    for (int r = 0; r < 9; r++)
                                    {
                                        if (r == r1 || r == r2 || r == r3)
                                        { continue; }

                                        if ((candidates[r, c] & valueMask) != 0) { targets.Add(new Point(c, r)); }
                                    }
                                }
                            }

                            if (targets.Count > 0 && IsHintUseful(targets, valueMask))
                            {
                                List<Point> sources = new List<Point>();
                                for (int c = 0; c < 9; c++)
                                {
                                    // Check if candidate v appears in this column in the swordfish rows, if yes add the points
                                    if ((union & (1 << c)) != 0)
                                    {
                                        if ((candidates[r1, c] & (1 << v)) != 0) { sources.Add(new Point(c, r1)); }
                                        if ((candidates[r2, c] & (1 << v)) != 0) { sources.Add(new Point(c, r2)); }
                                        if ((candidates[r3, c] & (1 << v)) != 0) { sources.Add(new Point(c, r3)); }
                                    }
                                }

                                return (sources, targets, 1 << v);
                            }
                        }
                    }
                }
            }

            // Column-based Swordfish
            for (int v = 0; v < 9; v++)
            {
                int valueMask = 1 << v;
                List<int> goodCols = new List<int>();

                for (int c = 0; c < 9; c++)
                {
                    int pc = GetPopCount(colMask[v, c]);
                    if (pc == 2 || pc == 3)
                        goodCols.Add(c);
                }

                for (int i = 0; i < goodCols.Count; i++)
                {
                    for (int j = i + 1; j < goodCols.Count; j++)
                    {
                        for (int k = j + 1; k < goodCols.Count; k++)
                        {
                            int c1 = goodCols[i], c2 = goodCols[j], c3 = goodCols[k];

                            int m1 = colMask[v, c1], m2 = colMask[v, c2], m3 = colMask[v, c3];

                            int union = m1 | m2 | m3;

                            // Check if union has 2 or 3 columns
                            if (GetPopCount(union) < 2 || GetPopCount(union) > 3) { continue; }

                            List<Point> targets = new List<Point>();

                            for (int r = 0; r < 9; r++)
                            {
                                if ((union & (1 << r)) != 0)
                                {
                                    for (int c = 0; c < 9; c++)
                                    {
                                        if (c == c1 || c == c2 || c == c3)
                                        { continue; }

                                        if ((candidates[r, c] & valueMask) != 0) { targets.Add(new Point(c, r)); }
                                    }
                                }
                            }

                            if (targets.Count > 0 && IsHintUseful(targets, valueMask))
                            {
                                List<Point> sources = new List<Point>();
                                for (int r = 0; r < 9; r++)
                                {
                                    if ((union & (1 << r)) != 0)
                                    {
                                        if ((candidates[r, c1] & (1 << v)) != 0) { sources.Add(new Point(c1, r)); }
                                        if ((candidates[r, c2] & (1 << v)) != 0) { sources.Add(new Point(c2, r)); }
                                        if ((candidates[r, c3] & (1 << v)) != 0) { sources.Add(new Point(c3, r)); }
                                    }
                                }

                                return (sources, targets, 1 << v);
                            }
                        }
                    }
                }
            }

            return null;
        }

        private List<Point> GetPointsFromMask(int mask, int startRow, int startCol)
        {
            List<Point> points = new List<Point>();

            for (int i = 0; i < 9; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    int r = startRow + (i / 3);
                    int c = startCol + (i % 3);
                    points.Add(new Point(c, r));
                }
            }
            return points;
        }

        /// <summary>
        /// Applies a Naked Single or Hidden Single to the grid.
        /// Propagates the change by removing this value from the candidates of its neighbors (Row/Col/Block).
        /// </summary>
        public bool ApplyNakedOrHiddenSingle(Point p, int mask)
        {
            cells[p.Y, p.X] = mask;
            candidates[p.Y, p.X] = 0;

            // Update global masks
            rows[p.Y] |= mask;
            cols[p.X] |= mask;
            blocks[GetBlockNumber(p.Y, p.X)] |= mask;

            // Update row neighbours
            for (int c = 0; c < 9; c++)
            {
                candidates[p.Y, c] &= ~mask;
            }

            // Update column neighbours
            for (int r = 0; r < 9; r++)
            {
                candidates[r, p.X] &= ~mask;
            }

            // Update block candidates
            int startRow = (p.Y / 3) * 3;
            int startCol = (p.X / 3) * 3;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    candidates[startRow + r, startCol + c] &= ~mask;
                }
            }
            return true;
        }

        private bool EliminateCandidates(List<Point> points, int maskToRemove)
        {
            bool changed = false;
            foreach (Point p in points)
            {
                int original = candidates[p.Y, p.X];
                int modified = original & ~maskToRemove;

                if (original != modified)
                {
                    candidates[p.Y, p.X] = modified;
                    changed = true;
                }
            }
            return changed;
        }

        /// <summary>
        /// Applies a Naked Pair or Triple hint.
        /// Eliminates the pair/triple candidates from all *other* cells in the unit.
        /// </summary>
        public bool ApplyNakedPairOrTriple(List<Point> targets, int mask)
        {
            return EliminateCandidates(targets, mask);
        }

        /// <summary>
        /// Applies a Hidden Pair or Triple hint.
        /// Removes *all other* candidates from the cells that contain the hidden tuple.
        /// </summary>
        public bool ApplyHiddenPairOrTriple(List<Point> targets, int mask)
        {
            return EliminateCandidates(targets, ~mask);
        }

        /// <summary>
        /// Applies a Pointing Pair or Triple hint.
        /// Eliminates the candidate from the rest of the unit (Row or Col) outside the block.
        /// </summary>
        public bool ApplyPointingPairOrTriple(List<Point> targets, int mask)
        {
            return EliminateCandidates(targets, mask);
        }

        /// <summary>
        /// Applies an X-Wing hint.
        /// Eliminates the candidate from the target cells in the intersecting columns/rows.
        /// </summary>
        public bool ApplyXWing(List<Point> targets, int mask)
        {
            return EliminateCandidates(targets, mask);
        }

        /// <summary>
        /// Applies a Y-Wing hint.
        /// Eliminates the common candidate from cells that see both pincers.
        /// </summary>
        public bool ApplyYWing(List<Point> targets, int mask)
        {
            return EliminateCandidates(targets, mask);
        }

        /// <summary>
        /// Applies a Swordfish hint.
        /// Eliminates the candidate from the target cells in the intersecting grid lines.
        /// </summary>
        public bool ApplySwordfish(List<Point> targets, int mask)
        {
            return EliminateCandidates(targets, mask);
        }

    }
}