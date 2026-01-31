using Sudoku_Namespace.Models;
using Sudoku_Namespace.Utilities;

namespace Sudoku_Namespace.IO
{

    /// <summary>
    /// Data transfer object used for serializing and de-serializing the Sudoku game state.
    /// </summary>
    public class GameSaveData
    {
        /// <summary>
        /// Gets or sets the jagged array representing the Sudoku grid state, 
        /// including values, fixed states and notations
        /// </summary>
        public Cell[][] GridData { get; set; }

        /// <summary>
        /// Gets or sets the elapsed time of the game in ticks.
        /// </summary>
        public long TimeElapsedTicks { get; set; }

        /// <summary>
        /// Gets or sets the difficulty level selected for this game.
        /// </summary>
        public DifficultyLevel Difficulty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the game was in Builder Mode or Play Mode.
        /// </summary>
        public bool IsBuilderMode { get; set; }
    }
}
