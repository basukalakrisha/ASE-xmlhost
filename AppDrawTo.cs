using BOOSE;
using System;

namespace Shape_Project
{
    /// <summary>
    /// BOOSE command that draws a line from the current cursor position
    /// to a specified X and Y coordinate on the canvas.
    /// </summary>
    public class AppDrawTo : ICommand
    {
        /// <summary>
        /// Canvas instance used to perform drawing operations.
        /// </summary>
        private readonly ICanvas canvas;

        /// <summary>
        /// Reference to the stored BOOSE program context.
        /// </summary>
        private StoredProgram program = null!;

        /// <summary>
        /// Raw parameter tokens supplied to the command.
        /// </summary>
        private string[] parameters = null!;

        /// <summary>
        /// Parsed X coordinate.
        /// </summary>
        private int x;

        /// <summary>
        /// Parsed Y coordinate.
        /// </summary>
        private int y;

        /// <summary>
        /// Creates a new DrawTo command bound to the given canvas.
        /// </summary>
        /// <param name="canvas">Canvas used to draw the line.</param>
        public AppDrawTo(ICanvas canvas)
        {
            this.canvas = canvas;
        }

        /// <summary>
        /// Initializes the command with its program context and parameters.
        /// Splits, validates, and compiles the coordinate values.
        /// </summary>
        /// <param name="Program">The stored program executing this command.</param>
        /// <param name="Params">Parameter string containing X and Y values.</param>
        /// <exception cref="CommandException">
        /// Thrown if parameters are missing or invalid.
        /// </exception>
        public void Set(StoredProgram Program, string Params)
        {
            program = Program;

            if (string.IsNullOrWhiteSpace(Params))
                throw new CommandException("drawto requires coordinates");

            parameters = Params.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            CheckParameters(parameters);
            Compile();
        }

        /// <summary>
        /// Ensures exactly two parameters (X and Y) are provided.
        /// </summary>
        /// <param name="parameters">Parameter array to validate.</param>
        /// <exception cref="CommandException">
        /// Thrown when the parameter count is incorrect.
        /// </exception>
        public void CheckParameters(string[] parameters)
        {
            if (parameters.Length != 2)
                throw new CommandException("drawto <x> <y> - requires exactly 2 coordinates");
        }

        /// <summary>
        /// Parses and validates the X and Y coordinate values.
        /// </summary>
        /// <exception cref="CommandException">
        /// Thrown if either coordinate is not a valid integer.
        /// </exception>
        public void Compile()
        {
            if (!int.TryParse(parameters[0], out x))
                throw new CommandException("X coordinate must be a valid integer!");

            if (!int.TryParse(parameters[1], out y))
                throw new CommandException("Y coordinate must be a valid integer!");
        }

        /// <summary>
        /// Executes the command by drawing a line to the specified coordinates.
        /// </summary>
        public void Execute()
        {
            canvas.DrawTo(x, y);
        }
    }
}
