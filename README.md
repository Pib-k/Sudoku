# Sudoku

Sudoku is a Sudoku application made in C# using Windows Forms.

## Features

### Gameplay & GUI

* **Interactive Gameplay**: A 9x9 grid supporting both mouse and keyboard navigation.
* **Dynamic Theming:** Switch between built in color themes and customize fonts via the Settings menu.
* **Resizing:** The GUI, including the board and buttons scales, dynamically to fit the window size.

### Game Modes

* **Solve Mode**: Plays the game with a timer and validates moves.
* **Build Mode**: Allows users to create a Sudoku puzzle themselves.
* **Sudoku Generation:** Generates unique Sudokus for specific difficulty levels (Easy to Extreme).

### Helpers

* **Advanced Hints:** Uses a Bitmask Solver to suggest the next logical move. Unlike basic solvers, it explains any of these strategies used:
    * Naked/Hidden Singles
    * Naked/Hidden/Pointing Pairs & Triples
    * X-Wing, Y-Wing, and Swordfish
* **Conflict Detection**: Visuals for invalid moves (duplicate numbers in rows, columns, or blocks).
* **Notation Mode**: Toggle "Pencil Marks" to note potential values in a cell.
* **Solvers**:
    * **Backtracking Solver**: A recursive algorithm to solve the board.
    * **Bitmask Solver**: Optimized logic for finding hints and validating possibilities.
**Solving Animation:** See the backtracking algorithm solve the board in real-time (toggle-able via the Settings Menu).

### Persistence
* **History:** Full Undo and Redo functionality, using stacks.
* **Persistence**: Save game state, including time elapsed, difficulty, and pencil marks, to JSON or Text files.

## Controls

You can use your mouse or keyboard to navigate the Sudoku.

| Action | Input |
| :--- | :--- |
| **Navigation** | Arrow Keys or Mouse Click |
| **Enter Number** | Keys 1 - 9 (or UI Buttons) |
| **Clear Cell** | 0 or Backspace (or Erase Button) |
| **Toggle Notations** | N key or Pencil Button |
| **Undo** | Ctrl + U (or UI Button) |
| **Redo** | Ctrl + R (or UI Button) |
| **Hint** | H key (or Hint Button) |
| **Erase Cell** | Backspace (or Erase Button) |

## Download

In order the download the .exe application either go to releases or click [here]().
You can also download the source code from the Releases page.

### Requirements:

Windows 10 (Version 1607 or newer) OR Windows 11.
64-bit edition (x64). <br>
The project relies only on standard .NET libraries, so no external third-party dependencies are required to build it. You only need the .NET 8 SDK.

### Core Logic & Data
* **`CoreGrid.cs`:** The data model representing the 9x9 board. It manages the `Cell` array and the Stacks for Undo/Redo history.
* **`Cell.cs`:** Represents a single cell, containing the value, fixed state (immutable), and a list of notations.
* **`HintDetails.cs`:** Represents a data class containing the hint type, which cells are the source of the hint, the target cells of the hint and which values are involved.
* **`ThemeManager.cs`**: Manages the program's style, applying themes to controls and the renderer.

### User Interface
* **`SudokuControl.cs`**: The main controller handling user input, game state (Paused/Solving), and interactions.
* **`SudokuBoardRenderer.cs`**: A dedicated GDI+ renderer responsible for drawing the grid, numbers, pencil marks, highlights, and conflict overlays.
* **`LayoutBuilder.cs`**: Handles the dynamic positioning and scaling of UI controls (buttons, panels) relative to the form size.
* **Forms**: `MainForm.cs`, `SettingsForm.cs`, `NewGameForm.cs`, `CompletionForm.cs`, and `InfoForm.cs`.

### Algorithms

Currently `Solver.cs` is unused as `BitmaskSolver.cs` is faster and more complete. 

* **`BitmaskSolver.cs`**: The core engine. It uses bitwise operations (AND, OR, XOR) to track candidates efficiently.
    * **Brute Force:** Recursive backtracking for validating uniqueness and solving.
    * **Human Logic:** Detects Sudoku Techniques (Singles, Pairs, X-Wings, etc.) to generate specific hints and analyze difficulty.
* **`DifficultyAnalyzer.cs`**: Grades a Sudoku's difficulty (Easy, Medium, Hard, Expert, Extreme) based on the most complex technique required to solve it.
* **`Generator.cs`**: Uses the solver and analyzer to generate unique Sudokus by filling a grid and iterively removing numbers.

### File Management
* **`SudokuFileManager.cs`**: Handles Input/Output operations.
* **`GameSaveData.cs`**: A Data Transfer Object used for serializing the game state to JSON.

## File Formats

Sudoku supports two file types:

1.  **JSON (.json) [Recommended]:** Saves the complete object graph, including the grid, notations, difficulty, timer ticks, and builder mode state.
2.  **Text (.txt):** Supports a custom format with a metadata header:
    ```text
    METADATA|Ticks|Difficulty|IsBuilder
    [Cell Data Line by Line]
    ```
    Also supports common 81-character string representation for simple Sudoku loading.

## Bugs

Found a bug? Please tell me!

Open a new issue with:

- Steps to reproduce: What did you click to make it break?
- Board State: Provide the 81-character string or the .json file if the solver/hint logic failed.
- Screenshots: Especially for layout or theme glitches.

