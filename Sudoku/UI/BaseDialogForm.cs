using Sudoku_Namespace.Rendering;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sudoku_Namespace.UI
{
    /// <summary>
    /// A base class that handles the common rounded styling, header, and painting logic.
    /// </summary>
    public abstract class BaseDialogForm : Form
    {
        protected ColorTheme Theme { get; private set; }
        protected string UserFont { get; private set; }

        protected const int FORM_CORNER_RADIUS = 30;
        protected const int BUTTON_CORNER_RADIUS = 10;
        protected const int LEFT_PADDING = 40;
        protected const int RIGHT_PADDING = 40;
        protected const int TOP_PADDING = 60;
        protected const int BOTTOM_PADDING = 40;

        public BaseDialogForm(Size parentSize, ColorTheme theme, string fontName)
        {
            this.Theme = theme;
            this.UserFont = fontName;

            int formSize = Math.Min(parentSize.Height / 2, 450);
            this.Size = new Size(formSize, formSize + 50);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.DoubleBuffered = true;
            this.BackColor = theme.BackgroundColor;
        }

        /// <summary>
        /// Sets up the top header with Title and Close button.
        /// </summary>
        protected void InitializeHeader(string titleText)
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent,
            };

            Label title = new Label
            {
                Text = titleText,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Theme.TextColor,
                AutoSize = true,
                Location = new Point(LEFT_PADDING, 30),
                BackColor = Color.Transparent
            };

            Panel closeHost = new Panel
            {
                Dock = DockStyle.Right,
                Width = 70,
                BackColor = Color.Transparent
            };

            Button closeBtn = new Button
            {
                Text = "×",
                Size = new Size(28, 28),
                Location = new Point(0, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Theme.TextColor,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };

            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, Theme.TextColor);
            closeBtn.FlatAppearance.MouseDownBackColor = Color.Transparent;

            // Add rounded hover effect
            closeBtn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddEllipse(0, 0, closeBtn.Width - 1, closeBtn.Height - 1);
                    closeBtn.Region = new Region(gp);
                }
            };

            closeBtn.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            closeHost.Controls.Add(closeBtn);
            header.Controls.Add(title);
            header.Controls.Add(closeHost);

            this.Controls.Add(header);
        }

        /// <summary>
        /// Creates a button with rounded styling and font.
        /// </summary>
        protected Button CreateBaseStyledButton(string text)
        {
            Button b = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font(UserFont, 12, FontStyle.Bold),
                BackColor = Theme.PrimaryColor,
                ForeColor = Theme.TextColor
            };

            b.FlatAppearance.BorderSize = 0;
            b.SizeChanged += (s, e) =>
            {
                b.Region = new Region(GetRoundedRegionPath(b.ClientRectangle, BUTTON_CORNER_RADIUS));
            };

            return b;
        }

        /// <summary>
        /// Painting logic for the rounded border.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

            using (GraphicsPath path = GetRoundedRegionPath(rect, FORM_CORNER_RADIUS))
            {
                this.Region = new Region(path);

                using (Pen pen = new Pen(Theme.GridLineColor, 2))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        protected GraphicsPath GetRoundedRegionPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            if (rect.Width <= diameter || rect.Height <= diameter) return new GraphicsPath();

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}