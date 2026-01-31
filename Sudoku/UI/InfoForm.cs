using Sudoku_Namespace.Rendering;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Sudoku_Namespace.UI
{
    /// <summary>
    /// A form that displays text in a stylized way.
    /// </summary>
    public class InfoForm : Form
    {
        private FlowLayoutPanel contentPanel;
        private ColorTheme theme;
        private string mainFontName;
        private List<Control> textControls = new List<Control>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoForm"/> class.
        /// </summary>
        /// <param name="title">The text to display in the window's title bar.</param>
        /// <param name="fontName">The name of the font to use for text rendering.</param>
        /// <param name="currentTheme">The color theme containing background and foreground colors.</param>
        public InfoForm(string title, string fontName, ColorTheme currentTheme)
        {
            this.mainFontName = fontName ?? "Segoe UI";
            this.theme = currentTheme;
            InitializeComponent();
            WriteManual();
        }

        private void InitializeComponent()
        {
            this.Text = "Sudoku Manual & Help";
            this.Size = new Size(550, 750);
            this.MinimumSize = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = theme.BackgroundColor;

            contentPanel = new FlowLayoutPanel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.AutoScroll = true;
            contentPanel.FlowDirection = FlowDirection.TopDown;
            contentPanel.WrapContents = false;
            contentPanel.Padding = new Padding(30);
            contentPanel.BackColor = theme.BackgroundColor;

            contentPanel.SizeChanged += (s, e) => WrapText();

            this.Controls.Add(contentPanel);
        }

        private void WrapText()
        {
            contentPanel.SuspendLayout();
            int scrollBarWidth = SystemInformation.VerticalScrollBarWidth;
            int newWidth = contentPanel.ClientSize.Width - contentPanel.Padding.Left - contentPanel.Padding.Right - scrollBarWidth;

            foreach (Control c in textControls)
            {
                c.Width = newWidth;

                if (c is Label)
                {
                    c.MaximumSize = new Size(newWidth, 0);
                }
            }
            contentPanel.ResumeLayout();
        }

        private void WriteManual()
        {
            contentPanel.SuspendLayout();

            AddTitle("Sudoku Manual");
            AddText("This manual covers controls, game modes, and solving techniques.");
            AddSeparator();

            AddHeading("1. Controls && Keybinds");

            AddSubHeading("layout");
            AddBullet("Info button", "Opens this user manual.");
            AddBullet("Start/Pause button", "Pauses or resumes the game.");
            AddBullet("Load button", "Allows user to load .txt or .json sudokus.");
            AddBullet("Save button", "Allows user to save .txt sudokus.");
            AddBullet("Save as button", "Allows user to save .txt or .json sudokus.");
            AddBullet("Settings button", "Opens settings window, allowing users to change theme, font and toggle hint explanations.");
            AddBullet("Solve button", "Solves current displayed sudoku.");
            AddBullet("Reset button", "Resets all user altered cells in current game mode.");
            AddBullet("New game button", "Opens new game menu, allowing user to generate a sudoku of preferred difficulty.");
            AddBullet("Left Mouse click", "Selects a cell.");

            AddSubHeading("Keybinds (Also have a respective button)");
            AddBullet("Arrow keys", "Move the selected cell.");
            AddBullet("Keys 1 to 9", "Enter number in selected cell.");
            AddBullet("Backspace key or 0 key", "Removes the value from selected cell.");
            AddBullet("N key", "Toggle notation mode.");
            AddBullet("Ctrl + U", "Undo most recent action.");
            AddBullet("Ctrl + R", "Redo most recent action.");

            AddText("Starting numbers of a puzzle are shown in a different color than numbers entered by the user so that fixed values and editable values are clearly distinguishable.");
            AddSeparator();


            AddHeading("2. Game Modes");

            AddSubHeading("Solve Mode");
            AddText("Solve Mode is used to solve a Sudoku puzzle. The timer runs automatically while playing. You can enter numbers, use hints, and undo or redo actions. The program validates moves according to Sudoku rules. When the puzzle is completed correctly, a completion window is shown.");

            AddSubHeading("Build Mode");
            AddText("Build Mode is used to create your own Sudoku puzzle. You can freely enter starting numbers and save the puzzle. You can later switch to Play Mode to solve it. The current mode is stored in the save file together with time and difficulty.");
            AddSeparator();

            AddHeading("3. Solving Techniques");
            AddBullet("Naked Single", "A cell has only one possible candidate remaining, so that value must be placed there.");
            AddBullet("Hidden Single", "A number can only fit in one cell within a row, column, or box, even if the cell has other candidates.");
            AddBullet("Naked Pair", "Two cells in the same row, column, or box contain the same two candidates, allowing those candidates to be removed from other cells in that unit.");
            AddBullet("Hidden Pair", "Two candidates appear only in the same two cells within a unit, so all other candidates can be removed from those cells.");
            AddBullet("Pointing Pair / Triple", "When a candidate in a box is confined to a single row or column, it can be removed from the rest of that row or column outside the box.");
            AddBullet("Naked Triple", "Three cells share exactly three candidates in a unit, allowing those candidates to be removed from other cells in that unit.");
            AddBullet("Hidden Triple", "Three candidates appear only in the same three cells within a unit, so all other candidates can be removed from those cells.");
            AddBullet("X-Wing", "A candidate appears in exactly two cells in two different rows and the same columns (or vice versa), allowing elimination of that candidate from other cells in those columns or rows.");
            AddBullet("Y-Wing", "Three cells form a pivot pattern where eliminating a shared candidate is possible from cells that see both ends of the pattern.");
            AddBullet("Swordfish", "A candidate appears in exactly three rows and the same three columns, allowing elimination of that candidate from other cells in those columns or rows.");
            AddSeparator();

            AddHeading("4. Hints");
            AddText("The program provides hints to guide players when solving a Sudoku puzzle. Hints suggest moves or highlight conflicts based on the logical solving strategies listed in the 'Solving Techniques' section.");
            AddSubHeading("Hint Categories");
            AddBullet("None", "No hint is available for the current selection.");
            AddBullet("Conflict", "Indicates a rule violation or conflict in the current puzzle state.");
            AddBullet("Suggestion", "Suggests a move or candidate elimination based on solving strategies, helping you progress logically.");
            AddSubHeading("Hint Details");
            AddText("When a hint is displayed, it indicates:");
            AddBullet("Target Cells", "Cells affected by the hint or strategy.");
            AddBullet("Source Cells", "Cells that form the basis of the hint, such as part of a pair, triple, or pattern.");
            AddBullet("Candidates", "Numbers relevant to the hint, either as forced values or eliminations.");
            AddBullet("Explanation", "A description of the logic behind the hint, explaining why a number must be placed or a candidate removed.");
            AddText("The hints automatically use the solving strategies already described. Each hint provides a textual explanation tailored to the current board state, helping you understand the reasoning.");
            AddText("Hints are available in Solve Mode. They can be used to assist learning and to progress when a puzzle becomes challenging. The explanations indicate both the affected cells and the reasoning behind candidate eliminations or forced placements, making it easier to follow the logic of each move.");
            AddSeparator();

            contentPanel.ResumeLayout();
        }

        private void AddTitle(string text)
        {
            Label l = new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font(mainFontName, 24, FontStyle.Bold),
                ForeColor = theme.TextColor,
                Margin = new Padding(0, 0, 0, 20)
            };
            contentPanel.Controls.Add(l);
            textControls.Add(l);
        }

        private void AddHeading(string text)
        {
            Label l = new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font(mainFontName, 16, FontStyle.Bold),
                ForeColor = theme.TextColor,
                Margin = new Padding(0, 20, 0, 10)
            };
            contentPanel.Controls.Add(l);
            textControls.Add(l);
        }

        private void AddSubHeading(string text)
        {
            Label l = new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font(mainFontName, 12, FontStyle.Bold),
                ForeColor = theme.TextColor,
                Margin = new Padding(0, 10, 0, 5)
            };
            contentPanel.Controls.Add(l);
            textControls.Add(l);
        }

        private void AddText(string text)
        {
            Label l = new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font(mainFontName, 11, FontStyle.Regular),
                ForeColor = theme.TextColor,
                Margin = new Padding(0, 0, 0, 10)
            };
            contentPanel.Controls.Add(l);
            textControls.Add(l);
        }

        private void AddBullet(string boldPrefix, string text)
        {
            Label l = new Label
            {
                Text = $"• {boldPrefix}: {text}",
                AutoSize = true,
                Font = new Font(mainFontName, 11, FontStyle.Regular),
                ForeColor = theme.TextColor,
                Margin = new Padding(10, 0, 0, 5)
            };
            contentPanel.Controls.Add(l);
            textControls.Add(l);
        }

        private void AddSeparator()
        {
            Panel p = new Panel
            {
                Height = 1,
                BackColor = theme.TextColor,
                Margin = new Padding(0, 15, 0, 15),
                Width = contentPanel.ClientSize.Width - 60
            };
            contentPanel.Controls.Add(p);
            textControls.Add(p);
        }
    }
}