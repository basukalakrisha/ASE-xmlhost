using BOOSE;
using System;

namespace Shape_Project
{
    /// <summary>
    /// BOOSE command that draws a rectangle at the current canvas position.
    /// Supports numeric values, variables, and expressions for width and height,
    /// with optional filled or outline rendering.
    /// </summary>

    public class AppRect : ICommand
    {
        /// <summary>
        /// Canvas used to draw the rectangle.
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
        /// Token representing the rectangle width expression or variable.
        /// </summary>
        private string widthToken = "";

        /// <summary>
        /// Token representing the rectangle height expression or variable.
        /// </summary>
        private string heightToken = "";

        /// <summary>
        /// Indicates whether the rectangle should be filled.
        /// </summary>
        private bool filled = false;

        /// <summary>
        /// Creates a new Rectangle command bound to the given canvas.
        /// </summary>
        /// <param name="canvas">Canvas used for drawing.</param>
        public AppRect(ICanvas canvas) => this.canvas = canvas;

        /// <summary>
        /// Initializes the command with its program context and parameters.
        /// Performs validation and prepares the command for execution.
        /// </summary>
        /// <param name="Program">The stored program executing this command.</param>
        /// <param name="Params">Parameter string containing width, height, and optional fill flag.</param>
        /// <exception cref="CommandException">
        /// Thrown when required parameters are missing or invalid.
        /// </exception>
        public void Set(StoredProgram Program, string Params)
        {
            program = Program ?? throw new ArgumentNullException(nameof(Program));

            if (string.IsNullOrWhiteSpace(Params))
                throw new CommandException("rect requires parameters");

            parameters = Params.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            CheckParameters(parameters);
            Compile();
        }

        /// <summary>
        /// Validates the number of parameters for the rectangle command.
        /// </summary>
        /// <param name="parameters">Array of parameter tokens.</param>
        /// <exception cref="CommandException">
        /// Thrown when the parameter count is incorrect.
        /// </exception>
        public void CheckParameters(string[] parameters)
        {
            if (parameters.Length < 2 || parameters.Length > 3)
                throw new CommandException("rect <width> <height> [filled]");
        }

        /// <summary>
        /// Stores width and height tokens and determines whether
        /// the rectangle should be filled.
        /// </summary>
        /// <exception cref="CommandException">
        /// Thrown when an invalid fill option is supplied.
        /// </exception>
        public void Compile()
        {
            widthToken = parameters[0];
            heightToken = parameters[1];

            if (parameters.Length == 3)
            {
                filled = parameters[2].ToLowerInvariant() switch
                {
                    "true" or "yes" or "1" or "filled" or "fill" => true,
                    "false" or "no" or "0" or "outline" => false,
                    _ => throw new CommandException($"Invalid fill option: '{parameters[2]}'")
                };
            }
        }

        /// <summary>
        /// Executes the rectangle command by evaluating the width and height
        /// and drawing the rectangle on the canvas.
        /// </summary>
        /// <exception cref="CommandException">
        /// Thrown when evaluated dimensions are negative.
        /// </exception>
        public void Execute()
        {
            int width = VariableHelper.ToInt(program, widthToken);
            int height = VariableHelper.ToInt(program, heightToken);

            if (width < 0)
                throw new CommandException("Rectangle width must be non-negative!");
            if (height < 0)
                throw new CommandException("Rectangle height must be non-negative!");

            canvas.Rect(width, height, filled);
        }
    }
}
