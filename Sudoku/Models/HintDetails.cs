using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Sudoku_Namespace.Models
{

    /// <summary>
    /// Specifies the category of hint a user receives.
    /// </summary>
    public enum HintType { None, Error, Suggestion }


    /// <summary>
    /// Every strategy/technique that can be used to provide hints.
    /// </summary>
    public enum StrategyType
    {
        None, NakedSingle, HiddenSingle, NakedPair,
        HiddenPair, PointingPairOrTriple, NakedTriple, HiddenTriple,
        XWing, YWing, Swordfish
    }

    /// <summary>
    /// Combines all relevant information about a hint to be presented to the user. This includes strategy type, affected cells, and candidate numbers.
    /// </summary>
    public class HintDetails
    {

        public HintType Type { get; set; } = HintType.None;

        public StrategyType Strategy { get; set; } = StrategyType.None;

        /// <summary>
        /// Gets or sets the list of cells where an action should be taken.
        /// </summary>
        public List<Point> TargetCells { get; set; } = new List<Point>();

        /// <summary>
        /// Gets or sets the list of cells that form the basis/source for the hint.
        /// </summary>
        public List<Point> SourceCells { get; set; } = new List<Point>();

        /// <summary>
        /// Gets or sets a bitmask representing the candidate numbers involved in the strategy.
        /// <remark>
        /// Bit 0 represents number 1, Bit 1 represents number 2, etc.
        /// </remark>
        public int CandidateMask { get; set; } = 0;

        /// <summary>
        /// Generates a human-readable explanation of the hint based on the identified strategy and context.
        /// </summary>
        public string GetExplanation()
        {
            string nums = GetNumbersString();
            string unit = GetUnitName();

            if (Type == HintType.Error)
            {
                return "There are incorrectly filled cells.";
            }

            return Strategy switch
            {
                StrategyType.NakedSingle =>
                    $"This cell has only one possible candidate remaining: {nums}. It must be placed here.",

                StrategyType.HiddenSingle =>
                    $"The number {nums} cannot go anywhere else in this row, column, or block. It is forced into this cell.",

                StrategyType.NakedPair =>
                    $" Two cells in {unit} are restricted to contain only {nums}. No other cells in {unit} can contain these numbers.",

                StrategyType.HiddenPair =>
                    $" The numbers {nums} are restricted to only two cells in {unit}. All other candidates can be removed from these cells.",

                StrategyType.PointingPairOrTriple =>
                    $" The number {nums} is restricted to a single line within a block. Since it must be in this block's line, it cannot exist in the rest of {unit} outside the block.",

                StrategyType.NakedTriple =>
                    $"Three cells in {unit} are restricted to contain only {nums}. No other cells in {unit} can contain these numbers.",

                StrategyType.HiddenTriple =>
                    $"The numbers {nums} are restricted to only three cells in {unit}. All other candidates can be removed from these cells.",

                StrategyType.XWing =>
                    $" The number {nums} forms an X-Wing pattern. It appears exactly twice in two rows (or columns) and aligns perfectly. This forms a locked rectangle, allowing you to eliminate {nums} from the cross-sections.",

                StrategyType.YWing =>
                    $" A Y-Wing chain exists here involving candidates {nums}. No matter which value the 'pivot' cell takes, one of the 'pincers' will eliminate the common candidate from the intersection.",

                StrategyType.Swordfish =>
                    $" The number {nums} forms a Swordfish pattern. It is locked into three rows (or columns), forming a grid that eliminates this candidate from other cells in the perpendicular lines.",

                _ => "No hint available."
            };
        }

        /// <summary>
        /// Converts the <see cref="CandidateMask"/> bitmask into a comma-separated string of numbers.
        /// </summary>
        private string GetNumbersString()
        {
            if (CandidateMask == 0) return "unknown";

            var values = new List<int>();
            for (int i = 0; i < 9; i++)
            {
                // Check if the bit at index i is set
                if ((CandidateMask & (1 << i)) != 0)
                {
                    values.Add(i + 1);
                }
            }
            return string.Join(", ", values);
        }

        /// <summary>
        /// Determines the name of the unit (Row, Column, Block) that is relevant to the current strategy.
        /// </summary>
        private string GetUnitName()
        {
            if (Strategy == StrategyType.XWing || Strategy == StrategyType.YWing || Strategy == StrategyType.Swordfish)
            {
                return "the grid";
            }

            List<Point> unitPoints = SourceCells;
            if (unitPoints == null || unitPoints.Count == 0)
            {
                unitPoints = TargetCells;
            }

            if (unitPoints == null || unitPoints.Count < 2)
            {
                return "cell";
            }

            List<string> units = new List<string>();

            // Special logic for Pointing Pairs and Pointing Triples
            if (Strategy == StrategyType.PointingPairOrTriple)
            {
                int tRow = TargetCells[0].Y;
                if (unitPoints.All(p => p.Y == tRow)) { return $"Row {tRow + 1}"; }

                int tCol = TargetCells[0].X;
                if (unitPoints.All(p => p.X == tCol)) { return $"Column {tCol + 1}"; }
            }

            // Standard logic for Pairs/Triples
            int firstRow = unitPoints[0].Y;
            if (unitPoints.All(p => p.Y == firstRow))
            {
                units.Add($"Row {firstRow + 1}");
            }

            int firstCol = unitPoints[0].X;
            if (unitPoints.All(p => p.X == firstCol))
            {
                units.Add($"Column {firstCol + 1}");
            }

            int firstBlock = (unitPoints[0].Y / 3) * 3 + (unitPoints[0].X / 3);
            if (unitPoints.All(p => ((p.Y / 3) * 3 + (p.X / 3)) == firstBlock))
            {
                units.Add($"Block {firstBlock + 1}");
            }

            if (units.Count == 0) return "unknown unit";

            return string.Join(" and ", units);

        }
    }
}
