using Sudoku_Namespace.Utility;
using System;
using System.Windows.Forms;

namespace Sudoku_Namespace.UI
{
    public class MainForm : Form
    {
        private Timer uiTimer;
        private GameStopwatch stopwatch = new GameStopwatch();
        private SudokuControl board;
        private LayoutBuilder layout;

        public MainForm()
        {

            board = new SudokuControl();
            board.Dock = DockStyle.Fill;
            layout = new LayoutBuilder(this, board);

            this.Controls.Add(board);
            this.Text = "Sudoku Solver";
            this.Width = 800;
            this.Height = 800;
            this.WindowState = FormWindowState.Maximized;
            this.Resize += layout.Rescale;

            InitializeTimer();
            InitializeIcon();

            this.Load += MainForm_Load;
            layout.Rescale(this, null);
        }


        private void InitializeTimer()
        {
            uiTimer = new Timer();
            uiTimer.Interval = 1000;
            uiTimer.Tick += UiTimer_Tick;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            uiTimer.Start();
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            if (board != null && layout.TimerLabel != null)
            {
                layout.TimerLabel.Text = board.GameTimer.GetFormattedTime();
            }
        }

        private void InitializeIcon()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            SuspendLayout();
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            ResumeLayout(false);

        }


    }
}

