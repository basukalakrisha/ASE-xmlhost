using System;
using BOOSE;

namespace Shape_Project
{
    /// <summary>
    /// Implements the BOOSE <c>write</c> command.
    /// </summary>
    /// <remarks>
    /// The write command outputs either literal text or the evaluated
    /// result of an expression at the current canvas cursor position.
    /// </remarks>
    public sealed class AppWrite : ICommand
    {
        /// <summary>
        /// Canvas used to render text output.
        /// </summary>
        private readonly ICanvas canvas;

        /// <summary>
        /// Reference to the currently executing stored program.
        /// </summary>
        private StoredProgram program = null!;

        /// <summary>
        /// Raw parameter string supplied to the write command.
        /// </summary>
        private string raw = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="AppWrite"/> command.
        /// </summary>
        /// <param name="canvas">Canvas on which text will be written.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the canvas reference is null.
        /// </exception>
        public AppWrite(ICanvas canvas)
        {
            this.canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }

        /// <summary>
        /// Initializes the command with the stored program context
        /// and captures the raw parameter string.
        /// </summary>
        /// <param name="Program">The current stored program.</param>
        /// <param name="Params">Text or expression to be written.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the program reference is null.
        /// </exception>
        /// <exception cref="CommandException">
        /// Thrown when no parameters are supplied.
        /// </exception>
        public void Set(StoredProgram Program, string Params)
        {
            program = Program ?? throw new ArgumentNullException(nameof(Program));

            if (string.IsNullOrWhiteSpace(Params))
                throw new CommandException("write requires text or an expression");

            raw = Params.Trim();
        }

        /// <summary>
        /// Compiles the write command.
        /// </summary>
        /// <remarks>
        /// No compilation step is required for this command.
        /// </remarks>
        public void Compile()
        {
            // nothing to compile
        }

        /// <summary>
        /// Executes the write command by evaluating the expression
        /// and writing the result to the canvas.
        /// </summary>
        public void Execute()
        {
            // EvaluateExpressionWithString must preserve double values,
            string output = program.EvaluateExpressionWithString(raw);
            canvas.WriteText(output);
        }

        /// <summary>
        /// Validates command parameters after parsing.
        /// </summary>
        /// <param name="Parameters">Array of parameter tokens.</param>
        /// <remarks>
        /// This method is intentionally left empty, as validation
        /// is handled during the <see cref="Set"/> phase.
        /// </remarks>
        public void CheckParameters(string[] Parameters)
        {

        }
    }
}
