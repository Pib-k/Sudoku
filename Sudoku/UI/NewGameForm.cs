using Sudoku_Namespace.Rendering;
using Sudoku_Namespace.Utilities;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sudoku_Namespace.UI
{
    public class NewGameForm : BaseDialogForm
    {
        public DifficultyLevel Difficulty { get; private set; }

        public NewGameForm(Size size, ColorTheme currentTheme, string userFont)
            : base(size, currentTheme, userFont)
        {
            int formSize = Math.Min(size.Height / 2, 400);
            this.Size = new Size(formSize, formSize + 50);

            InitializeHeader("NEW GAME");
            CreateDifficultyButtons();
        }

        private void CreateDifficultyButtons()
        {
            TableLayoutPanel tlp = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Padding = new Padding(LEFT_PADDING, TOP_PADDING + 30, RIGHT_PADDING, BOTTOM_PADDING),
                BackColor = Color.Transparent
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            foreach (DifficultyLevel diff in Enum.GetValues(typeof(DifficultyLevel)))
            {
                if (diff == DifficultyLevel.Solved) continue;

                tlp.RowCount++;
                tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

                Button diffBtn = base.CreateBaseStyledButton(diff.ToString());
                diffBtn.Dock = DockStyle.Fill;
                diffBtn.Margin = new Padding(0, 5, 0, 5);
                diffBtn.Tag = diff;

                diffBtn.Click += (s, e) =>
                {
                    Difficulty = (DifficultyLevel)((Control)s).Tag;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };

                tlp.Controls.Add(diffBtn);
            }

            this.Controls.Add(tlp);
        }
    }
}