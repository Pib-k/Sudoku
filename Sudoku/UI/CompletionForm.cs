using Sudoku_Namespace.Rendering;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sudoku_Namespace.UI
{
    /// <summary>
    /// A base class that handles the common rounded styling, header, and painting logic.
    /// </summary>
    public class CompletionForm : BaseDialogForm
    {
        public CompletionForm(Size parentSize, ColorTheme theme, string fontName, TimeSpan timeElapsed, string difficultyText) :
            base(parentSize, theme, fontName)
        {
            InitializeHeader("Sudoku Completed");
            BuildContent(timeElapsed, difficultyText);
        }
        private void BuildContent(TimeSpan timeElapsed, string difficulty)
        {
            Panel content = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(
                    LEFT_PADDING,
                    TOP_PADDING + 30,
                    RIGHT_PADDING,
                    BOTTOM_PADDING),
                BackColor = Color.Transparent
            };

            Label title = new Label
            {
                Text = "Congratulations on completing the Sudoku!",
                Font = new Font(UserFont, 16, FontStyle.Bold),
                ForeColor = Theme.TextColor,
                Height = 80,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label timeHeader = CreateHeaderLabel("Time Elapsed");
            Label timeValue = CreateValueLabel(timeElapsed.ToString(@"hh\:mm\:ss"));

            Label diffHeader = CreateHeaderLabel("Difficulty");
            Label diffValue = CreateValueLabel(difficulty);

            Button newGameButton = CreateBaseStyledButton("New Game");
            newGameButton.Dock = DockStyle.Bottom;
            newGameButton.Height = 45;

            newGameButton.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            content.Controls.Add(newGameButton);
            content.Controls.Add(diffValue);
            content.Controls.Add(diffHeader);
            content.Controls.Add(timeValue);
            content.Controls.Add(timeHeader);
            content.Controls.Add(title);

            Controls.Add(content);
        }

        private Label CreateHeaderLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font(UserFont, 12, FontStyle.Bold),
                ForeColor = Theme.TextColor,
                Height = 30,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private Label CreateValueLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font(UserFont, 14, FontStyle.Regular),
                ForeColor = Theme.TextColor,
                Height = 40,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

    }
}