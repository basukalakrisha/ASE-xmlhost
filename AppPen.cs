using BOOSE;
using System;

namespace Shape_Project
{
    /// <summary>
    /// BOOSE command that sets the current pen colour for the canvas.
    /// Accepts red, green, and blue values as literals or variables.
    /// </summary>
    /// <remarks>
    /// Usage:
    /// <list type="bullet">
    /// <item><description>pen &lt;red&gt; &lt;green&gt; &lt;blue&gt;</description></item>
    /// <item><description>pen r g b</description></item>
    /// </list>
    /// RGB values must be in the range 0–255.
    /// </remarks>
    public class AppPen : ICommand
    {
        /// <summary>
        /// Canvas whose pen colour will be updated.
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
        /// Token representing the red colour component.
        /// </summary>
        private string rToken = "";

        /// <summary>
        /// Token representing the green colour component.
        /// </summary>
        private string gToken = "";

        /// <summary>
        /// Token representing the blue colour component.
        /// </summary>
        private string bToken = "";

        /// <summary>
        /// Creates a new Pen command bound to the given canvas.
        /// </summary>
        /// <param name="canvas">Canvas used for drawing.</param>
        public AppPen(ICanvas canvas) => this.canvas = canvas;

        /// <summary>
        /// Initializes the command with its program context and parameters.
        /// Performs validation and prepares the command for execution.
        /// </summary>
        /// <param name="Program">The stored program executing this command.</param>
        /// <param name="Params">Parameter string containing RGB values.</param>
        /// <exception cref="CommandException">
        /// Thrown when parameters are missing or invalid.
        /// </exception>
        public void Set(StoredProgram Program, string Params)
        {
            program = Program;

            if (string.IsNullOrWhiteSpace(Params))
                throw new CommandException("pen requires RGB parameters");

            parameters = Params.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            CheckParameters(parameters);
            Compile();
        }

        /// <summary>
        /// Validates the number of parameters for the pen command.
        /// </summary>
        /// <param name="parameters">Array of parameter tokens.</param>
        /// <exception cref="CommandException">
        /// Thrown when the parameter count is incorrect.
        /// </exception>
        public void CheckParameters(string[] parameters)
        {
            if (parameters.Length != 3)
                throw new CommandException("pen <red> <green> <blue>");
        }

        /// <summary>
        /// Stores the RGB parameter tokens for runtime evaluation.
        /// </summary>
        public void Compile()
        {
            rToken = parameters[0];
            gToken = parameters[1];
            bToken = parameters[2];
        }

        /// <summary>
        /// Evaluates RGB values at runtime and updates the canvas pen colour.
        /// </summary>
        /// <exception cref="CommandException">
        /// Thrown when colour values are outside the valid range.
        /// </exception>
        public void Execute()
        {
            int r = VariableHelper.ToInt(program, rToken);
            int g = VariableHelper.ToInt(program, gToken);
            int b = VariableHelper.ToInt(program, bToken);

            if (r < 0 || r > 255) throw new CommandException("Red value must be between 0 and 255!");
            if (g < 0 || g > 255) throw new CommandException("Green value must be between 0 and 255!");
            if (b < 0 || b > 255) throw new CommandException("Blue value must be between 0 and 255!");

            canvas.SetColour(r, g, b);
        }
    }
}
