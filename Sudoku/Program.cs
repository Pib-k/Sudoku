using Sudoku_Namespace.UI;
using System;
using System.Windows.Forms;

namespace Sudoku_Namespace
{
    internal class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.Run(new MainForm());
        }
    }
}