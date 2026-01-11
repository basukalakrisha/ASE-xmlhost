
using BOOSE;
using System;
namespace Shape_Project
{
    /// <summary>
    /// Implements the ICommand interface to handle cursor movement commands.
    /// Moves the drawing cursor to the specified coordinates without drawing.
    /// </summary>
    public class AppMoveTo : ICommand
    {
        private readonly ICanvas canvas;
        private StoredProgram program = null!;
        private string[] parameters = null!;
        private int x, y;

        /// <summary>
        /// Initializes a new instance of the CommandMoveTo class.
        /// </summary>
        /// <param name="canvas">The canvas on which to move the cursor.</param>
        public AppMoveTo(ICanvas canvas)
        {
            this.canvas = canvas;
        }

        /// <summary>
        /// Sets up the command with the stored program and parameter string.
        /// Parses and validates the coordinates, then compiles the command.
        /// </summary>
        /// <param name="Program">The stored program context.</param>
        /// <param name="Params">The parameter string containing X and Y coordinates.</param>
        /// <exception cref="CommandException">Thrown when parameters are missing or invalid.</exception>
        public void Set(StoredProgram Program, string Params)
        {
            program = Program;
            if (string.IsNullOrWhiteSpace(Params))
                throw new CommandException("moveto requires coordinates");
            parameters = Params.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            CheckParameters(parameters);
            Compile();
        }

        /// <summary>
        /// Validates that exactly two coordinate parameters have been provided.
        /// </summary>
        /// <param name="parameters">The array of parameter strings to validate.</param>
        /// <exception cref="CommandException">Thrown when parameter count is not exactly 2.</exception>
        public void CheckParameters(string[] parameters)
        {
            if (parameters.Length != 2)
                throw new CommandException("moveto <x> <y> - requires exactly 2 coordinates");
        }

        /// <summary>
        /// Compiles the command by parsing the X and Y coordinate values.
        /// Validates that both coordinates are valid integers.
        /// </summary>
        /// <exception cref="CommandException">Thrown when coordinates are not valid integers.</exception>
        public void Compile()
        {
            if (!int.TryParse(parameters[0], out x))
                throw new CommandException("X coordinate must be a valid integer!");
            if (!int.TryParse(parameters[1], out y))
                throw new CommandException("Y coordinate must be a valid integer!");
        }

        /// <summary>
        /// Executes the moveto command on the canvas.
        /// Moves the cursor to the compiled X,Y coordinates without drawing.
        /// </summary>
        public void Execute()
        {
            canvas.MoveTo(x, y);
        }
    }
}