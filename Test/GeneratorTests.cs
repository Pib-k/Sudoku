using Sudoku_Namespace.Utilities;
using System.Text;

namespace SudokuTests
{
    [TestClass]
    public class GeneratorTests
    {
        [TestMethod]
        public void TestSeedGeneration()
        {
            Generator g = new Generator();
            int[,]? sudoku = g.SeedGenerator();
            string s2 = "";


            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    sb.Append(sudoku[r, c].ToString() + " ");
                    s2 += sudoku[r, c].ToString();
                }
                sb.Append("\n"); // Add new line after every row
            }

            string res = sb.ToString();
            Assert.IsNotNull(sudoku);
            Console.WriteLine(res);
            Console.WriteLine(s2);
        }

        [TestMethod]
        public void TestGenerationBasedOnDifficulty_FoundEasy()
        {
            DifficultyAnalyzer da = new DifficultyAnalyzer();
            Generator generator = new Generator();
            int[,] sudoku = generator.GenerateSudoku(DifficultyLevel.Easy);
            string sudokuString = "";

            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    sb.Append(sudoku[r, c].ToString() + " ");
                    sudokuString += sudoku[r, c].ToString();
                }
                sb.Append("\n");
            }

            Assert.AreEqual(DifficultyLevel.Easy, da.EvaluateDifficulty(sudoku));


            Console.WriteLine(sudokuString);
            Console.WriteLine(sb.ToString());
        }

        [TestMethod]
        public void TestGenerationBasedOnDifficulty_FoundMedium()
        {
            DifficultyAnalyzer da = new DifficultyAnalyzer();
            Generator generator = new Generator();
            int[,] sudoku = generator.GenerateSudoku(DifficultyLevel.Medium);
            string sudokuString = "";

            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    sb.Append(sudoku[r, c].ToString() + " ");
                    sudokuString += sudoku[r, c].ToString();
                }
                sb.Append("\n");
            }

            Assert.AreEqual(DifficultyLevel.Medium, da.EvaluateDifficulty(sudoku));


            Console.WriteLine(sudokuString);
            Console.WriteLine(sb.ToString());
        }

        [TestMethod]
        public void TestGenerationBasedOnDifficulty_FoundHard()
        {
            DifficultyAnalyzer da = new DifficultyAnalyzer();
            Generator generator = new Generator();
            int[,] sudoku = generator.GenerateSudoku(DifficultyLevel.Hard);
            string sudokuString = "";

            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    sb.Append(sudoku[r, c].ToString() + " ");
                    sudokuString += sudoku[r, c].ToString();
                }
                sb.Append("\n");
            }

            Assert.AreEqual(DifficultyLevel.Hard, da.EvaluateDifficulty(sudoku));
            Console.WriteLine(sudokuString);
            Console.WriteLine(sb.ToString());
        }

        [TestMethod]
        public void TestGenerationBasedOnDifficulty_FoundExpert()
        {
            DifficultyAnalyzer da = new DifficultyAnalyzer();
            Generator generator = new Generator();
            int[,] sudoku = generator.GenerateSudoku(DifficultyLevel.Expert);
            string sudokuString = "";

            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    sb.Append(sudoku[r, c].ToString() + " ");
                    sudokuString += sudoku[r, c].ToString();
                }
                sb.Append("\n"); // Add new line after every row
            }

            Assert.AreEqual(DifficultyLevel.Expert, da.EvaluateDifficulty(sudoku));

            Console.WriteLine(sudokuString);
            Console.WriteLine(sb.ToString());
        }
    }
}
