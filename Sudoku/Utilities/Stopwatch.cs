using System;
using System.Diagnostics;

namespace Sudoku_Namespace.Utility
{
    /// <summary>
    /// Provides a stopwatch for tracking elapsed time during gameplay.
    /// </summary>
    public class GameStopwatch
    {
        private readonly Stopwatch _internalTimer = new Stopwatch();
        public TimeSpan LoadedOffset { get; set; } = TimeSpan.Zero;
        public void Start() => _internalTimer.Start();
        public void Stop() => _internalTimer.Stop();
        public void Reset()
        {
            _internalTimer.Reset();
            LoadedOffset = TimeSpan.Zero;
        }

        public TimeSpan TotalElapsed => _internalTimer.Elapsed + LoadedOffset;

        /// <summary>
        /// Returns the elapsed time formatted as a string in "HH:mm:ss" format.
        /// </summary>
        public string GetFormattedTime()
        {
            return String.Format("{0:00}:{1:00}:{2:00}",
                TotalElapsed.Hours, TotalElapsed.Minutes, TotalElapsed.Seconds);
        }
    }
}
