using Sudoku_Namespace.Algorithms;
using System.Drawing;

namespace SudokuTests
{
    [TestClass]
    public class BitMaskSolverTest
    {

        private int[,] cells;

        private BitmaskSolver solver;

        [TestInitialize]
        public void Setup()
        {
            cells = new int[9, 9];
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    cells[r, c] = 0;
                }
            }
            solver = new BitmaskSolver(cells);
        }

        [TestMethod]
        public void BitWordToNumber_AssertRightValue()
        {
            int bitWord = 1 << (5 - 1); // represents number 5
            Assert.IsTrue(solver.BitToNumber(bitWord) == 5);
        }

        [TestMethod]
        public void InitializeBitmaskGrids_ValidBoardTrue()
        {
            cells[0, 0] = 5;
            cells[0, 1] = 3;
            bool result = solver.InitializeBitmaskGrids();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void InitializeBitmaskGrids_ValidBoardFalse()
        {
            cells[0, 0] = 1;
            cells[0, 1] = 1;
            bool result = solver.InitializeBitmaskGrids();
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Solve_BoardTrue()
        {
            cells[0, 0] = 5;
            solver.InitializeBitmaskGrids();
            int[,] result = solver.Solve();
            Assert.IsNotNull(result);

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    Assert.AreNotEqual(0, result[r, c]);
                }
            }
        }

        [TestMethod]
        public void NakedSingle_FoundTrue()
        {
            for (int r = 0; r < 9; r++)
            {
                cells[r, 0] = r + 1;
                cells[0, 0] = 0;
            }
            solver.InitializeBitmaskGrids();
            (Point, int)? p = solver.FindNakedSingle();

            Assert.IsNotNull(p);
            Assert.AreEqual(new Point(0, 0), p.Value.Item1);
        }

        [TestMethod]
        public void NakedSingle_FoundFalse()
        {
            solver.InitializeBitmaskGrids();
            (Point, int)? p = solver.FindNakedSingle();

            Assert.IsNull(p);
        }

        [TestMethod]
        public void HiddenSingle_FoundTrue()
        {
            cells[1, 3] = 1;
            cells[2, 6] = 1;
            cells[3, 1] = 1;
            cells[6, 2] = 1;

            // 000 000 000
            // 000 100 000
            // 000 000 100
            // 010 000 000
            // 000 000 000
            // 000 000 000
            // 001 000 000
            // 000 000 000
            // 000 000 000

            // (0,0): has no '1' in its row, col or block.
            // (0,1)..(0,8): Have '1' removed as there are already 1s the blocks.

            solver = new BitmaskSolver(cells);
            bool initSuccess = solver.InitializeBitmaskGrids();

            Assert.IsTrue(initSuccess);

            var result = solver.FindHiddenSingle();

            Assert.IsNotNull(result);
            Assert.AreEqual(new Point(0, 0), result.Value.Item1);
            Assert.AreEqual(1, solver.BitToNumber(result.Value.Item2));
        }

        [TestMethod]
        public void NakedPair_Row_FoundFalse()
        {

            // Only values 1 and 2 are available in row 0.
            // As no useful improvement, there is no naked pair found. 

            for (int c = 2; c < 9; c++)
            {
                cells[0, c] = c + 1;
            }

            solver.InitializeBitmaskGrids();
            var result = solver.FindNakedPair();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void NakedPair_Row_FoundTrue()
        {
            // Only values 1, 2 and 3 are available in row 0.

            for (int c = 2; c < 9; c++)
            {
                if (c == 2) continue;
                cells[0, c] = c + 1;
            }

            // Disable candidate 3 from col 0 and 1.
            cells[8, 0] = 3;
            cells[5, 1] = 3;

            solver.InitializeBitmaskGrids();
            var result = solver.FindNakedPair();

            Assert.IsNotNull(result);

            List<Point> sources = result.Value.Item1;
            Assert.IsTrue(sources.Contains(new Point(0, 0)), $"{sources[0]}");
            Assert.IsTrue(sources.Contains(new Point(1, 0)), $"{sources[1]}");

            Assert.IsTrue(result.Value.Item2.Contains(new Point(2, 0)));

            // 000 000 011
            Assert.AreEqual(3, result.Value.Item3);
        }

        [TestMethod]
        public void NakedPair_Block_FoundFalse()
        {
            cells[0, 1] = 3;
            cells[0, 2] = 4;
            cells[1, 0] = 5;
            cells[1, 2] = 6;
            cells[2, 0] = 7;
            cells[2, 1] = 8;
            cells[2, 2] = 9;

            solver.InitializeBitmaskGrids();
            var result = solver.FindNakedPair();

            Assert.IsNull(result);

        }

        [TestMethod]
        public void NakedPair_Block_FoundTrue()
        {
            // cells (0,0) and (1,1) only have candidates 1 and 2. cell (2,2) has candidates 1, 2 and 9.
            cells[0, 1] = 3;
            cells[0, 2] = 4;
            cells[1, 0] = 5;
            cells[1, 2] = 6;
            cells[2, 0] = 7;
            cells[2, 1] = 8;

            cells[0, 5] = 9;
            cells[1, 6] = 9;

            solver.InitializeBitmaskGrids();
            var result = solver.FindNakedPair();

            Assert.IsNotNull(result);

            Assert.IsTrue(result.Value.Item1.Contains(new Point(0, 0)));
            Assert.IsTrue(result.Value.Item1.Contains(new Point(1, 1)));

            Assert.IsTrue(result.Value.Item2.Contains(new Point(2, 2)));
            // 000 000 011
            Assert.AreEqual(3, result.Value.Item3);
        }

        [TestMethod]
        public void HiddenPair_Block_FoundTrue()
        {
            // Row 0 now has 1 & 2 used.
            cells[0, 8] = 1;
            cells[0, 7] = 2;

            // Row 1 now has 1 & 2 used.
            cells[1, 5] = 1;
            cells[1, 4] = 2;

            // Remove candidates 1 and 2 from the remaining cell in Row 2 (2,2).
            cells[2, 2] = 3;

            solver.InitializeBitmaskGrids();
            var result = solver.FindHiddenPair();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Value.Item1.Contains(new Point(0, 2)));
            Assert.IsTrue(result.Value.Item1.Contains(new Point(1, 2)));
            Assert.AreEqual(3, result.Value.Item2);
        }

        [TestMethod]
        public void HiddenPair_Block_FoundFalse()
        {
            // Row 0 now has 1 & 2 used.
            cells[0, 8] = 1;
            cells[0, 7] = 2;

            // Row 1 now has 1 & 2 used.
            cells[1, 5] = 1;
            cells[1, 4] = 2;

            // Remove candidates from (2,0) and (2,1)
            cells[8, 0] = 9;
            cells[8, 1] = 9;
            cells[2, 2] = 9;
            cells[2, 3] = 3;
            cells[2, 4] = 4;
            cells[2, 5] = 5;
            cells[2, 6] = 6;
            cells[2, 7] = 7;
            cells[2, 8] = 8;

            solver.InitializeBitmaskGrids();
            var result = solver.FindHiddenPair();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void HiddenPair_Row_FoundTrue()
        {

            // 0xx 345 678
            // 000 000 000
            // 000 000 000
            // 000 000 000
            // 000 000 000
            // 000 000 000
            // 000 000 000
            // 100 000 000
            // 200 000 000

            for (int c = 3; c < 9; c++)
            {
                cells[0, c] = c;
            }

            cells[7, 0] = 1;
            cells[8, 0] = 2;

            solver.InitializeBitmaskGrids();
            var result = solver.FindHiddenPair();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Value.Item1.Contains(new Point(1, 0)));
            Assert.IsTrue(result.Value.Item1.Contains(new Point(2, 0)));
            Assert.AreEqual(3, result.Value.Item2);
        }

        [TestMethod]
        public void NakedTriple_Row_FoundTrue()
        {
            // Setup a Naked Triple {1, 2, 3} in Row 0 at cols 0, 1, 2.
            // Target is (0, 8) which contains candidate 1 to be eliminated.

            // Block other candidates (4-9) in Row 0, except for cell (0,8)
            cells[0, 3] = 4; cells[0, 4] = 5; cells[0, 5] = 6; cells[0, 6] = 7; cells[0, 7] = 8;

            // Remove candidate 9 from (0,0), (0,1) and (0,2)
            cells[2, 2] = 9;

            // Refine (0,0) to {1, 2}, (0,1) to {2, 3} and (0,2) to {1, 3}
            cells[3, 0] = 3;
            cells[3, 1] = 1;
            cells[8, 2] = 2;



            Assert.IsTrue(solver.InitializeBitmaskGrids());

            var result = solver.FindNakedTriple();

            Assert.IsNotNull(result);

            // 000 000 111
            Assert.AreEqual(7, result.Value.Item3);

            List<Point> points = result.Value.Item1;
            Assert.IsTrue(points.Contains(new Point(0, 0)));
            Assert.IsTrue(points.Contains(new Point(1, 0)));
            Assert.IsTrue(points.Contains(new Point(2, 0)));

            Assert.IsTrue(result.Value.Item2.Contains(new Point(8, 0)));
            List<Point> targets = result.Value.Item2;
            Assert.IsTrue(targets.Contains(new Point(8, 0)));
        }

        [TestMethod]
        public void NakedTriple_Row_FoundFalse()
        {
            // Setup a Naked Triple {1, 2, 3} in Row 0 at cols 0, 1, 2.
            // Target is (0, 8) which contains candidate 1 to be eliminated.

            // Block other candidates (4-9) in Row 0, except for cell (0,8)
            cells[0, 3] = 4; cells[0, 4] = 5; cells[0, 5] = 6; cells[0, 6] = 7; cells[0, 7] = 8;

            // Remove candidate 9 from (0,0), (0,1) and (0,2)
            cells[2, 2] = 9;

            // (0,0) to {1, 2}, (0,1) to {2, 3} and (0,2) to {1, 3}
            cells[3, 0] = 3;
            cells[3, 1] = 1;
            cells[8, 2] = 2;

            // Remove candidates 1, 2 and 3 from (0, 8)
            cells[1, 8] = 1;
            cells[2, 8] = 2;
            cells[4, 8] = 3;

            // Remove candidates 1, 2 and 3 from the rest of Block 0 so block check fails aswell.
            cells[1, 0] = 4; cells[1, 1] = 5; cells[1, 2] = 6;
            cells[2, 0] = 7; cells[2, 1] = 8;


            Assert.IsTrue(solver.InitializeBitmaskGrids());
            var result = solver.FindNakedTriple();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void HiddenTriple_Row_FoundTrue()
        {
            // Hidden Triple {1, 2, 3} in Row 0 at cols 0, 1, 2.

            // Removes 4, 5, 6, 7, 8 from the candidates of (0,0), (0,1), (0,2).
            cells[0, 3] = 4;
            cells[0, 4] = 5;
            cells[0, 5] = 6;
            cells[0, 6] = 7;
            cells[0, 7] = 8;

            // Remove candidates 1, 2 and 3 from (0, 8)
            cells[1, 8] = 1;
            cells[2, 8] = 2;
            cells[3, 8] = 3;

            Assert.IsTrue(solver.InitializeBitmaskGrids());

            var result = solver.FindHiddenTriple();

            Assert.IsNotNull(result);

            // 000 000 111
            Assert.AreEqual(7, result.Value.Item2);

            List<Point> points = result.Value.Item1;
            Assert.AreEqual(3, points.Count);
            Assert.IsTrue(points.Contains(new Point(0, 0)));
            Assert.IsTrue(points.Contains(new Point(1, 0)));
            Assert.IsTrue(points.Contains(new Point(2, 0)));
        }

        [TestMethod]
        public void HiddenTriple_Row_FoundFalse()
        {
            // Hidden Triple {1, 2, 3} in Row 0 at cols 0, 1, 2.

            // Removes 4, 5, 6, 7, 8 from the candidates of (0,0), (0,1), (0,2).
            cells[0, 3] = 4;
            cells[0, 4] = 5;
            cells[0, 5] = 6;
            cells[0, 6] = 7;
            cells[0, 7] = 8;

            // Remove candidates 1, 2 and 3 from (0, 8)
            cells[1, 8] = 1;
            cells[2, 8] = 2;
            cells[3, 8] = 3;

            // Remove 9 from col 0, 1 and 2
            cells[2, 2] = 9;

            Assert.IsTrue(solver.InitializeBitmaskGrids());
            var result = solver.FindHiddenTriple();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void XWing_FoundFalse()
        {
            // Setup a potential X-Wing for candidate 1 in Rows 1 and 5, Cols 2 and 6.
            // Ruin it by adding candidate 1 to (1, 3).

            // Fill Row 1 with noise, leaving holes at 2, 3, and 6.
            cells[1, 0] = 2;
            cells[1, 1] = 3;
            cells[1, 4] = 5;
            cells[1, 5] = 6;
            cells[1, 7] = 8;
            cells[1, 8] = 9;

            // Fill Row 5 with different noise to avoid column conflicts with Row 1.
            cells[5, 0] = 4;
            cells[5, 1] = 5;
            cells[5, 3] = 2;
            cells[5, 4] = 3;
            cells[5, 5] = 8;
            cells[5, 7] = 6;
            cells[5, 8] = 7;

            Assert.IsTrue(solver.InitializeBitmaskGrids());

            var result = solver.FindXWing();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Xwing_FoundTrue()
        {
            // 003 800 510
            // 008 700 930
            // 1x0 3x5 728

            // 000 200 849
            // 8x1 9x6 257
            // 000 506 163

            // 964 127 305
            // 382 659 401
            // 010 400 002

            cells[2, 0] = 1;
            cells[4, 0] = 8;
            cells[6, 0] = 9;
            cells[7, 0] = 3;
            cells[6, 1] = 6;
            cells[7, 1] = 8;
            cells[8, 1] = 1;
            cells[0, 2] = 3;
            cells[1, 2] = 8;
            cells[4, 2] = 1;
            cells[6, 2] = 4;
            cells[7, 2] = 2;
            cells[0, 3] = 8;
            cells[1, 3] = 7;
            cells[2, 3] = 3;
            cells[3, 3] = 2;
            cells[4, 3] = 9;
            cells[5, 3] = 5;
            cells[6, 3] = 1;
            cells[7, 3] = 6;
            cells[8, 3] = 4;

            cells[6, 4] = 2;
            cells[7, 4] = 5;

            cells[2, 5] = 5;
            cells[4, 5] = 6;
            cells[6, 5] = 7;
            cells[7, 5] = 9;

            cells[0, 6] = 5;
            cells[1, 6] = 9;
            cells[2, 6] = 7;
            cells[3, 6] = 8;
            cells[4, 6] = 2;
            cells[5, 6] = 1;
            cells[6, 6] = 3;
            cells[7, 6] = 4;
            cells[8, 6] = 6;

            cells[0, 7] = 1;
            cells[1, 7] = 3;
            cells[2, 7] = 2;
            cells[3, 7] = 4;
            cells[4, 7] = 5;
            cells[5, 7] = 6;
            cells[6, 7] = 8;
            cells[7, 7] = 7;
            cells[8, 7] = 9;

            cells[2, 8] = 8;
            cells[3, 8] = 9;
            cells[4, 8] = 7;
            cells[5, 8] = 3;
            cells[6, 8] = 5;
            cells[7, 8] = 1;
            cells[8, 8] = 2;

            solver.InitializeBitmaskGrids();
            var result = solver.FindXWing();

            Assert.IsNotNull(result);
            Assert.AreEqual(8, result.Value.Item3);

            List<Point> points = result.Value.Item1;
            Assert.IsTrue(points.Contains(new Point(1, 2)), $"{points[0]}");
            Assert.IsTrue(points.Contains(new Point(1, 4)), $"{points[1]}");
            Assert.IsTrue(points.Contains(new Point(4, 2)), $"{points[2]}");
            Assert.IsTrue(points.Contains(new Point(4, 4)), $"{points[3]}");

        }

        [TestMethod]
        public void XWing_Column_FoundTrue()
        {
            int val = 1;
            for (int r = 1; r < 8; r++)
            {
                if (val == 7) val++;
                cells[r, 2] = val;
                val++;
            }

            val = 8;
            for (int r = 1; r < 8; r++)
            {
                if (val == 7) val--;
                cells[r, 6] = val;
                val--;
            }

            cells[4, 2] = 5;
            cells[5, 2] = 4;
            bool s = solver.InitializeBitmaskGrids();
            Assert.IsTrue(s);

            var result = solver.FindXWing();

            Assert.IsNotNull(result);

            // Candidate 7: 001 000 000
            Assert.AreEqual(64, result.Value.Item3);

            List<Point> points = result.Value.Item1;
            Assert.AreEqual(4, points.Count);
            Assert.IsTrue(points.Contains(new Point(2, 0)), $"{points[0]}");
            Assert.IsTrue(points.Contains(new Point(6, 0)), $"{points[1]}");
            Assert.IsTrue(points.Contains(new Point(2, 8)), $"{points[2]}");
            Assert.IsTrue(points.Contains(new Point(6, 8)), $"{points[3]}");
        }

        [TestMethod]
        public void Xwing_Row_FoundTrue()
        {
            int val = 1;
            for (int c = 1; c < 8; c++)
            {
                if (val == 4) val++;
                cells[1, c] = val;
                val++;
            }

            val = 8;
            for (int c = 1; c < 8; c++)
            {
                if (val == 4) val--;
                cells[5, c] = val;
                val--;
            }

            cells[1, 3] = 5;
            cells[1, 4] = 3;
            bool s = solver.InitializeBitmaskGrids();
            Assert.IsTrue(s);

            var result = solver.FindXWing();

            Assert.IsNotNull(result);

            // Candidate 4: 000 001 000
            Assert.AreEqual(8, result.Value.Item3);

            List<Point> points = result.Value.Item1;
            Assert.AreEqual(4, points.Count);
            Assert.IsTrue(points.Contains(new Point(0, 1)), $"{points[0]}");
            Assert.IsTrue(points.Contains(new Point(8, 1)), $"{points[1]}");
            Assert.IsTrue(points.Contains(new Point(0, 5)), $"{points[2]}");
            Assert.IsTrue(points.Contains(new Point(8, 5)), $"{points[3]}");
        }

        [TestMethod]
        public void YWing_FoundFalse()
        {
            // Pivot (0,0) {1, 2}
            // Pincer A (0,5) {1, 3}
            // Pincer B (5,0) {2, 4} <-- The '4' breaks it (it should be 3 for a valid Y-Wing)

            // Block candidates 4-9 in Row 0
            cells[0, 1] = 4;
            cells[0, 2] = 5;
            cells[0, 3] = 6;
            cells[0, 4] = 7;
            cells[0, 6] = 8;
            cells[0, 7] = 9;

            // Block candidates 5-9 in Col 0 (We leave 4 available for Pincer B)
            cells[1, 0] = 8;
            cells[2, 0] = 9;
            cells[3, 0] = 5;
            cells[4, 0] = 6;
            cells[6, 0] = 7;

            // Refine Pivot (0,0) candidates to {1, 2}
            cells[2, 2] = 3;

            // Refine Pincer A (0,5) candidates to {1, 3}
            cells[1, 5] = 2;

            // Refine Pincer B (5,0) candidates to {2, 4}
            cells[5, 1] = 1;
            cells[5, 3] = 3;

            Assert.IsTrue(solver.InitializeBitmaskGrids());
            var result = solver.FindYWing();

            Assert.IsNull(result);
        }

        public void Ywing_FoundTrue()
        {
            // 045 090 678
            // 000 000 000
            // 000 002 000
            // 000 000 000
            // 000 000 000
            // 014 560 789
            // 000 000 000
            // 000 000 000
            // 000 000 000

            // Restrict (0,0) to {1, 2}.
            cells[1, 1] = 3;
            cells[0, 1] = 4; cells[0, 2] = 5; cells[0, 6] = 6; cells[0, 7] = 7; cells[0, 8] = 8;
            cells[0, 4] = 9;

            // Restrict (0,5) to {1, 3}.
            cells[2, 5] = 2;

            // Restrict (5,0) to {2, 3}.
            cells[5, 1] = 1;
            cells[5, 2] = 4; cells[5, 3] = 5; cells[5, 4] = 6; cells[5, 6] = 7; cells[5, 7] = 8; cells[5, 8] = 9;

            // Now we have a target cell (5,5) that is the intersection of the pincers. Its candidate 3 should be eliminated.
            Assert.IsTrue(solver.InitializeBitmaskGrids());

            var result = solver.FindYWing();
            Assert.IsNotNull(result);

            List<Point> points = result.Value.Item1;
            Assert.IsTrue(points.Contains(new Point(0, 0)), $"{points[0]}");
            Assert.IsTrue(points.Contains(new Point(0, 5)), $"{points[1]}");
            Assert.IsTrue(points.Contains(new Point(5, 0)), $"{points[2]}");
        }

        [TestMethod]
        public void Swordfish_FoundFalse()
        {
            // Try to set up a Swordfish for candidate 4 on Rows 1, 4, 7 and Cols 2, 5, 8.
            // But spoil Row 1 by adding candidate 4 to Col 0.

            int[] fishRows = { 1, 4, 7 };
            int[] fishCols = { 2, 5, 8 };
            int[] fillerValues = { 1, 2, 3, 5, 6, 9 }; // Notice 4 is missing

            // Setup "Perfect" rows first
            for (int i = 0; i < fishRows.Length; i++)
            {
                int r = fishRows[i];
                int fillerIndex = 0;
                for (int c = 0; c < 9; c++)
                {
                    // If not a fish col, fill it with noise
                    if (!fishCols.Contains(c))
                    {
                        // SPOILER: For Row 1 (i==0), do NOT fill Col 0.
                        if (i == 0 && c == 0) continue;

                        int val = fillerValues[(fillerIndex + i) % fillerValues.Length];
                        cells[r, c] = val;
                        fillerIndex++;
                    }
                }
            }

            // At this point:
            // Row 4 and 7 have candidate 4 locked in Cols {2, 5, 8}.
            // Row 1 has candidate 4 locked in Cols {0, 2, 5, 8}.
            // Because Row 1 has an extra candidate column that doesn't align with the other rows (Col 0),
            // The 3x3 Swordfish pattern is broken.

            Assert.IsTrue(solver.InitializeBitmaskGrids());

            var result = solver.FindSwordfish();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Swordfish_Row_FoundTrue()
        {
            // In Rows 1, 4, 7, the candidate 4 only exists in Cols 2, 5, 8.
            // Candidate 4 can be eliminated from Cols 2, 5, 8 in all other rows.

            int candidateVal = 4;
            int[] fishRows = { 1, 4, 7 };
            int[] fishCols = { 2, 5, 8 };

            int[] fillerValues = { 1, 2, 3, 5, 6, 9 };

            // Rows (1, 4, 7)
            for (int i = 0; i < fishRows.Length; i++)
            {
                int r = fishRows[i];
                int fillerIndex = 0;

                for (int c = 0; c < 9; c++)
                {
                    // If this is not fish column, we fill it.
                    if (!fishCols.Contains(c))
                    {
                        // Shift to avoid non-valid Sudoku.
                        int val = fillerValues[(fillerIndex + i) % fillerValues.Length];
                        cells[r, c] = val;
                        fillerIndex++;
                    }
                }
            }

            // Target cell.
            cells[0, 2] = 0;

            solver = new BitmaskSolver(cells);
            Assert.IsTrue(solver.InitializeBitmaskGrids());

            var result = solver.FindSwordfish();

            Assert.IsNotNull(result);

            // 4
            Assert.AreEqual(8, result.Value.Item3);


            List<Point> points = result.Value.Item1;
            Assert.IsTrue(points.Count >= 6);
            foreach (var p in points)
            {
                Assert.IsTrue(fishRows.Contains(p.Y), $"Point {p} should be Fish Row");
                Assert.IsTrue(fishCols.Contains(p.X), $"Point {p} should be Fish Col");
            }
        }

        [TestMethod]
        public void Swordfish_Column_FoundTrue()
        {
            // In Cols 0, 4, 8, the candidate 6 only exists in Rows 1, 5, 7.
            // Candidate 6 can be eliminated from Rows 1, 5, 7 in all other cols.

            int candidateVal = 6;
            int[] fishCols = { 0, 4, 8 };
            int[] fishRows = { 1, 5, 7 };

            int[] fillerValues = { 1, 2, 3, 4, 5, 7, 8, 9 };

            // Columns (0, 4, 8)
            for (int i = 0; i < fishCols.Length; i++)
            {
                int c = fishCols[i];
                int fillerIndex = 0;

                for (int r = 0; r < 9; r++)
                {
                    if (!fishRows.Contains(r))
                    {
                        int val = fillerValues[(fillerIndex + i) % fillerValues.Length];
                        cells[r, c] = val;
                        fillerIndex++;
                    }
                }
            }

            cells[1, 1] = 0;

            solver = new BitmaskSolver(cells);
            Assert.IsTrue(solver.InitializeBitmaskGrids());

            var result = solver.FindSwordfish();

            Assert.IsNotNull(result);

            // 6
            Assert.AreEqual(32, result.Value.Item3);

            List<Point> points = result.Value.Item1;
            foreach (var p in points)
            {
                Assert.IsTrue(fishCols.Contains(p.X), $"Point {p} should be Fish Col");
                Assert.IsTrue(fishRows.Contains(p.Y), $"Point {p} should be Fish Row");
            }
        }

    }
}
