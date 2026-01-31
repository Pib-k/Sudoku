using Sudoku_Namespace.Models;
using Sudoku_Namespace.Utilities;
using System;
using System.IO;
using System.Text.Json;

namespace Sudoku_Namespace.IO
{
    /// <summary>
    /// Handles the Input/Output operations for saving and loading Sudoku game files.
    /// Supports both JSON and TXT formats.
    /// </summary>
    internal class SudokuFileManager
    {

        /// <summary>
        /// Saves the current Sudoku game state to a file.
        /// </summary>
        /// <param name="grid">The current grid object containing cell data.</param>
        /// <param name="path">The full file path including extension.</param>
        /// <param name="time">The time elapsed in the current game.</param>
        /// <param name="difficulty">The difficulty level of the game.</param>
        /// <param name="isBuilder">True if the game is currently in builder/custom mode.</param>
        /// <exception cref="ArgumentException">Thrown when the file extension is not .json or .txt.</exception>
        public void Save(CoreGrid grid, string path, TimeSpan time, DifficultyLevel difficulty, bool isBuilder)
        {
            if (path.EndsWith(".json"))
            {
                SaveToJSON(grid, path, time, difficulty, isBuilder);
            }
            else if (path.EndsWith(".txt"))
            {
                SaveToTXT(grid, path, time, difficulty, isBuilder);
            }
            else throw new Exception("Invalid file type");
        }

        /// <summary>
        /// Serializes the game data to a JSON file.
        /// </summary>
        private void SaveToJSON(CoreGrid grid, string path, TimeSpan time, DifficultyLevel difficulty, bool isBuilder)
        {
            GameSaveData data = new GameSaveData
            {
                GridData = grid.GetJaggedGrid(),
                TimeElapsedTicks = time.Ticks,
                Difficulty = difficulty,
                IsBuilderMode = isBuilder
            };

            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(data, options);

            using (StreamWriter stream = new StreamWriter(path))
            {
                stream.Write(jsonString);
            }
        }

        /// <summary>
        /// Serializes the game data to a custom text format.
        /// </summary>
        /// <remarks>
        /// Format: METADATA|Ticks|Difficulty|IsBuilder \n CellData...
        /// </remarks>
        private void SaveToTXT(CoreGrid grid, string path, TimeSpan time, DifficultyLevel difficulty, bool isBuilder)
        {
            using (StreamWriter stream = new StreamWriter(path))
            {
                stream.WriteLine($"METADATA|{time.Ticks}|{difficulty}|{isBuilder}");
                foreach (Cell c in grid.GridArray)
                {
                    stream.WriteLine(c.ToString());
                }
            }
        }

        /// <summary>
        /// Loads a Sudoku game from a JSON or TXT file.
        /// </summary>
        /// <param name="path">The full file path.</param>
        /// <returns>A <see cref="GameSaveData"/> object containing the loaded game state.</returns>
        /// <exception cref="ArgumentException">Thrown if the file type is invalid.</exception>
        /// <exception cref="Exception">Thrown if deserialization fails.</exception>
        public GameSaveData Load(string path)
        {

            using (StreamReader stream = new StreamReader(path))
            {
                if (path.EndsWith(".txt"))
                {
                    return LoadFromTXT(path, stream);
                }
                else if (path.EndsWith(".json"))
                {
                    return LoadFromJSON(path, stream);
                }
                else
                {
                    throw new ArgumentException("Invalid file type");
                }

            }
        }

        /// <summary>
        /// Parses a text file stream into GameSaveData.
        /// Supports both detailed metadata format and common 81-character string format.
        /// </summary>
        private GameSaveData LoadFromTXT(string path, StreamReader txtReader)
        {
            GameSaveData txtData = new GameSaveData();
            txtData.GridData = new Cell[9][];
            for (int i = 0; i < 9; i++)
            {
                txtData.GridData[i] = new Cell[9];
            }

            string line = txtReader.ReadLine();
            if (line == null)
            {
                throw new Exception("File is empty");
            }
            if (line.StartsWith("METADATA|"))
            {
                string[] parts = line.Split('|');
                if (parts.Length != 4)
                {
                    throw new Exception("Invalid metadata format");
                }

                txtData.TimeElapsedTicks = long.Parse(parts[1]);
                txtData.Difficulty = (DifficultyLevel)Enum.Parse(typeof(DifficultyLevel), parts[2]);
                txtData.IsBuilderMode = bool.Parse(parts[3]);
                line = txtReader.ReadLine();

                int i = 0;
                while (line != null && i < 81)
                {
                    int val = line[0] - '0';
                    bool isFixed = (line[1] == 'F');

                    Cell cell = new Cell { Value = val, Fixed = isFixed };

                    if (line.Length > 2)
                    {
                        for (int k = 2; k < line.Length; k++)
                        {
                            if (char.IsDigit(line[k]))
                            {
                                cell.Notations.Add(line[k] - '0');
                            }
                        }
                    }
                    txtData.GridData[i / 9][i % 9] = cell;

                    line = txtReader.ReadLine();
                    i++;
                }
            }
            else if (line.Length == 81)
            {
                txtData.TimeElapsedTicks = 0;
                txtData.Difficulty = DifficultyLevel.Custom;
                txtData.IsBuilderMode = false;
                for (int i = 0; i < 81; i++)
                {
                    int number = line[i] - '0';
                    txtData.GridData[i / 9][i % 9] = new Cell { Value = number, Fixed = (number != 0) };
                }
            }
            else { throw new Exception("Invalid file format"); }

            return txtData;
        }

        /// <summary>
        /// Deserializes a JSON stream into GameSaveData.
        /// </summary>
        private GameSaveData LoadFromJSON(string path, StreamReader jsonReader)
        {
            string jsonString = jsonReader.ReadToEnd();
            GameSaveData jsonData = JsonSerializer.Deserialize<GameSaveData>(jsonString);
            if (jsonData == null)
            {
                throw new Exception("Failed to deserialize JSON data");
            }
            return jsonData;
        }
    }
}
