using Sudoku_Namespace.Algorithms;
using Sudoku_Namespace.IO;
using Sudoku_Namespace.Models;
using Sudoku_Namespace.Rendering;
using Sudoku_Namespace.Utilities;
using Sudoku_Namespace.Utility;
using Suudoku.Models;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sudoku_Namespace.UI
{
    /// <summary>
    /// Manages the building of the layout of the Sudoku User Interface.
    /// Handles control positioning, scaling, and event wiring.
    /// </summary>
    internal class LayoutBuilder
    {
        private SudokuControl board;
        public Form form;
        private string currentFilePath = null;

        public Label TimerLabel;
        private TableLayoutPanel bottomTLP;
        private TableLayoutPanel numbersTLP;
        private TableLayoutPanel utilityTLP;
        private TableLayoutPanel topTLP;
        private Button StartButton;
        private Label difficultyLabel;

        private Button noteButton;
        private Button settingsButton;
        private Button undoButton;
        private Button redoButton;
        private Button hintButton;
        private Button eraseButton;

        private string userSelectedFont = "Segoe UI Emoji";
        private bool ShowHintExplanation = true;
        private bool showAnimatedSolve = true;
        private Label hintExplanationLabel;

        private const int CORNER_RADIUS = 10;
        private const int MARGIN = 20;
        private const int TOP_PANEL_HEIGHT = 40;
        private const int BOTTOM_PANEL_HEIGHT = 50;
        private ThemeManager themeManager = new ThemeManager();
        private ToolTip toolTip;

        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutBuilder"/> class.
        /// </summary>
        /// <param name="form">The main Windows Form.</param>
        /// <param name="board">The custom Sudoku control.</param>
        public LayoutBuilder(Form form, SudokuControl board)
        {
            this.form = form;
            this.board = board;

            form.MinimumSize = new Size(666, 666);
            toolTip = new ToolTip();

            CreateTopPanel();
            CreateBottomControlPanel();
            CreateRightSidePanel();

            board.StateChanged += UpdateUIState;
            board.HintChanged += UpdateHintLabel;
            board.NotationModeChanged += UpdateNotationButtonVisuals;
            board.CompletionAnimationFinished += OnCompletionAnimationFinished;
            themeManager.LoadTheme("Default", form, board);
            UpdateUIState(this, EventArgs.Empty);
        }

        private void UpdateUIState(object sender, EventArgs e)
        {
            switch (board.CurrentState)
            {
                case GameState.Building:
                    StartButton.Text = "Lock && Play";
                    difficultyLabel.Text = "Difficulty: Custom";
                    TimerLabel.Text = "00:00:00";
                    break;

                case GameState.Playing:
                    StartButton.Text = "Pause";
                    break;

                case GameState.Paused:
                    StartButton.Text = "Resume";
                    break;

                case GameState.Animating:
                    StartButton.Text = "Solving...";
                    break;

                case GameState.Won:
                    StartButton.Text = "Pause";
                    break;
            }

            bool isInteractive = (board.CurrentState == GameState.Building || board.CurrentState == GameState.Playing);

            numbersTLP.Enabled = isInteractive;
            undoButton.Enabled = isInteractive;
            redoButton.Enabled = isInteractive;
            eraseButton.Enabled = isInteractive;
            noteButton.Enabled = isInteractive;
            hintButton.Enabled = (board.CurrentState == GameState.Playing);
            settingsButton.Enabled = (board.CurrentState != GameState.Animating);
            form.Cursor = board.CurrentState == GameState.Animating ? Cursors.WaitCursor : Cursors.Default;
        }

        /// <summary>
        /// Rescales buttons and board positions based on the form size.
        /// Ensures the UI remains responsive and centered.
        /// </summary>
        public void Rescale(object o, EventArgs e)
        {
            form.SuspendLayout();

            int height = form.ClientSize.Height - TOP_PANEL_HEIGHT - BOTTOM_PANEL_HEIGHT - (MARGIN * 4);
            int width = form.ClientSize.Width - (MARGIN * 3);

            int rightPanelWidth = Math.Max(140, Math.Min(form.ClientSize.Width / 4, 250));

            // Board calculations
            int maxBoardWidth = width - rightPanelWidth;
            board.BoardSize = (Math.Min(height, maxBoardWidth) / 9) * 9;

            // Centering
            int totalGameWidth = board.BoardSize + MARGIN + rightPanelWidth;
            int centeredLeft = (form.Width - totalGameWidth) / 2;
            if (centeredLeft < MARGIN) centeredLeft = MARGIN;

            // Set positions
            board.BoardLeft = centeredLeft;
            board.BoardTop = TOP_PANEL_HEIGHT + MARGIN;

            // Top Panel
            topTLP.Location = new Point(board.BoardLeft, 10);
            topTLP.Size = new Size(board.BoardSize, 40);

            // Bottom Panel
            bottomTLP.Location = new Point(board.BoardLeft, board.BoardTop + board.BoardSize + MARGIN);
            bottomTLP.Size = new Size(board.BoardSize, BOTTOM_PANEL_HEIGHT);

            int rightPanelX = board.BoardLeft + board.BoardSize + MARGIN;
            utilityTLP.Location = new Point(rightPanelX, board.BoardTop);
            int buttonSideSize = rightPanelWidth / 3;

            // Row 0 & 2 are Buttons, Row 1 & 3 are Labels.
            if (utilityTLP.RowStyles.Count >= 4)
            {
                utilityTLP.RowStyles[0] = new RowStyle(SizeType.Absolute, buttonSideSize);
                utilityTLP.RowStyles[1] = new RowStyle(SizeType.AutoSize);
                utilityTLP.RowStyles[2] = new RowStyle(SizeType.Absolute, buttonSideSize);
                utilityTLP.RowStyles[3] = new RowStyle(SizeType.AutoSize);
            }

            utilityTLP.Size = new Size(rightPanelWidth, (buttonSideSize * 2) + 40);

            int numberPadY = utilityTLP.Bottom + MARGIN;
            numbersTLP.Location = new Point(rightPanelX, numberPadY);
            numbersTLP.Size = new Size(rightPanelWidth, rightPanelWidth);

            // Hint Explanation label

            hintExplanationLabel.Location = new Point(numbersTLP.Left, numbersTLP.Bottom + MARGIN);
            hintExplanationLabel.MaximumSize = new Size(numbersTLP.Width, 0);

            hintExplanationLabel.Font = new Font(userSelectedFont, 12, FontStyle.Bold);
            float bottomPanelFontSize = Math.Max(10, Math.Min(board.BoardSize / 45f, 16));
            float rightPanelFontSize = Math.Max(10, rightPanelWidth / 10f);
            float utilityLabelSize = Math.Max(6, rightPanelWidth / 25f);

            UpdatePanelStyle(topTLP, 12);
            UpdatePanelStyle(bottomTLP, bottomPanelFontSize);
            UpdatePanelStyle(numbersTLP, rightPanelFontSize);
            UpdateUtilityPanelStyle(utilityTLP, rightPanelFontSize, utilityLabelSize);

            board.Invalidate();
            form.ResumeLayout(true);
        }

        /// <summary>
        /// Updates font size and applies rounded regions for buttons in a panel.
        /// </summary>
        private void UpdatePanelStyle(TableLayoutPanel panel, float fontSize)
        {
            foreach (Control control in panel.Controls)
            {
                control.Font = new Font(userSelectedFont, fontSize, FontStyle.Bold);
                if (control is Button)
                {
                    control.Region = GetRoundedRegion(control.ClientRectangle, CORNER_RADIUS);
                }
            }
        }

        /// <summary>
        /// Updates the labels under the utility buttons, and the buttons themselves.
        /// </summary>
        private void UpdateUtilityPanelStyle(TableLayoutPanel panel, float buttonFontSize, float labelFontSize)
        {
            foreach (Control control in panel.Controls)
            {
                if (control is Button)
                {
                    control.Font = new Font(userSelectedFont, buttonFontSize, FontStyle.Bold);
                    control.Region = GetRoundedRegion(control.ClientRectangle, CORNER_RADIUS);
                }
                else if (control is Label)
                {
                    control.Font = new Font(userSelectedFont, labelFontSize, FontStyle.Regular);
                }
            }
        }

        /// <summary>
        /// Creates a standardized, styled button.
        /// </summary>
        private Button CreateStyledButton(string text, string caption)
        {
            Button b = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(3),
                BackColor = Color.LightBlue,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand,
                Region = GetRoundedRegion(new Rectangle(0, 0, 100, 50), 10)
            };

            toolTip.SetToolTip(b, caption);
            return b;
        }

        /// <summary>
        /// Creates the Top Panel containing Start/Timer and Load/Save controls.
        /// </summary>
        private void CreateTopPanel()
        {
            topTLP = new TableLayoutPanel
            {
                ColumnCount = 6,
                BackColor = Color.White,
                RowCount = 1
            };

            topTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8F));
            topTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            topTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26F));
            topTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            topTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            topTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));

            StartButton = CreateStyledButton("Start", "Start or stop the game timer.");
            StartButton.Margin = new Padding(1);
            StartButton.Click += OnStartButtonClick;

            TimerLabel = new Label
            {
                Text = "00:00:00",
                Margin = new Padding(1),
                BackColor = Color.Transparent,
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            Button loadButton = CreateStyledButton("Load", "Load a Sudoku game from a file.");
            loadButton.Margin = new Padding(1);
            loadButton.Click += OnLoadButtonClick;

            Button saveButton = CreateStyledButton("Save", "Save the current game to a file");
            saveButton.Margin = new Padding(1);
            saveButton.Click += OnSaveButtonClick;

            Button saveAsButton = CreateStyledButton("Save as", "Save the current game as a new file.");
            saveAsButton.Margin = new Padding(1);
            saveAsButton.Click += OnSaveAsButtonClick;

            Button infoButton = CreateStyledButton("\u24D8", "Show the program manual.");
            infoButton.Margin = new Padding(1);
            infoButton.Click += OnInfoButtonClick;

            topTLP.Controls.Add(StartButton, 1, 0);
            topTLP.Controls.Add(infoButton, 0, 0);
            topTLP.Controls.Add(TimerLabel, 2, 0);
            topTLP.Controls.Add(loadButton, 3, 0);
            topTLP.Controls.Add(saveButton, 4, 0);
            topTLP.Controls.Add(saveAsButton, 5, 0);

            form.Controls.Add(topTLP);
        }

        /// <summary>
        /// Creates the Bottom Panel with Difficulty info, Solve, Reset, and New Game buttons.
        /// </summary>
        private void CreateBottomControlPanel()
        {
            bottomTLP = new TableLayoutPanel()
            {
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.White
            };

            for (int i = 0; i < 4; i++)
            {
                bottomTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            }

            difficultyLabel = new Label
            {
                Text = "Difficulty: Easy",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = Color.Black
            };

            Button solveButton = new Button
            {
                Text = "Solve",
                Margin = new Padding(5),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                BackColor = Color.LightBlue,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand,
                Region = GetRoundedRegion(new Rectangle(0, 0, 100, 50), 10),
                Dock = DockStyle.Fill
            };

            solveButton.Click += OnSolveButtonClick;
            toolTip.SetToolTip(solveButton, "Reveals the solution for the current board.");

            Button ResetButton = new Button
            {
                Text = "Reset",
                Margin = new Padding(5),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                BackColor = Color.LightBlue,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand,
                Region = GetRoundedRegion(new Rectangle(0, 0, 100, 50), 10),
                Dock = DockStyle.Fill
            };

            ResetButton.Click += OnResetButtonClick;
            toolTip.SetToolTip(ResetButton, "Build Mode: Clears board.\nPlay Mode: Clears user input only.");

            Button newGameButton = new Button
            {
                Text = "New Game",
                Margin = new Padding(5),
                Font = new Font(userSelectedFont, 16, FontStyle.Bold),
                BackColor = Color.LightBlue,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand,
                Region = GetRoundedRegion(new Rectangle(0, 0, 100, 50), 10),
                Dock = DockStyle.Fill
            };
            newGameButton.Click += OnNewGameButtonClick;
            toolTip.SetToolTip(newGameButton, "Starts a new game.");

            bottomTLP.Controls.Add(difficultyLabel, 0, 0);
            bottomTLP.Controls.Add(solveButton, 1, 0);
            bottomTLP.Controls.Add(ResetButton, 2, 0);
            bottomTLP.Controls.Add(newGameButton, 3, 0);

            form.Controls.Add(bottomTLP);
        }


        /// <summary>
        /// Creates the right side panel containing the number pad and the utility buttons (Undo, Hint, etc.).
        /// </summary>
        private void CreateRightSidePanel()
        {
            utilityTLP = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 4,
                BackColor = Color.White,
            };

            utilityTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            utilityTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            utilityTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));

            utilityTLP.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            utilityTLP.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            utilityTLP.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            utilityTLP.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            undoButton = CreateStyledButton("\U000021BA", "Undo the last action.");
            undoButton.Click += (s, e) => { board.Undo(); board.Focus(); };

            redoButton = CreateStyledButton("\U000021BB", "Redo the previously undone action.");
            redoButton.Click += (s, e) => { board.Redo(); board.Focus(); };

            hintButton = CreateStyledButton("?", "Suggest the next move and technique.");
            hintButton.Click += (s, e) => OnHintButtonClick();

            settingsButton = CreateStyledButton("\u2699", "Configure themes and fonts.");
            settingsButton.Click += (s, e) => OnSettingsButtonClick();

            eraseButton = CreateStyledButton("\u232B", "Delete Cell Contents");
            eraseButton.Click += (s, e) => { board.Delete(); board.Focus(); };

            noteButton = CreateStyledButton("\u270E", "Toggle notation (pencil) mode.");
            noteButton.Click += (s, e) => OnNotationButtonClick();

            utilityTLP.Controls.Add(undoButton, 0, 0);
            utilityTLP.Controls.Add(redoButton, 1, 0);
            utilityTLP.Controls.Add(eraseButton, 2, 0);

            utilityTLP.Controls.Add(CreateUtilityLabel("Undo"), 0, 1);
            utilityTLP.Controls.Add(CreateUtilityLabel("Redo"), 1, 1);
            utilityTLP.Controls.Add(CreateUtilityLabel("Erase"), 2, 1);

            utilityTLP.Controls.Add(hintButton, 0, 2);
            utilityTLP.Controls.Add(noteButton, 1, 2);
            utilityTLP.Controls.Add(settingsButton, 2, 2);

            utilityTLP.Controls.Add(CreateUtilityLabel("Hint"), 0, 3);
            utilityTLP.Controls.Add(CreateUtilityLabel("Notes"), 1, 3);
            utilityTLP.Controls.Add(CreateUtilityLabel("Settings"), 2, 3);


            form.Controls.Add(utilityTLP);

            numbersTLP = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 3,
                BackColor = Color.White,
            };

            for (int i = 0; i < 3; i++) numbersTLP.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            for (int i = 0; i < 3; i++) numbersTLP.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));

            for (int i = 1; i <= 9; i++)
            {
                Button numberButton = CreateStyledButton(i.ToString(), $"Insert number {i}.");
                numberButton.Tag = i;
                int val = i;
                numberButton.Click += (s, e) => OnNumberButtonClick(val);

                int row = (i - 1) / 3;
                int col = (i - 1) % 3;
                numbersTLP.Controls.Add(numberButton, col, row);
            }
            form.Controls.Add(numbersTLP);

            hintExplanationLabel = CreateHintLabel();
            form.Controls.Add(hintExplanationLabel);
        }

        private Label CreateHintLabel()
        {
            return new Label
            {
                Text = "",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                ForeColor = Color.Black,
                Font = new Font(userSelectedFont, 12, FontStyle.Bold),
                Margin = new Padding(0)
            };
        }

        private void OnHintButtonClick()
        {
            board.ToggleHint();
            board.Focus();
        }
        private void OnNotationButtonClick()
        {
            board.NotationMode = !board.NotationMode;
            UpdateNotationButtonVisuals(null, null);
            board.Focus();
        }

        /// <summary>
        /// Opens the Settings dialog, allowing changes to theme, font, and hints.
        /// </summary>
        private void OnSettingsButtonClick()
        {
            if (board.CurrentState == GameState.Playing)
            {
                board.TogglePause();
            }

            using (var settingsForm = new SettingsForm(
                board.Size,
                ThemeManager.CurrentTheme,
                userSelectedFont,
                ShowHintExplanation,
                showAnimatedSolve))
            {
                // This blocks interaction with the main window until settings is closed
                if (settingsForm.ShowDialog(form) == DialogResult.OK)
                {
                    // Sync the values back to your main application variables
                    userSelectedFont = settingsForm.SelectedFont;
                    ShowHintExplanation = settingsForm.ShowHintExplanation;
                    showAnimatedSolve = settingsForm.AnimatedOn;

                    board.Renderer.currentFont = userSelectedFont;
                    themeManager.LoadTheme(settingsForm.UserSelectedTheme, form, board);

                    // Apply styles and themes
                    Rescale(null, null);
                    if (ShowHintExplanation && board.CurrentHint != null && board.CurrentHint.Strategy != StrategyType.None)
                    {
                        hintExplanationLabel.Text = "Hint: " + board.CurrentHint.GetExplanation();
                    }
                    else { hintExplanationLabel.Text = ""; }
                }
            }
        }

        private void OnNumberButtonClick(int number)
        {
            board.PlaceNumber(number);
            board.Focus();
        }

        /// <summary>
        /// Attempts to solve the current board. Shows an error message if the board is unsolvable.
        /// </summary>
        private async void OnSolveButtonClick(object sender, EventArgs e)
        {
            if (board.CurrentState != GameState.Playing) { return; }

            try
            {
                bool solved;
                if (showAnimatedSolve)
                {
                    solved = await board.SolveAnimated();
                }
                else
                {
                    solved = board.Solve();
                }
                if (solved)
                {
                    board.Invalidate();
                    board.Focus();
                    board.CheckForCompletion();
                }
                else
                {
                    MessageBox.Show(
                        "Sudoku not solvable",
                        "Solve Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                        );
                }
            }
            finally
            {
                board.Focus();
            }
        }

        /// <summary>
        /// Resets the current game state. Behavior depends on Play Mode vs Builder Mode.
        /// </summary>
        private void OnResetButtonClick(object sender, EventArgs e)
        {
            bool resetFull = (board.CurrentState == GameState.Building);
            board.Grid.Reset(!resetFull);

            if (board.CurrentState == GameState.Playing)
            {
                board.GameTimer.Reset();
                board.GameTimer.Start();
                TimerLabel.Text = "00:00:00";
            }
            board.WrongCells.Clear();
            board.CurrentHint = null;
            board.UpdateConflicts();
            board.Invalidate();
            board.Focus();
        }

        /// <summary>
        /// Opens the New Game dialog to select difficulty, or to build your own Sudoku.
        /// </summary>
        public void OnNewGameButtonClick(object sender, EventArgs e)
        {
            using (NewGameForm ngf = new NewGameForm(form.Size, ThemeManager.CurrentTheme, userSelectedFont))
            {
                var result = ngf.ShowDialog(form);

                if (result == DialogResult.OK)
                {
                    board.Grid.Reset(false);
                    DifficultyLevel difficulty = ngf.Difficulty;

                    if (difficulty == DifficultyLevel.Extreme)
                    {
                        MessageBox.Show(
                            "Extreme difficulty puzzles may require solving techniques not currently supported by the hint assistant or trial-and-error methods.",
                            "WARNING",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                    else if(difficulty == DifficultyLevel.Custom)
                    {
                        board.CurrentState = GameState.Building;
                        board.Invalidate();
                        board.Focus();
                        difficultyLabel.Text = "Difficulty: " + difficulty.ToString();
                        return;
                    }

                    board.NewGame(difficulty);
                    currentFilePath = null;
                    TimerLabel.Text = "00:00:00";
                    difficultyLabel.Text = "Difficulty: " + difficulty.ToString();
                }
            }
        }

        /// <summary>
        /// Toggles the game timer (Pause/Resume) or locks a Builder Mode board to start playing.
        /// </summary>
        public void OnStartButtonClick(object sender, EventArgs e)
        {
            if (board.CurrentState == GameState.Building)
            {
                BitmaskSolver solver = new BitmaskSolver((int[,])board.Grid.GetValueArray().Clone());
                if (!solver.InitializeBitmaskGrids() || !solver.HasUniqueSolution())
                {
                    MessageBox.Show(
                        "The current Sudoku board does not have a unique or valid solution. Please adjust the board before starting the game.",
                        "Invalid Sudoku",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }
                board.RecalculateSolution();
                DifficultyAnalyzer analyzer = new DifficultyAnalyzer();
                DifficultyLevel? difficulty = analyzer.EvaluateDifficulty((int[,])board.Grid.GetValueArray().Clone());
                difficultyLabel.Text = "Difficulty: " + difficulty.ToString();
                board.CurrentState = GameState.Playing;
            }
            else
            {
                board.TogglePause();
            }
            board.Invalidate();
            board.Focus();
        }


        /// <summary>
        /// Opens a file dialog to select a save file.
        /// </summary>
        private void OnLoadButtonClick(object sender, EventArgs e)
        {
            if (board.CurrentState == GameState.Playing)
            {
                board.TogglePause();
            }
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "TextFiles|*.txt|JSON files|*.json|All Files|*.*";
                dialog.Title = "Load Sudoku file";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        currentFilePath = dialog.FileName;
                        SudokuFileManager sfm = new SudokuFileManager();
                        GameSaveData data = sfm.Load(currentFilePath);

                        Cell[,] newGrid = new Cell[9, 9];
                        for (int r = 0; r < 9; r++)
                        {
                            for (int c = 0; c < 9; c++)
                            {
                                newGrid[r, c] = data.GridData[r][c];
                            }
                        }

                        board.LoadGame(newGrid, data.IsBuilderMode);
                        board.GameTimer.Reset();
                        board.GameTimer.LoadedOffset = TimeSpan.FromTicks(data.TimeElapsedTicks);
                        if (data.TimeElapsedTicks > 0)
                        {
                            TimeSpan ts = TimeSpan.FromTicks(data.TimeElapsedTicks);
                            TimerLabel.Text = ts.ToString(@"hh\:mm\:ss");
                        }
                        else
                        {
                            TimerLabel.Text = "00:00:00";
                        }

                        difficultyLabel.Text = "Difficulty: " + (data.IsBuilderMode ? "Custom" : data.Difficulty.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"failed to load sudoku:\n {ex.Message}",
                            "Load error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }
                }
            }
            board.Focus();
        }

        /// <summary>
        /// Saves the current game. If no file path exists, calls SaveAs.
        /// </summary>
        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                OnSaveAsButtonClick(sender, e);
                return;
            }
            DoSave(currentFilePath);
        }

        /// <summary>
        /// Opens a file dialog to save the game to a new file location.
        /// </summary>
        private void OnSaveAsButtonClick(object sender, EventArgs e)
        {
            if (board.CurrentState == GameState.Playing)
            {
                board.TogglePause();
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "TextFiles|*.txt|JSON files|*.json|All Files|*.*";
                dialog.Title = "Save Sudoku as..";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = dialog.FileName;
                    DoSave(currentFilePath);
                }
            }
            board.Focus();
        }

        private void DoSave(string path)
        {
            try
            {
                SudokuFileManager sfm = new SudokuFileManager();
                string difficultyText = difficultyLabel.Text.Replace("Difficulty: ", "");
                DifficultyLevel difficultyLevel;

                if (difficultyText == "Custom") difficultyLevel = DifficultyLevel.Custom;
                else Enum.TryParse(difficultyText, out difficultyLevel);

                bool isBuilder = (board.CurrentState == GameState.Building);

                sfm.Save(board.Grid, path, board.GameTimer.TotalElapsed, difficultyLevel, isBuilder);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"failed to save sudoku:\n {ex.Message}", "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens a window with information about the program
        /// </summary>
        private void OnInfoButtonClick(object sender, EventArgs e)
        {
            if (board.CurrentState == GameState.Playing)
            {
                board.TogglePause();
            }
            using (InfoForm infoForm = new InfoForm("manual", userSelectedFont, ThemeManager.CurrentTheme))
            {
                infoForm.ShowDialog(form);
            }
        }

        // Creates a rounded region for a button
        private Region GetRoundedRegion(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);             // top-left
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90); // top-right
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90); // bottom-right
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90); // bottom-left

            path.CloseFigure();
            return new Region(path);
        }

        /// <summary>
        /// Changes the notation button color to show whether user is in notation mode.
        /// </summary>
        public void UpdateNotationButtonVisuals(object sender, EventArgs ea)
        {
            if (board.NotationMode)
            {
                if (HighLuminance())
                {
                    noteButton.BackColor = ControlPaint.Dark(StartButton.BackColor);
                }
                else
                {
                    noteButton.BackColor = ControlPaint.Light(StartButton.BackColor);
                }
            }
            else
            {
                noteButton.BackColor = StartButton.BackColor;
            }
        }

        /// <summary>
        /// Helper to create the small labels under the utility buttons.
        /// </summary>
        private Label CreateUtilityLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                BackColor = Color.Transparent,
                ForeColor = Color.Black,
                Margin = new Padding(0),
                AutoSize = true
            };
        }

        private void UpdateHintLabel(object o, EventArgs e)
        {
            if (board.CurrentHint != null)
            {
                if (ShowHintExplanation)
                {
                    hintExplanationLabel.Text = "Hint: " + board.CurrentHint.GetExplanation();
                }
                else { hintExplanationLabel.Text = ""; }
            }
            else
            {
                hintExplanationLabel.Text = "No hint found.";
            }
        }

        private bool HighLuminance()
        {
            return (0.5 * 255 < board.BackColor.R * 0.2126 + board.BackColor.G * 0.7152 + board.BackColor.B * 0.0722);
        }

        private void OnCompletionAnimationFinished(object sender, EventArgs e)
        {
            string diffText = difficultyLabel.Text.Replace("Difficulty: ", "");
            DifficultyLevel difficulty = DifficultyLevel.Easy;
            Enum.TryParse(diffText, out difficulty);

            using (var completionForm = new CompletionForm(
                form.Size,
                ThemeManager.CurrentTheme,
                userSelectedFont,   
                board.GameTimer.TotalElapsed,
                difficulty.ToString()))
            {
                var result = completionForm.ShowDialog(form);

                if (result == DialogResult.OK)
                {
                    OnNewGameButtonClick(null, null);
                }
            }
        }

    }
}