using BOOSE;
using System;

namespace Shape_Project
{
    /// <summary>
    /// Implements the BOOSE <c>poke</c> command, which assigns a value
    /// to a specific index of an existing array variable.
    /// </summary>
    /// <remarks>
    /// Supported array types are integer and real (double) arrays.
    /// The command syntax is:
    /// <c>poke &lt;arrayName&gt; &lt;index&gt; = &lt;value&gt;</c>
    /// </remarks>
    public class AppPoke : ICommand
    {
        /// <summary>
        /// Reference to the currently executing stored program.
        /// </summary>
        private StoredProgram program = null!;

        /// <summary>
        /// Name of the target array variable.
        /// </summary>
        private string arrayName = "";

        /// <summary>
        /// Expression representing the index to be updated.
        /// </summary>
        private string indexExpr = "";

        /// <summary>
        /// Expression representing the value to assign at the index.
        /// </summary>
        private string valueExpr = "";

        /// <summary>
        /// Initializes the command with the stored program context
        /// and parses the parameter string.
        /// </summary>
        /// <param name="Program">The current stored program.</param>
        /// <param name="Params">
        /// Command parameters in the form:
        /// <c>&lt;arrayName&gt; &lt;index&gt; = &lt;value&gt;</c>
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

            // expected: "nums 5 = 99"  OR  "prices 5 = 99.99"
            if (string.IsNullOrWhiteSpace(Params))
                throw new CommandException("poke usage: poke <arrayName> <index> = <value>");

            string p = Params.Replace(",", " ").Trim();

            // split on '=' first
            int eq = p.IndexOf('=');
            if (eq < 0)
                throw new CommandException("poke usage: poke <arrayName> <index> = <value>");

            string left = p.Substring(0, eq).Trim();
            string right = p.Substring(eq + 1).Trim();

            var leftParts = left.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (leftParts.Length != 2)
                throw new CommandException("poke usage: poke <arrayName> <index> = <value>");

            arrayName = leftParts[0].Trim();
            indexExpr = leftParts[1].Trim();
            valueExpr = right;
        }

        /// <summary>
        /// Compiles the command.
        /// </summary>
        /// <remarks>
        /// No compilation logic is required for this command.
        /// </remarks>
        public void Compile() { }

        /// <summary>
        /// Executes the poke operation by assigning a value
        /// to the specified index of the target array.
        /// </summary>
        /// <exception cref="CommandException">
        /// Thrown if the array does not exist, is not an array,
        /// the index is invalid, or the array type is unsupported.
        /// </exception>
        public void Execute()
        {
            if (!program.VariableExists(arrayName))
                throw new CommandException("Array not declared: " + arrayName);

            object v = program.GetVariable(arrayName);
            if (v is not AppArray arr)
                throw new CommandException("Type mismatch, expected an array: " + arrayName);

            int index = VariableHelper.ToInt(program, indexExpr);

            if (arr.IntData != null)
            {
                int iv = VariableHelper.ToInt(program, valueExpr);
                arr.Poke(index, iv);
                return;
            }

            if (arr.RealData != null)
            {
                double dv = VariableHelper.ToDouble(program, valueExpr);
                arr.Poke(index, dv);
                return;
            }

            throw new CommandException("Unsupported array type: " + arrayName);
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
