using Sudoku_Namespace.UI;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Sudoku_Namespace.Rendering
{
    /// <summary>
    /// Represents a color palette setup used to style the application.
    /// </summary>
    public struct ColorTheme
    {
        public string Name;
        public Color BackgroundColor;
        public Color PrimaryColor;
        public Color HighlightColor;
        public Color TextColor;
        public Color UnfixedTextColor;
        public Color TargetColor;
        public Color SourceColor;
        public Color GridLineColor;
    }

    /// <summary>
    /// Handles the creation, management, and application of Color Themes.
    /// </summary>
    public class ThemeManager
    {
        /// <summary>
        /// The active theme.
        /// </summary>
        public static ColorTheme CurrentTheme;

        /// <summary>
        /// A list of all available themes.
        /// </summary>
        public static List<ColorTheme> Themes = new List<ColorTheme>
        {
            new ColorTheme
            {
                Name = "Default",
                BackgroundColor = Color.White,
                PrimaryColor = Color.LightBlue,
                HighlightColor = Color.FromArgb(100, ControlPaint.Light(Color.LightBlue)),
                TextColor = Color.Black,
                UnfixedTextColor = Color.Gray,
                TargetColor = Color.Red,
                SourceColor = Color.LightGreen,
                GridLineColor = Color.Black
            },
            new ColorTheme
            {
                Name = "Sol",
                BackgroundColor = Color.FromArgb(253, 246, 227),   // Creamy white
                PrimaryColor = Color.FromArgb(238, 232, 213),      // Beige
                HighlightColor = Color.FromArgb(100, 181, 137, 0), // Light Orange highlight
                TextColor = Color.FromArgb(101, 123, 131),         // Gray-Green text
                UnfixedTextColor = Color.FromArgb(38, 139, 210),   // Blue
                TargetColor = Color.FromArgb(220, 50, 47),          // Light Red
                SourceColor = Color.ForestGreen,
                GridLineColor = Color.FromArgb(147, 161, 161)      // Light Grey
            },
            new ColorTheme
            {
                Name = "Dark Modern",
                BackgroundColor = Color.FromArgb(32, 33, 36),
                PrimaryColor = Color.FromArgb(53, 59, 72),
                HighlightColor = Color.FromArgb(80, 100, 149, 237),
                TextColor = Color.WhiteSmoke,
                UnfixedTextColor = Color.LightSkyBlue,
                TargetColor = Color.LightCoral,
                SourceColor = Color.Tan,
                GridLineColor = Color.FromArgb(80, 80, 80)
            },
            new ColorTheme
            {
                Name = "Factory",
                BackgroundColor = Color.FromArgb(39, 40, 34),       // Dark Gray
                PrimaryColor = Color.FromArgb(62, 64, 54),          // Lighter Gray selection
                HighlightColor = Color.FromArgb(100, 253, 151, 31), // Translucent Orange
                TextColor = Color.FromArgb(248, 248, 242),          // Off-White
                UnfixedTextColor = Color.FromArgb(102, 217, 239),   // Cyan
                TargetColor = Color.FromArgb(249, 38, 114),         // Hot Pink
                SourceColor = Color.FromArgb(166, 226, 46),         // Vibrant Lime
                GridLineColor = Color.FromArgb(73, 72, 62)          // Subtle Grey
            },
            new ColorTheme
                {
                Name = "Atari Gray",
                BackgroundColor = Color.FromArgb(45, 45, 45),       // Console Gray
                PrimaryColor = Color.FromArgb(60, 60, 60),          // Panel Gray
                HighlightColor = Color.FromArgb(100, 255, 255, 255),// White Glow
                TextColor = Color.White,                            // White
                UnfixedTextColor = Color.FromArgb(0, 195, 227),     // Neon Blue
                TargetColor = Color.FromArgb(255, 69, 84),          // Neon Red
                SourceColor = Color.FromArgb(0, 195, 227),          // Neon Blue
                GridLineColor = Color.FromArgb(80, 80, 80)          // Gray
            },
            new ColorTheme
            {
                Name = "Midnight Purple",
                BackgroundColor = Color.FromArgb(30, 30, 46),       // Dark Purple
                PrimaryColor = Color.FromArgb(45, 45, 68),          // Purple
                HighlightColor = Color.FromArgb(100, 189, 147, 249),// Purpur Purple
                TextColor = Color.FromArgb(205, 214, 244),          // Light Gray
                UnfixedTextColor = Color.FromArgb(137, 180, 250),   // Tropic Light Blue
                TargetColor = Color.FromArgb(243, 139, 168),        // Soft Red
                SourceColor = Color.FromArgb(166, 227, 161),        // Pistachio
                GridLineColor = Color.FromArgb(88, 91, 112)         // Gray
            },
            new ColorTheme
            {
                Name = "Deep Ocean",
                BackgroundColor = Color.FromArgb(0, 25, 51),        // Abyss Blue
                PrimaryColor = Color.FromArgb(0, 51, 77),           // Deep Teal
                HighlightColor = Color.FromArgb(100, 0, 150, 200),  // Water Reflection
                TextColor = Color.FromArgb(200, 230, 255),          // Pale Water Blue
                UnfixedTextColor = Color.FromArgb(0, 255, 255),     // Cyan Bioluminescence
                TargetColor = Color.FromArgb(255, 127, 80),         // Coral
                SourceColor = Color.FromArgb(64, 224, 208),         // Turquoise
                GridLineColor = Color.FromArgb(0, 75, 100)          // Dark Teal
            }
        };

        /// <summary>
        /// Finds a theme by name and applies it to the specified form and board.
        /// </summary>
        /// <param name="themeName">The name of the theme to load.</param>
        /// <param name="form">The main Windows Form to color.</param>
        /// <param name="board">The Sudoku board control to color.</param>
        public void LoadTheme(string themeName, Form form, SudokuControl board)
        {
            ColorTheme selTheme = Themes.Find(t => t.Name == themeName);

            if (!string.IsNullOrEmpty(selTheme.Name))
            {
                CurrentTheme = selTheme;
                ApplyTheme(form, board);
            }
        }

        /// <summary>
        /// Applies a theme by name and applies it to the specified form and board.
        /// </summary>
        /// <param name="themeName">The name of the theme to load.</param>
        /// <param name="form"> The main Windows Form to apply background and control colors to.</param>
        /// <param name="board"> The custom SudokuControl to repaint.</param>
        public void ApplyTheme(Form form, SudokuControl board)
        {
            form.BackColor = CurrentTheme.BackgroundColor;
            ApplyColorsToCollection(form.Controls);

            if (board != null)
            {
                SudokuBoardRenderer newRenderer = new SudokuBoardRenderer();

                newRenderer.BackgroundColor = CurrentTheme.BackgroundColor;
                newRenderer.GridLineColor = CurrentTheme.GridLineColor;
                newRenderer.TextColor = CurrentTheme.TextColor;
                newRenderer.UnfixedTextColor = CurrentTheme.UnfixedTextColor;
                newRenderer.HighlightColor = CurrentTheme.HighlightColor;
                newRenderer.SourceColor = CurrentTheme.SourceColor;
                newRenderer.TargetColor = CurrentTheme.TargetColor;

                board.Renderer = newRenderer;
            }
        }

        /// <summary>
        /// Recursional iteration through a control collection to apply theme colors to Buttons, Panels, and Labels.
        /// </summary>
        /// <param name="controls">The collection of child controls to style.</param>
        private void ApplyColorsToCollection(Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                if (c is Button b)
                {
                    b.BackColor = CurrentTheme.PrimaryColor;
                    b.ForeColor = CurrentTheme.TextColor;
                }
                else if (c is Panel p)
                {
                    p.BackColor = CurrentTheme.BackgroundColor;
                }
                else if (c is Label l)
                {
                    l.ForeColor = CurrentTheme.TextColor;
                    l.BackColor = CurrentTheme.BackgroundColor;
                }

                if (c.HasChildren)
                {
                    ApplyColorsToCollection(c.Controls);
                }
            }
        }
    }
}

