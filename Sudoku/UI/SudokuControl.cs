using Sudoku_Namespace.Algorithms;
using Sudoku_Namespace.Models;
using Sudoku_Namespace.Rendering;
using Sudoku_Namespace.Utilities;
using Sudoku_Namespace.Utility;
using Suudoku.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sudoku_Namespace.UI
{
    /// <summary>
    /// Custom control responsible for rendering the Sudoku board, handling user input and managing the game state.
    /// </summary>
    public class SudokuControl : Control
    {
        private const int SUDOKU_DIMENSION = 9;
        public int BoardSize { get; set; } = 450;
        public int BoardLeft { get; set; } = 50;
        public int BoardTop { get; set; } = 60;

        private int highlightedCellX = 0;
        private int highlightedCellY = 0;

        /// <summary>
        /// Gets or sets the current hint details to be displayed on the board, if there is any.
        /// </summary>
        public HintDetails CurrentHint { get; set; } = new HintDetails();

        public GameStopwatch GameTimer { get; } = new GameStopwatch();

        private GameState _currentState = GameState.Playing;

        /// <summary>
        /// Gets the current state of the game loop. Setting this fires the StateChanged event.
        /// </summary>
        public GameState CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    HandleTimerForState(value);
                    StateChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        public event EventHandler StateChanged;

        private bool showCompletionText = false;
        private float completionTextAlpha = 1f;
        private Timer completionTextTimer;
        public event EventHandler CompletionAnimationFinished;

        private int[,] solutionGrid;

        /// <summary>
        /// Stores coordinates of incorrectly filled cells.
        /// </summary>
        public HashSet<Point> WrongCells { get; set; } = new HashSet<Point>();

        private SudokuBoardRenderer _renderer;

        /// <summary>
        /// The underlying data structure representing the 9x9 Sudoku grid.
        /// </summary>
        public CoreGrid Grid { get; private set; } = new CoreGrid(SUDOKU_DIMENSION);

        private readonly Stack<Cell[,]> undoGrids = new Stack<Cell[,]>();
        private readonly Stack<Cell[,]> redoGrids = new Stack<Cell[,]>();
        private readonly int[,] conflictType = new int[SUDOKU_DIMENSION, SUDOKU_DIMENSION];

        /// <summary>
        /// Occurs when a hint is generated or cleared.
        /// </summary>
        public event EventHandler HintChanged;

        /// <summary>
        /// Occurs when the notation mode is toggled.
        /// </summary>
        public event EventHandler NotationModeChanged;

        private bool _notationMode = false;

        /// <summary>
        /// Gets or sets a value indicating whether input is treated as notations or final values.
        /// </summary>
        public bool NotationMode
        {
            get { return _notationMode; }
            set
            {
                if (_notationMode != value)
                {
                    _notationMode = value;
                    NotationModeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the renderer used to draw the Sudoku board.
        /// </summary>
        public SudokuBoardRenderer Renderer
        {
            get { return _renderer; }
            set
            {
                _renderer = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SudokuControl"/> class.
        /// Sets up double buffering, input styles, and default game state.
        /// </summary>
        public SudokuControl()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
            KeyDown += ArrowPressed;
            KeyPress += KeyPressed;

            MouseClick += (sender, e) =>
            {
                if (CurrentState != GameState.Playing && CurrentState != GameState.Building)
                    return;

                if ((e.X - BoardLeft) < BoardSize && (e.Y - BoardTop) < BoardSize &&
                    (e.X - BoardLeft) >= 0 && (e.Y - BoardTop) >= 0)
                {
                    int cellSize = BoardSize / SUDOKU_DIMENSION;
                    highlightedCellX = (e.X - BoardLeft) / cellSize;
                    highlightedCellY = (e.Y - BoardTop) / cellSize;
                    Focus();
                    Invalidate();
                }
            };

            NewGame(DifficultyLevel.Easy);
        }

        private void HandleTimerForState(GameState state)
        {
            switch (state)
            {
                case GameState.Playing: GameTimer.Start(); break;
                case GameState.Paused: 
                case GameState.Won: GameTimer.Stop(); break;
                case GameState.Building: GameTimer.Reset(); break;
            }
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
            {
                CurrentState = GameState.Paused;
            }
            else if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Playing;
            }
        }

        /// <summary>
        /// Determines whether the specified key is a regular input key or a special key that requires extra processing.
        /// Overridden to have Arrow keys be the movement control.
        /// </summary>
        /// <param name="keyData">One of the <see cref="Keys"/> values.</param>
        /// <returns>True if the key is an arrow key; otherwise, calls the base implementation.</returns>
        protected override bool IsInputKey(Keys keyData) => keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right || base.IsInputKey(keyData);

        /// <summary>
        /// Paints the background, the grid, the hints, and any overlays.
        /// </summary>
        /// <param name="pea">A <see cref="PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs pea)
        {
            base.OnPaint(pea);
            Graphics g = pea.Graphics;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int startX = BoardLeft;
            int startY = BoardTop;
            bool isPaused = (CurrentState == GameState.Paused);
            bool isSolveMode = (CurrentState == GameState.Playing || CurrentState == GameState.Won || CurrentState == GameState.Paused);

            _renderer.Draw(g, Grid, BoardSize, 9, startX, startY, highlightedCellX, highlightedCellY, conflictType, isSolveMode, isPaused, CurrentHint);

            if (CurrentState == GameState.Won || showCompletionText)
            {
                DrawCompletionText(g);
            }
        }

        private void DrawCompletionText(Graphics g)
        {
            string text = "COMPLETED!";
            using (Font font = new Font("Segoe UI", BoardSize / 10f, FontStyle.Bold))
            {
                SizeF textSize = g.MeasureString(text, font);
                float x = BoardLeft + (BoardSize - textSize.Width) / 2;
                float y = BoardTop + (BoardSize - textSize.Height) / 2;

                Color textColor = Color.FromArgb((int)(completionTextAlpha * 255), Color.LimeGreen);
                using (Brush brush = new SolidBrush(textColor))
                {
                    g.DrawString(text, font, new SolidBrush(Color.FromArgb((int)(completionTextAlpha * 128), 0, 0, 0)), x + 3, y + 3);
                    g.DrawString(text, font, brush, x, y);
                }
            }
        }

        /// <summary>
        /// Handles arrow key presses to navigate the selection highlight across the grid.
        /// </summary>
        /// <param name="o">The sender object.</param>
        /// <param name="ea">Key event arguments.</param>
        public void ArrowPressed(object o, KeyEventArgs ea)
        {
            if (CurrentState != GameState.Playing && CurrentState != GameState.Building) { return; }

            switch (ea.KeyCode)
            {
                case Keys.Up:
                    highlightedCellY = (highlightedCellY - 1 + SUDOKU_DIMENSION) % SUDOKU_DIMENSION;
                    break;
                case Keys.Down:
                    highlightedCellY = (highlightedCellY + 1) % SUDOKU_DIMENSION;
                    break;
                case Keys.Left:
                    highlightedCellX = (highlightedCellX - 1 + SUDOKU_DIMENSION) % SUDOKU_DIMENSION;
                    break;
                case Keys.Right:
                    highlightedCellX = (highlightedCellX + 1) % SUDOKU_DIMENSION;
                    break;
            }
            Invalidate();
        }

        /// <summary>
        /// Handles placing numbers, clearing cells, or toggling modes/actions (Undo, Redo, Hints).
        /// </summary>
        /// <param name="o">The sender object.</param>
        /// <param name="ea">Key press event arguments.</param>
        public void KeyPressed(object o, KeyPressEventArgs ea)
        {

            if (CurrentState != GameState.Playing && CurrentState != GameState.Building) { return; }

            char keyChar = ea.KeyChar;

            if (keyChar >= '1' && keyChar <= '9')
            {
                PlaceNumber(keyChar - '0');
            }
            else if (keyChar == '0' || keyChar == '\b') // Allow backspace to clear cell
            {
                Delete();
            }
            else if (keyChar == 'n' || keyChar == 'N')
            {
                NotationMode = !NotationMode;
            }
            else if (keyChar == '\u0015') // Ctrl + U
            {
                Undo();
            }
            else if (keyChar == '\u0012') // Ctrl + R
            {
                Redo();
            }
            else if (keyChar == 'h' || keyChar == 'H')
            {
                ToggleHint();
            }
            Focus();
        }

        /// <summary>
        /// Places a number or notation in the currently highlighted cell, depending on the current mode.
        /// </summary>
        /// <param name="number">The number to place or use as a notation in the highlighted cell.</param>
        public void PlaceNumber(int number)
        {
            if (CurrentState != GameState.Playing && CurrentState != GameState.Building) { return; }

            ClearHints();

            if (NotationMode)
            {
                undoGrids.Push(CreateDeepCopy(Grid.GridArray));
                redoGrids.Clear();

                if (Grid.GridArray[highlightedCellY, highlightedCellX].Notations.Contains(number))
                {
                    Grid.GridArray[highlightedCellY, highlightedCellX].Notations.Remove(number);
                }
                else { Grid.AddNotation(highlightedCellY, highlightedCellX, number); }

                Invalidate();
            }
            else
            {
                if (CurrentState == GameState.Playing && Grid.IsFixed(highlightedCellY, highlightedCellX))
                {
                    return;
                }
                if (Grid.GetValue(highlightedCellY, highlightedCellX) == number) { return; }

                undoGrids.Push(CreateDeepCopy(Grid.GridArray));
                redoGrids.Clear();

                bool isFixed = (CurrentState == GameState.Building);
                Grid.SetValue(highlightedCellY, highlightedCellX, number, isFixed);
                RemoveNotationsFromPeers(highlightedCellY, highlightedCellX, number);
                UpdateConflicts();

                if (CurrentState == GameState.Playing) { CheckForCompletion(); }

                Invalidate();
            }
        }

        /// <summary>
        /// Deletes the value or notation from the currently highlighted cell, depending on the current mode.
        /// </summary>
        public void Delete()
        {
            if (CurrentState != GameState.Playing && CurrentState != GameState.Building) { return; }

            ClearHints();

            if (NotationMode)
            {
                undoGrids.Push(CreateDeepCopy(Grid.GridArray));
                redoGrids.Clear();

                Grid.ClearNotations(highlightedCellY, highlightedCellX);

                UpdateConflicts();
                Invalidate();
            }
            else
            {
                if (CurrentState == GameState.Playing && Grid.IsFixed(highlightedCellY, highlightedCellX)) { return; }
                undoGrids.Push(CreateDeepCopy(Grid.GridArray));
                redoGrids.Clear();

                Grid.SetValue(highlightedCellY, highlightedCellX, 0, false);
                UpdateConflicts();
                Invalidate();
            }
        }

        /// <summary>
        /// Reverts the board to its previous state, if a previous state is available.
        /// </summary>
        public void Undo()
        {
            if (CurrentState != GameState.Playing && CurrentState != GameState.Building) { return; }

            if (undoGrids.Count == 0) { return; }

            ClearHints();
            redoGrids.Push(CreateDeepCopy(Grid.GridArray));
            Grid.GridArray = undoGrids.Pop();
            UpdateConflicts();
            Invalidate();
        }

        /// <summary>
        /// Re-aplies a state that was previously undone. If such a state existst.
        /// </summary>
        public void Redo()
        {
            if (CurrentState != GameState.Playing && CurrentState != GameState.Building) { return; }

            if (redoGrids.Count == 0) { return; }

            ClearHints();
            undoGrids.Push(CreateDeepCopy(Grid.GridArray));
            Grid.GridArray = redoGrids.Pop();
            UpdateConflicts();
            Invalidate();
        }

        private void ClearHints()
        {
            if (CurrentHint != null)
            {
                CurrentHint = null;
                HintChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Initializes a new Sudoku game with the specified difficulty level.
        /// </summary>
        /// <param name="difficulty">The difficulty level to use when generating the new Sudoku puzzle. Determines the technique complexity and number of
        /// clues provided in the generated grid.</param>
        public void NewGame(DifficultyLevel difficulty)
        {
            Generator g = new Generator();
            int[,] generatedBoard = g.GenerateSudoku(difficulty);

            if (generatedBoard == null)
            {
                MessageBox.Show("Could not generate board of this difficulty.");
                throw new Exception("Failed to generate a new Sudoku board.");
            }
            Grid.GridArray = Grid.ValuesToCellArray(generatedBoard);

            undoGrids.Clear();
            redoGrids.Clear();
            WrongCells.Clear();
            ClearHints();
            GameTimer.Reset();
            GameTimer.Start();

            BitmaskSolver solver = new BitmaskSolver((int[,])generatedBoard.Clone());
            if (solver.InitializeBitmaskGrids()) { solutionGrid = solver.Solve(); }

            UpdateConflicts();
            CurrentState = (difficulty == DifficultyLevel.Custom) ? GameState.Building : GameState.Playing;
            Invalidate();
        }

        /// <summary>
        /// Loads a saved game state, resets the internal solver, and generates the new solution grid.
        /// </summary>
        /// <param name="loadedCells">The 9x9 Cell array loaded from the save file.</param>
        public void LoadGame(Cell[,] loadedCells, bool isBuilderMode)
        {
            Grid.GridArray = loadedCells;

            undoGrids.Clear();
            redoGrids.Clear();
            WrongCells.Clear();
            ClearHints();

            int[,] solverBoard = new int[9, 9];
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    solverBoard[r, c] = loadedCells[r, c].Value;
                }
            }
            BitmaskSolver solver = new BitmaskSolver(solverBoard);
            if (!solver.InitializeBitmaskGrids() || (solutionGrid = solver.Solve()) == null)
            {
                solutionGrid = new int[9, 9];
                if (!isBuilderMode)
                {
                    CurrentState = GameState.Building;
                    UpdateConflicts();
                    Invalidate();
                    throw new Exception("Loaded game is invalid and cannot be solved.");
                }
            }
            CurrentState = isBuilderMode ? GameState.Building : GameState.Paused;
            UpdateConflicts();
            Invalidate();
        }

        /// <summary>
        /// Checks if the current grid state represents a completely and correctly solved Sudoku.
        /// Triggers the completion animation if solved.
        /// </summary>
        public void CheckForCompletion()
        {
            if (solutionGrid == null) { throw new NullReferenceException("solutionGrid should not be null"); }

            WrongCells.Clear();
            bool filled = true;
            bool isCorrect = true;

            for (int r = 0; r < SUDOKU_DIMENSION; r++)
            {
                for (int c = 0; c < SUDOKU_DIMENSION; c++)
                {
                    int userValue = Grid.GetValue(r, c);
                    if (userValue == 0) { isCorrect = false; filled = false; }
                    else if (userValue != solutionGrid[r, c]) { WrongCells.Add(new Point(c, r)); isCorrect = false; }
                }
            }

            if (isCorrect)
            {
                CurrentState = GameState.Won;
                ShowCompletionText();
            }
            else if (filled)
            {
                CurrentHint = new HintDetails
                {
                    Type = HintType.Error,
                    TargetCells = new List<Point>(WrongCells),
                    Strategy = StrategyType.None
                };
                Invalidate();
            }

        }

        /// <summary>
        /// Plays the "COMPLETED" text overlay animation, fading it out over time.
        /// </summary>
        public void ShowCompletionText()
        {
            showCompletionText = true;
            completionTextAlpha = 1f;

            completionTextTimer = new Timer { Interval = 25 };
            completionTextTimer.Tick += (s, e) =>
            {
                completionTextAlpha -= 0.03f;
                if (completionTextAlpha <= 0f)
                {
                    completionTextAlpha = 0f;
                    showCompletionText = false;
                    completionTextTimer.Stop();
                    CompletionAnimationFinished?.Invoke(this, EventArgs.Empty);
                }
                Invalidate();
            };
            completionTextTimer.Start();
            Invalidate();
        }

        public async Task<bool> SolveAnimated()
        {
            CurrentState = GameState.Animating;
            try
            {
                BitmaskSolver solver = new BitmaskSolver((int[,])Grid.GetValueArray().Clone());
                if (solver.InitializeBitmaskGrids() == false) { return false; }

                void UpdateDisplay(int[,] currentGrid)
                {
                    for (int r = 0; r < SUDOKU_DIMENSION; r++)
                    {
                        for (int c = 0; c < SUDOKU_DIMENSION; c++)
                        {
                            if (!Grid.IsFixed(r, c))
                            {
                                Grid.SetValue(r, c, currentGrid[r, c], false);
                            }
                        }
                    }
                    Invalidate();
                }

                return await solver.SolveAsync(UpdateDisplay, 20);
            }
            finally
            {
                CurrentState = GameState.Won;
            }

        }

        /// <summary>
        /// Tries to solve the entire board using the <see cref="BitmaskSolver"/>. Fills empty cells with the solution if found.
        /// </summary>
        /// <returns>True if a solution was found and applied and false otherwise</returns>
        public bool Solve()
        {
            if (CurrentState == GameState.Paused) { return false; }

            BitmaskSolver solver = new BitmaskSolver((int[,])Grid.GetValueArray().Clone());
            if (solver.InitializeBitmaskGrids() == false) { return false; }

            int[,] result = solver.Solve();

            if (result != null)
            {
                for (int r = 0; r < SUDOKU_DIMENSION; r++)
                {
                    for (int c = 0; c < SUDOKU_DIMENSION; c++)
                    {
                        if (Grid.GridArray[r, c].Value == 0)
                        {
                            Grid.SetValue(r, c, result[r, c], false);
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RecalculateSolution()
        {
            BitmaskSolver solver = new BitmaskSolver((int[,])Grid.GetValueArray().Clone());
            solver.InitializeBitmaskGrids();
            solutionGrid = solver.Solve();
        }

        /// <summary>
        /// Updates the internal 2D conflict array based on the current grid state and the Sudoku rules.
        /// </summary>
        public void UpdateConflicts()
        {
            Array.Clear(conflictType, 0, conflictType.Length);

            for (int r = 0; r < SUDOKU_DIMENSION; r++)
            {
                for (int c = 0; c < SUDOKU_DIMENSION; c++)
                {
                    int value = Grid.GetValue(r, c);

                    if (value != 0)
                    {
                        HashSet<Point> conflicts = Grid.GetConflictPositions(r, c, value);

                        if (conflicts.Count > 0)
                        {
                            conflictType[r, c] = value;

                            foreach (Point p in conflicts)
                            {
                                conflictType[p.Y, p.X] = value;
                            }
                        }
                    }
                }
            }
            Invalidate();
        }

        /// <summary>
        /// Scans the grid for the first user-entered cell that violates Sudoku rules.
        /// </summary>
        /// <returns>
        /// A <see cref="HashSet{Point}"/> containing the coordinates of the conflicting cell 
        /// and all cells it conflicts with (row, column, or block peers), or <c>null</c> if no conflicts are found.
        /// </returns>
        public HashSet<Point> GetConflict()
        {
            for (int r = 0; r < SUDOKU_DIMENSION; r++)
            {
                for (int c = 0; c < SUDOKU_DIMENSION; c++)
                {
                    if (conflictType[r, c] != 0 && Grid.GridArray[r, c].Fixed == false)
                    {
                        HashSet<Point> conflictPos = Grid.GetConflictPositions(r, c, conflictType[r, c]);
                        conflictPos.Add(new Point(c, r));
                        return conflictPos;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Toggles the display of the current hint.
        /// </summary>
        public void ToggleHint()
        {
            if (CurrentState != GameState.Playing) { return; }

            if (CurrentHint != null) { CurrentHint = null; }
            else { ShowHint(); HintChanged?.Invoke(this, null); }
            Invalidate();
        }

        /// <summary>
        /// Removes the specified value from the notation lists of all peer cells (row, column, and 3x3 block).
        /// </summary>
        /// <param name="row">The row index of the placed value.</param>
        /// <param name="col">The column index of the placed value.</param>
        /// <param name="value">The value that was placed, which should be removed from peers's candidates.</param>
        private void RemoveNotationsFromPeers(int row, int col, int value)
        {
            for (int c = 0; c < 9; c++)
            {
                Grid.GridArray[row, c].Notations.Remove(value);
            }

            for (int r = 0; r < 9; r++)
            {
                Grid.GridArray[r, col].Notations.Remove(value);
            }

            int blockRowStart = (row / 3) * 3;
            int blockColStart = (col / 3) * 3;

            for (int r = blockRowStart; r < blockRowStart + 3; r++)
            {
                for (int c = blockColStart; c < blockColStart + 3; c++)
                {
                    Grid.GridArray[r, c].Notations.Remove(value);
                }
            }
        }

        /// <summary>
        /// Starts the <see cref="BitmaskSolver"/> to analyze the grid and determine the next step.
        /// Sets the <see cref="CurrentHint"/> property with the found strategy (e.g., Naked Single, SwordFish).
        /// </summary>
        public void ShowHint()
        {
            HashSet<Point> conflicts = GetConflict();

            if (conflicts != null && conflicts.Count > 0)
            {
                CurrentHint = new HintDetails
                {
                    Type = HintType.Error,
                    TargetCells = new List<Point>(conflicts),
                    Strategy = StrategyType.None
                };
                Invalidate();
                return;
            }
            else if (WrongCells.Count > 0)
            {
                CurrentHint = new HintDetails
                {
                    Type = HintType.Error,
                    TargetCells = new List<Point>(WrongCells),
                    Strategy = StrategyType.None
                };
                Invalidate();
                return;
            }

            BitmaskSolver solver = new BitmaskSolver((int[,])Grid.GetValueArray().Clone());
            if (!solver.InitializeBitmaskGrids()) { return; }
            solver.UserCandidates = GetUserCandidatesMask();

            // Naked single
            var ns = solver.FindNakedSingle();
            if (ns.HasValue)
            {
                CurrentHint = new HintDetails
                {
                    Type = HintType.Suggestion,
                    TargetCells = new List<Point> { ns.Value.Item1 },
                    SourceCells = null,
                    Strategy = StrategyType.NakedSingle,
                    CandidateMask = ns.Value.Item2
                };
                Invalidate();

                return;
            }

            // Hidden single
            var hs = solver.FindHiddenSingle();
            if (hs.HasValue)
            {
                CurrentHint = new HintDetails
                {
                    Type = HintType.Suggestion,
                    TargetCells = new List<Point> { hs.Value.Item1 },
                    SourceCells = null,
                    Strategy = StrategyType.HiddenSingle,
                    CandidateMask = hs.Value.Item2
                };
                Invalidate();
                return;
            }

            // Naked pair
            var np = solver.FindNakedPair();
            if (np.HasValue)
            {
                CurrentHint = new HintDetails
                {
                    Type = HintType.Suggestion,
                    SourceCells = np.Value.Item1,
                    TargetCells = np.Value.Item2,
                    Strategy = StrategyType.NakedPair,
                    CandidateMask = np.Value.Item3
                };
                Invalidate();
                return;
            }

            // Hidden pair
            var hp = solver.FindHiddenPair();
            if (hp.HasValue)
            {
                CurrentHint = new HintDetails
                {
                    Type = HintType.Suggestion,
                    TargetCells = hp.Value.Item1,
                    Strategy = StrategyType.HiddenPair,
                    CandidateMask = hp.Value.Item2
                };
                Invalidate();
                return;
            }

            var pp = solver.FindPointingPairOrTriple();
            if (pp.HasValue)
            {
                CurrentHint = new HintDetails
                {
                    Type = HintType.Suggestion,
                    SourceCells = pp.Value.Item1,
                    TargetCells = pp.Value.Item2,
                    Strategy = StrategyType.PointingPairOrTriple,
                    CandidateMask = pp.Value.Item3
                };
                Invalidate();
                return;
            }

            // Naked triple
            var nt = solver.FindNakedTriple();
            if (nt.HasValue)
            {
                CurrentHint = new HintDetails
                {
                    Type = HintType.Suggestion,
                    SourceCells = nt.Value.Item1,
                    TargetCells = nt.Value.Item2,
                    Strategy = StrategyType.NakedTriple,
                    CandidateMask = nt.Value.Item3
                };
                Invalidate();
                return;
            }

            // Hidden triple
            var ht = solver.FindHiddenTriple();
            if (ht.HasValue)
            {
                CurrentHint = new HintDetails
                {
                    Type = HintType.Suggestion,
                    TargetCells = ht.Value.Item1,
                    Strategy = StrategyType.HiddenTriple,
                    CandidateMask = ht.Value.Item2
                };
                Invalidate();
                return;
            }

            var xw = solver.FindXWing();
            if (xw.HasValue)
            {
                CurrentHint = new HintDetails
                {
                    Type = HintType.Suggestion,
                    SourceCells = xw.Value.Item1,
                    TargetCells = xw.Value.Item2,
                    Strategy = StrategyType.XWing,
                    CandidateMask = xw.Value.Item3
                };
                Invalidate();
                return;
            }

            var yw = solver.FindYWing();
            if (yw.HasValue)
            {
                CurrentHint = new HintDetails
                {
                    Type = HintType.Suggestion,
                    SourceCells = yw.Value.Item1,
                    TargetCells = yw.Value.Item2,
                    Strategy = StrategyType.YWing,
                    CandidateMask = yw.Value.Item3
                };
                Invalidate();
                return;
            }

            var sf = solver.FindSwordfish();
            if (sf.HasValue)
            {
                CurrentHint = new HintDetails
                {
                    Type = HintType.Suggestion,
                    SourceCells = sf.Value.Item1,
                    TargetCells = sf.Value.Item2,
                    Strategy = StrategyType.Swordfish,
                    CandidateMask = sf.Value.Item3
                };
                Invalidate();
                return;
            }

            // null 
            CurrentHint = null;
            Invalidate();
        }


        /// <summary>
        /// Creates a complete, independent copy of the Sudoku grid.
        /// </summary>
        /// <param name="orig"> The original 2D array of Cells to copy.</param>
        /// <returns>A new 2D array containing cloned Cell objects.</returns>
        public Cell[,] CreateDeepCopy(Cell[,] orig)
        {
            int dimension = orig.GetLength(0);
            Cell[,] newArray = new Cell[dimension, dimension];

            for (int r = 0; r < dimension; r++)
            {
                for (int c = 0; c < dimension; c++)
                {
                    newArray[r, c] = orig[r, c].Clone();
                }
            }
            return newArray;
        }

        /// <summary>
        /// Generates a bitmask representation of the user's candidate notations for each cell in the grid.
        /// </summary>
        /// <returns>A 9x9 array where each integer is a bitmask representing candidates.</returns>
        private int[,] GetUserCandidatesMask()
        {
            int[,] masks = new int[SUDOKU_DIMENSION, SUDOKU_DIMENSION];
            for (int r = 0; r < SUDOKU_DIMENSION; r++)
            {
                for (int c = 0; c < SUDOKU_DIMENSION; c++)
                {
                    if (Grid.GridArray[r, c].Value != 0)
                    {
                        masks[r, c] = 0;
                    }
                    else
                    {
                        int m = 0;
                        if (Grid.GridArray[r, c].Notations.Count == 0)
                        {
                            m = 511; // 111 111 111
                        }
                        else
                        {
                            foreach (int n in Grid.GridArray[r, c].Notations)
                            {
                                m |= (1 << (n - 1));
                            }
                        }
                        masks[r, c] = m;
                    }
                }
            }
            return masks;
        }
    }
}