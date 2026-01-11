using BOOSE;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Shape_Project
{
    /// <summary>
    /// Represents an array variable within the BOOSE language.
    /// Supports integer and real (double) array types.
    /// </summary>
    /// <remarks>
    /// The array command syntax is:
    /// <c>array &lt;int|real&gt; &lt;name&gt; &lt;size&gt;</c>
    /// </remarks>
    public class AppArray : Evaluation
    {
        /// <summary>
        /// Reference to the currently executing stored program.
        /// </summary>
        private StoredProgram program = null!;

        /// <summary>
        /// Element type of the array ("int" or "real").
        /// </summary>
        private string elementType = "";   // "int" or "real"

        /// <summary>
        /// Name of the array variable.
        /// </summary>
        private string arrayName = "";

        /// <summary>
        /// Expression representing the size of the array.
        /// </summary>
        private string sizeExpr = "";

        /// <summary>
        /// Gets the length of the array.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Storage for integer array data.
        /// </summary>
        public int[]? IntData { get; private set; }

        /// <summary>
        /// Storage for real (double) array data.
        /// </summary>
        public double[]? RealData { get; private set; }

        /// <summary>
        /// Parses and validates the array declaration parameters.
        /// </summary>
        /// <param name="Program">The current stored program.</param>
        /// <param name="Params">
        /// Parameters in the form:
        /// <c>&lt;int|real&gt; &lt;name&gt; &lt;size&gt;</c>
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the program reference is null.
        /// </exception>
        /// <exception cref="CommandException">
        /// Thrown when parameters are missing, malformed, or invalid.
        /// </exception>
        public override void Set(StoredProgram Program, string Params)
        {
            program = Program ?? throw new ArgumentNullException(nameof(Program));

            // expected: "int nums 10"  or  "real prices 10"
            if (string.IsNullOrWhiteSpace(Params))
                throw new CommandException("array usage: array <int|real> <name> <size>");

            var parts = Params.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                throw new CommandException("array usage: array <int|real> <name> <size>");

            elementType = parts[0].Trim().ToLowerInvariant();
            arrayName = parts[1].Trim();
            sizeExpr = parts[2].Trim();

            if (elementType != "int" && elementType != "real")
                throw new CommandException("array supports only int or real arrays");

            if (!Regex.IsMatch(arrayName, @"^[A-Za-z_]\w*$"))
                throw new CommandException("Invalid array name: " + arrayName);
        }

        /// <summary>
        /// Compiles the array command.
        /// </summary>
        /// <remarks>
        /// Compilation is deferred; tokens are stored for execution.
        /// </remarks>
        public override void Compile()
        {
            // store tokens only
        }

        /// <summary>
        /// Executes the array declaration, allocating storage
        /// and registering the array with the program.
        /// </summary>
        /// <exception cref="CommandException">
        /// Thrown if the size is invalid or a type mismatch occurs.
        /// </exception>
        public override void Execute()
        {
            int size = VariableHelper.ToInt(program, sizeExpr);
            if (size <= 0)
                throw new CommandException("array size must be > 0");

            if (program.VariableExists(arrayName))
            {
                // Unrestricted behavior: redeclare means reset the array
                object v = program.GetVariable(arrayName);
                if (v is AppArray existing)
                {
                    Length = size;
                    if (elementType == "int")
                    {
                        existing.IntData = new int[size];
                        existing.RealData = null;
                    }
                    else
                    {
                        existing.RealData = new double[size];
                        existing.IntData = null;
                    }
                    existing.Length = size;
                    return;
                }

                // name exists but isn't an array
                throw new CommandException("Type mismatch, expected an array: " + arrayName);
            }

            Length = size;

            if (elementType == "int")
                IntData = new int[size];
            else
                RealData = new double[size];

            // register this array as a variable in the program
            SetEvaluationName(this, arrayName);
            this.Value = 0; // Evaluation.Value is int in your BOOSE.dll

            AddVariableToProgram(program, this);
        }

        /// <summary>
        /// Writes an integer value to the specified array index.
        /// </summary>
        /// <param name="index">Target index.</param>
        /// <param name="value">Integer value to store.</param>
        public void Poke(int index, int value)
        {
            if (IntData == null) throw new CommandException("Not an int array");
            CheckIndex(index);
            IntData[index] = value;
        }

        /// <summary>
        /// Writes a real (double) value to the specified array index.
        /// </summary>
        /// <param name="index">Target index.</param>
        /// <param name="value">Real value to store.</param>
        public void Poke(int index, double value)
        {
            if (RealData == null) throw new CommandException("Not a real array");
            CheckIndex(index);
            RealData[index] = value;
        }

        /// <summary>
        /// Reads an integer value from the specified array index.
        /// </summary>
        /// <param name="index">Target index.</param>
        /// <returns>The integer value at the index.</returns>
        public int PeekInt(int index)
        {
            if (IntData == null) throw new CommandException("Not an int array");
            CheckIndex(index);
            return IntData[index];
        }

        /// <summary>
        /// Reads a real (double) value from the specified array index.
        /// </summary>
        /// <param name="index">Target index.</param>
        /// <returns>The real value at the index.</returns>
        public double PeekReal(int index)
        {
            if (RealData == null) throw new CommandException("Not a real array");
            CheckIndex(index);
            return RealData[index];
        }

        /// <summary>
        /// Validates that an index is within array bounds.
        /// </summary>
        /// <param name="index">Index to validate.</param>
        /// <exception cref="CommandException">
        /// Thrown when the index is out of range.
        /// </exception>
        private void CheckIndex(int index)
        {
            if (index < 0 || index >= Length)
                throw new CommandException($"Array index out of range: {index} (0..{Length - 1})");
        }

        /// <summary>
        /// Sets the variable name of an evaluation instance using reflection.
        /// </summary>
        /// <param name="ev">Evaluation object.</param>
        /// <param name="name">Variable name to assign.</param>
        private static void SetEvaluationName(Evaluation ev, string name)
        {
            var prop = ev.GetType().GetProperty("VarName");
            if (prop != null && prop.CanWrite) { prop.SetValue(ev, name); return; }

            prop = ev.GetType().GetProperty("VariableName");
            if (prop != null && prop.CanWrite) { prop.SetValue(ev, name); return; }

            var field = ev.GetType().GetField("varName",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            if (field != null) field.SetValue(ev, name);
        }

        /// <summary>
        /// Registers the evaluation variable with the stored program.
        /// </summary>
        /// <param name="program">The stored program.</param>
        /// <param name="variable">The variable to add.</param>
        private static void AddVariableToProgram(StoredProgram program, Evaluation variable)
        {
            var m = program.GetType().GetMethod("AddVariable", new[] { typeof(Evaluation) });
            if (m != null) { m.Invoke(program, new object[] { variable }); return; }

            m = program.GetType().GetMethod("AddVariable");
            if (m != null) { m.Invoke(program, new object[] { variable }); return; }

            throw new StoredProgramException("StoredProgram has no AddVariable method.");
        }
    }
}
