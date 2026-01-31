using Sudoku_Namespace.Models;

/* 
 * Test setup and some tests for CoreGrid
 */
namespace SudokuTests
{
    [TestClass]
    public class CoreGridTests
    {
        private CoreGrid grid;

        [TestInitialize]
        public void Setup()
        {
            grid = new CoreGrid(9);
        }

        [TestMethod]
        public void SetValue()
        {
            grid.SetValue(2, 3, 7, true);

            Assert.AreEqual(7, grid.GetValue(2, 3));
            Assert.IsTrue(grid.IsFixed(2, 3));
        }

        [TestMethod]
        public void ClearValue()
        {
            grid.SetValue(1, 1, 5, true);
            grid.ClearValue(1, 1);

            Assert.AreEqual(0, grid.GetValue(1, 1));
            Assert.IsFalse(grid.IsFixed(1, 1));
        }

        [TestMethod]
        public void ClearNotations()
        {
            grid.GridArray[4, 4].Notations.Add(1);
            grid.GridArray[4, 4].Notations.Add(2);
            grid.GridArray[4, 4].Notations.Add(3);

            grid.ClearNotations(4, 4);

            Assert.AreEqual(0, grid.GridArray[4, 4].Notations.Count);
        }

        [TestMethod]
        public void IsValidMove_EmptyBoard()
        {
            Assert.IsTrue(grid.IsValidMove(0, 0, 5));
            Assert.IsTrue(grid.IsValidMove(4, 4, 9));
            Assert.IsTrue(grid.IsValidMove(8, 8, 1));
        }

        [TestMethod]
        public void IsValidMove_CellEmpty()
        {
            grid.SetValue(0, 0, 5, false);

            Assert.IsFalse(grid.IsValidMove(0, 0, 3));
            Assert.IsFalse(grid.IsValidMove(0, 0, 5));
        }

        [TestMethod]
        public void IsValidMove_Row()
        {
            grid.SetValue(1, 0, 2, false);

            Assert.IsTrue(grid.IsValidMove(0, 0, 3));

            Assert.IsFalse(grid.IsValidMove(2, 0, 2));
        }

        [TestMethod]
        public void IsValidMove_Column()
        {
            grid.SetValue(3, 0, 7, false);

            Assert.IsFalse(grid.IsValidMove(3, 4, 7));
            Assert.IsTrue(grid.IsValidMove(3, 4, 1));
        }

        [TestMethod]
        public void IsValidMove_Block()
        {
            grid.SetValue(0, 0, 9, false);
            Assert.IsFalse(grid.IsValidMove(1, 1, 9));

            Assert.IsTrue(grid.IsValidMove(1, 1, 4));
        }

        [TestMethod]
        public void IsValidMove_ColumnRow()
        {
            // Unique row: y = 0
            for (int x = 0; x < 9; x++)
                grid.SetValue(x, 0, x + 1, false);

            // Unique column: x = 0
            for (int y = 0; y < 9; y++)
                grid.SetValue(0, y, y + 1, false);

            // Make (0, 0) empty again
            grid.SetValue(0, 0, 0, false);

            Assert.IsTrue(grid.IsValidMove(0, 0, 1));
        }
    }
}

