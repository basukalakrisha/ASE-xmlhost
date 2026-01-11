using BOOSE;
using System;
using System.Globalization;

namespace Shape_Project
{
    /// <summary>
    /// Implements the BOOSE <c>peek</c> command, which reads a value
    /// from an array at a specified index and assigns it to a variable.
    /// </summary>
    /// <remarks>
    /// Supported array types are integer and real (double) arrays.
    /// The command syntax is:
    /// <c>peek &lt;destVar&gt; = &lt;arrayName&gt; &lt;index&gt;</c>
    /// </remarks>
    public  class AppPeek : ICommand
    {
        /// <summary>
        /// Reference to the currently executing stored program.
        /// </summary>
        private StoredProgram program = null!;

        /// <summary>
        /// Name of the destination variable that will receive the value.
        /// </summary>
        private string destVar = "";

        /// <summary>
        /// Name of the source array variable.
        /// </summary>
        private string arrayName = "";

        /// <summary>
        /// Expression representing the index to read from the array.
        /// </summary>
        private string indexExpr = "";

        /// <summary>
        /// Initializes the command with the stored program context
        /// and parses the parameter string.
        /// </summary>
        /// <param name="Program">The current stored program.</param>
        /// <param name="Params">
        /// Command parameters in the form:
        /// <c>&lt;destVar&gt; = &lt;arrayName&gt; &lt;index&gt;</c>
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the program reference is null.
        /// </exception>
        /// <exception cref="CommandException">
        /// Thrown when the parameters are missing or malformed.
        /// </exception>
        public void Set(StoredProgram Program, string Params)
        {
            program = Program ?? throw new ArgumentNullException(nameof(Program));

            // expected: "x = nums 5"
            if (string.IsNullOrWhiteSpace(Params))
                throw new CommandException("peek usage: peek <destVar> = <arrayName> <index>");

            string p = Params.Replace(",", " ").Trim();

            int eq = p.IndexOf('=');
            if (eq < 0)
                throw new CommandException("peek usage: peek <destVar> = <arrayName> <index>");

            string left = p.Substring(0, eq).Trim();
            string right = p.Substring(eq + 1).Trim();

            var rightParts = right.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (rightParts.Length != 2)
                throw new CommandException("peek usage: peek <destVar> = <arrayName> <index>");

            destVar = left;
            arrayName = rightParts[0].Trim();
            indexExpr = rightParts[1].Trim();
        }

        /// <summary>
        /// Compiles the command.
        /// </summary>
        /// <remarks>
        /// No compilation logic is required for this command.
        /// </remarks>
        public void Compile() { }

        /// <summary>
        /// Executes the peek operation by reading a value from the array
        /// and assigning it to the destination variable.
        /// </summary>
        /// <exception cref="CommandException">
        /// Thrown if the array or destination variable does not exist,
        /// if types are incompatible, or if the array type is unsupported.
        /// </exception>
        public void Execute()
        {
            if (!program.VariableExists(arrayName))
                throw new CommandException("Array not declared: " + arrayName);

            if (!program.VariableExists(destVar))
                throw new CommandException("Destination variable not declared: " + destVar);

            object a = program.GetVariable(arrayName);
            if (a is not AppArray arr)
                throw new CommandException("Type mismatch, expected an array: " + arrayName);

            int index = VariableHelper.ToInt(program, indexExpr);

            object dest = program.GetVariable(destVar);

            if (arr.IntData != null)
            {
                int value = arr.PeekInt(index);
                SetInt(dest, value);
                return;
            }

            if (arr.RealData != null)
            {
                double value = arr.PeekReal(index);
                SetReal(dest, value);
                return;
            }

            throw new CommandException("Unsupported array type: " + arrayName);
        }

        /// <summary>
        /// Assigns an integer value to the destination variable.
        /// </summary>
        /// <param name="dest">The destination variable object.</param>
        /// <param name="value">The integer value to assign.</param>
        /// <exception cref="CommandException">
        /// Thrown when the destination variable is not compatible with an integer value.
        /// </exception>
        private static void SetInt(object dest, int value)
        {
            // common: IntCommand / BOOSE.Int are Evaluation with int Value
            if (dest is Evaluation ev)
            {
                ev.Value = value;
                return;
            }

            throw new CommandException("Type mismatch, expected an int destination");
        }

        /// <summary>
        /// Assigns a real (double) value to the destination variable.
        /// </summary>
        /// <param name="dest">The destination variable object.</param>
        /// <param name="value">The real value to assign.</param>
        /// <exception cref="CommandException">
        /// Thrown when the destination variable is not compatible with a real value.
        /// </exception>
        private static void SetReal(object dest, double value)
        {
            // your replacement real
            if (dest is AppReal rc)
            {
                rc.RealValue = value; // needs setter; make RealValue settable OR use internal method
                return;
            }

            // if BOOSE.Real exists with a double property named Value
            var prop = dest.GetType().GetProperty("Value");
            if (prop != null && prop.CanWrite && prop.PropertyType == typeof(double))
            {
                prop.SetValue(dest, value);
                return;
            }

            throw new CommandException("Type mismatch, expected a real destination");
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
            //throw new NotImplementedException();
        }
    }
}
