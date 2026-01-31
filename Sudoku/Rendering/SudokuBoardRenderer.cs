using Sudoku_Namespace.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Sudoku_Namespace.Rendering
{

    /// <summary>
    /// Handles the graphical rendering of the Sudoku board, including the grid, numbers, highlights, conflicts, and hints.
    /// </summary>
    public class SudokuBoardRenderer
    {
        public Color BackgroundColor;
        public Color GridLineColor;
        public Color TextColor;
        public Color UnfixedTextColor;
        public Color HighlightColor;
        public Color TargetColor;
        public Color SourceColor;
        public string currentFont;

        /// <summary>
        /// An array of brushes corresponding to specific conflict types (colors).
        /// </summary>
        private readonly Brush[] conflictBrushes = new Brush[]
            {
                Brushes.Transparent, // placeholder for 0
                Brushes.LightCoral,
                Brushes.LightGreen,
                Brushes.Orange,
                Brushes.Violet,
                Brushes.Yellow,
                Brushes.Cyan,
                Brushes.Pink,
                Brushes.Gold,
                Brushes.Brown
             };

        /// <summary>
        /// Draws the complete Sudoku board state.
        /// </summary>
        /// <param name="g">The GDI+ Graphics surface to draw on.</param>
        /// <param name="grid">The grid data model containing values and notations.</param>
        /// <param name="size">The total pixel width/height of the board.</param>
        /// <param name="dimension">The number of cells per row/column.</param>
        /// <param name="x">The X-coordinate offset for the top-left corner.</param>
        /// <param name="y">The Y-coordinate offset for the top-left corner.</param>
        /// <param name="hlX">The column index of the currently selected cell.</param>
        /// <param name="hlY">The row index of the currently selected cell.</param>
        /// <param name="conflictType">A 2D array indicating conflict IDs for coloring specific cells.</param>
        /// <param name="solveMode">If true, hints are drawn; if false, manual conflicts are drawn.</param>
        /// <param name="paused">If true, hides the board and displays a "PAUSED" overlay.</param>
        /// <param name="currentHint">The current hint details to visualize.</param>
        public void Draw(Graphics g, CoreGrid grid, int size, int dimension, int x, int y, int hlX, int hlY, int[,] conflictType, bool solveMode, bool paused, HintDetails currentHint)
        {
            int cellSize = size / dimension;

            if (paused) // Do not show board if game paused
            {
                using (SolidBrush b = new SolidBrush(BackgroundColor))
                using (Font f = new Font(currentFont, 24, FontStyle.Bold))
                using (Brush textBrush = new SolidBrush(TextColor))
                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.FillRectangle(b, x, y, size, size);
                    g.DrawString("GAME PAUSED", f, textBrush,
                        new RectangleF(x, y, size, size), sf);
                }
                return;
            }

            DrawBackground(g, size, x, y);
            DrawHighlights(g, size, x, y, cellSize, hlX, hlY);
            DrawConflicts(g, conflictType, x, y, dimension, cellSize, solveMode);
            DrawHint(g, x, y, cellSize, solveMode, currentHint);
            DrawNumbers(g, grid, size, dimension, x, y, cellSize);
            DrawGrid(g, size, x, y, cellSize);
        }

        private void DrawBackground(Graphics g, int size, int x, int y)
        {
            using (SolidBrush brush = new SolidBrush(BackgroundColor))
            {
                g.FillRectangle(brush, x, y, size, size);
            }
        }

        private void DrawGrid(Graphics g, int size, int x, int y, int cellSize)
        {
            using (Pen thin = new Pen(GridLineColor, 1))
            using (Pen thick = new Pen(GridLineColor, 3))
            {
                for (int i = 0; i <= 9; i++)
                {
                    Pen p = i % 3 == 0 ? thick : thin;
                    g.DrawLine(p, x + i * cellSize, y, x + i * cellSize, y + size);
                    g.DrawLine(p, x, y + i * cellSize, x + size, y + i * cellSize);
                }
            }
        }

        private void DrawHighlights(Graphics g, int size, int x, int y, int cellSize, int hlX, int hlY)
        {
            using (SolidBrush hl = new SolidBrush(HighlightColor))
            using (SolidBrush cellHl = new SolidBrush(Color.FromArgb(100, ControlPaint.Dark(HighlightColor, 0.1f))))
            {
                g.FillRectangle(hl, x + hlX * cellSize, y, cellSize, size); // Col
                g.FillRectangle(hl, x, y + hlY * cellSize, size, cellSize); // Row
                g.FillRectangle(cellHl, x + hlX * cellSize, y + hlY * cellSize, cellSize, cellSize); // Cell
            }
        }

        private void DrawNumbers(Graphics g, CoreGrid grid, int size, int dimension, int x, int y, int cellSize)
        {
            float fontSize = Math.Max(8f, cellSize * 0.55f);
            float smallFontSize = Math.Max(6f, cellSize / 3f * 0.45f);

            using (Font font = new Font(currentFont, fontSize, FontStyle.Regular))
            using (Font smallFont = new Font(currentFont, smallFontSize, FontStyle.Regular))
            using (Brush fixedBrush = new SolidBrush(TextColor))
            using (Brush unfixedBrush = new SolidBrush(UnfixedTextColor))
            using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                for (int r = 0; r < dimension; r++)
                {
                    for (int c = 0; c < dimension; c++)
                    {
                        int value = grid.GetValue(r, c);

                        if (value != 0)
                        {
                            bool isFixed = grid.IsFixed(r, c);
                            Brush brushToUse = isFixed ? fixedBrush : unfixedBrush;

                            string text = value.ToString();
                            RectangleF cellRect = new RectangleF(x + c * cellSize, y + r * cellSize, cellSize, cellSize);

                            g.DrawString(text, font, brushToUse, cellRect, sf);
                        }
                        else // Draws notations when there is no number in the cell.
                        {
                            List<int> notations = grid.GridArray[r, c].Notations;

                            if (notations.Count > 0)
                            {
                                float subCellSize = cellSize / 3f;
                                foreach (int val in notations)
                                {
                                    int notationRow = (val - 1) / 3; // makes 9 mini rectanlges 
                                    int notationCol = (val - 1) % 3;

                                    RectangleF subCellRect = new RectangleF(
                                        x + c * cellSize + notationCol * subCellSize,
                                        y + r * cellSize + notationRow * subCellSize,
                                        subCellSize,
                                        subCellSize
                                    );

                                    g.DrawString(val.ToString(), smallFont, unfixedBrush, subCellRect, sf); // draws the number centralised based on that rectangle
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DrawConflicts(Graphics g, int[,] conflictTypes, int x, int y, int dimension, int cellSize, bool solveMode)
        {
            if (solveMode)
            {
                return;
            }

            for (int r = 0; r < dimension; r++)
            {
                for (int c = 0; c < dimension; c++)
                {
                    int type = conflictTypes[r, c];
                    if (type != 0 && type < conflictBrushes.Length)
                    {
                        g.FillRectangle(conflictBrushes[type], x + c * cellSize, y + r * cellSize, cellSize, cellSize);
                    }
                }
            }
        }

        private void DrawHint(Graphics g, int x, int y, int cellSize, bool solveMode, HintDetails hint)
        {
            if (hint == null || hint.Type == HintType.None)
            {
                return;
            }

            if (!solveMode)
            {
                return;
            }

            if (hint.Type == HintType.Error)
            {
                using (SolidBrush errorBrush = new SolidBrush(Color.FromArgb(180, TargetColor)))
                {
                    foreach (Point p in hint.TargetCells)
                    {
                        g.FillRectangle(errorBrush, x + p.X * cellSize, y + p.Y * cellSize, cellSize, cellSize);
                    }
                }
                return;
            }
            if (hint.Type == HintType.Suggestion)
            {
                using (SolidBrush sourceBrush = new SolidBrush(Color.FromArgb(100, SourceColor)))
                {
                    if (hint.SourceCells != null)
                    {
                        foreach (Point p in hint.SourceCells)
                        {
                            g.FillRectangle(sourceBrush, x + p.X * cellSize, y + p.Y * cellSize, cellSize, cellSize);
                        }
                    }
                }
                using (SolidBrush targetBrush = new SolidBrush(Color.FromArgb(100, TargetColor)))
                {
                    foreach (Point p in hint.TargetCells)
                    {
                        g.FillRectangle(targetBrush, x + p.X * cellSize, y + p.Y * cellSize, cellSize, cellSize);
                    }
                }
            }
        }
    }
}
