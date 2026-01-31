using Sudoku_Namespace.Utilities;

namespace SudokuTests
{
    [TestClass]
    public class DifficultyMeterTests
    {
        private int[,] cells;

        private void Setup(string sudoku)
        {
            cells = new int[9, 9];
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (sudoku != null)
                    {
                        this.cells[r, c] = sudoku[(r * 9) + c] - '0';
                    }
                    else { this.cells[r, c] = 0; }
                }
            }
        }

        [TestMethod]
        public void EvaluateDifficulty_Easy()
        {
            string sudoku = "070000043040009610800634900094052000358460020000800530080070091902100005007040802";
            Setup(sudoku);
            DifficultyAnalyzer dm = new DifficultyAnalyzer();
            DifficultyLevel? diff = dm.EvaluateDifficulty(cells);

            Assert.AreEqual(DifficultyLevel.Easy, diff, $"{diff}");
        }

        [TestMethod]
        public void EvaluateDifficulty_Expert()
        {
            string swordfishSudoku = "951000486000800050608050203000704030409000000020600040300000508080502360000000104";
            Setup(swordfishSudoku);
            DifficultyAnalyzer dm = new DifficultyAnalyzer();
            DifficultyLevel? diff = dm.EvaluateDifficulty(cells);

            Assert.AreEqual(DifficultyLevel.Expert, diff, $"{diff}");
        }

        [TestMethod]
        public void EvaluateDifficulty_ReturnsNullAsInvalid()
        {
            string ywing = "002222022200000000000005000000000600000000007000800000003000000000000900000000000";
            Setup(ywing);
            DifficultyAnalyzer dm = new DifficultyAnalyzer();
            DifficultyLevel? diff = dm.EvaluateDifficulty(cells);

            Assert.IsNull(diff, $"{diff}");
        }

        [TestMethod]
        public void EvaluateDifficulty_UnsolvableSudoku()
        {
            string notSolvable = "530070000600295000098000060800060003400803001700020006060000280000419005000080079";
            Setup(notSolvable);
            DifficultyAnalyzer dm = new DifficultyAnalyzer();
            DifficultyLevel? diff = dm.EvaluateDifficulty(cells);

            Console.Write(cells.ToString());
            Assert.IsNull(diff, $"{diff}");

        }
    }
}
