using System;
using System.Linq;
using BOOSE;

namespace Shape_Project
{
    /// <summary>
    /// BOOSE command that draws a circle at the current canvas position.
    /// Supports numeric values, variables, and expressions for the radius,
    /// with optional filled or outline rendering.
    /// </summary>
   
    public class AppCircle : ICommand
    {
        /// <summary>
        /// Canvas used to draw the circle.
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
        /// Token representing the radius expression or variable.
        /// </summary>
        private string radiusToken = "";

        /// <summary>
        /// Indicates whether the circle should be filled.
        /// </summary>
        private bool filled = false;

        /// <summary>
        /// Creates a new Circle command bound to the given canvas.
        /// </summary>
        /// <param name="canvas">Canvas used for drawing.</param>
        public AppCircle(ICanvas canvas)
        {
            this.canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }

        /// <summary>
        /// Initializes the command with its program context and parameters.
        /// Performs validation and prepares the command for execution.
        /// </summary>
        /// <param name="Program">The stored program executing this command.</param>
        /// <param name="Params">Parameter string containing radius and optional fill flag.</param>
        /// <exception cref="CommandException">
        /// Thrown if required parameters are missing.
        /// </exception>
        public void Set(StoredProgram Program, string Params)
        {
            program = Program ?? throw new ArgumentNullException(nameof(Program));

            if (string.IsNullOrWhiteSpace(Params))
                throw new CommandException("circle requires parameters");

            // Normalize commas to spaces
            string p = Params.Trim().Replace(",", " ");

            // Split parameters for radius and fill detection
            parameters = p.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            CheckParameters(parameters);
            Compile();
        }

        /// <summary>
        /// Validates the minimum number of parameters for the circle command.
        /// </summary>
        /// <param name="parameters">Array of parameter tokens.</param>
        /// <exception cref="CommandException">
        /// Thrown when no radius parameter is provided.
        /// </exception>
        public void CheckParameters(string[] parameters)
        {
            if (parameters == null || parameters.Length < 1)
                throw new CommandException("circle <radius> [filled]");
        }

        /// <summary>
        /// Determines the radius expression and whether the circle is filled.
        /// </summary>
        public void Compile()
        {
            filled = false;

            if (parameters.Length == 1)
            {
                radiusToken = parameters[0];
                return;
            }

            string last = parameters[^1].ToLowerInvariant();

            if (IsFillToken(last))
            {
                filled = true;
                radiusToken = string.Join(" ", parameters.Take(parameters.Length - 1));
            }
            else if (IsOutlineToken(last))
            {
                filled = false;
                radiusToken = string.Join(" ", parameters.Take(parameters.Length - 1));
            }
            else
            {
                radiusToken = string.Join(" ", parameters);
                filled = false;
            }
        }

        /// <summary>
        /// Executes the circle command by evaluating the radius
        /// and drawing the circle on the canvas.
        /// </summary>
        /// <exception cref="CommandException">
        /// Thrown if the evaluated radius is not positive.
        /// </exception>
        public void Execute()
        {
            double dv = VariableHelper.ToDouble(program, radiusToken);
            int radius = (int)Math.Round(dv);

            if (radius <= 0)
                throw new CommandException("Circle radius must be positive!");

            canvas.Circle(radius, filled);
        }

        /// <summary>
        /// Determines whether a token represents a filled circle.
        /// </summary>
        private static bool IsFillToken(string t) =>
            t is "true" or "yes" or "1" or "filled" or "fill";

        /// <summary>
        /// Determines whether a token represents an outline-only circle.
        /// </summary>
        private static bool IsOutlineToken(string t) =>
            t is "false" or "no" or "0" or "outline";
    }
}
