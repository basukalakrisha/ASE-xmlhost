using BOOSE;
using System;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Shape_Project
{
    /// <summary>
    /// Represents a boolean variable within the BOOSE language.
    /// </summary>
    /// <remarks>
    /// This command allows declaration and optional assignment of a boolean value.
    /// The syntax supported is:
    /// <c>boolean &lt;name&gt;</c> or <c>boolean &lt;name&gt; = &lt;expression&gt;</c>.
    /// Boolean values are internally synchronised with <see cref="Evaluation.Value"/>
    /// using <c>1</c> for <c>true</c> and <c>0</c> for <c>false</c>.
    /// </remarks>
    public  class AppBoolean : Evaluation
    {
        /// <summary>
        /// Reference to the currently executing stored program.
        /// </summary>
        private StoredProgram program = null!;

        /// <summary>
        /// Name of the boolean variable.
        /// </summary>
        private string varName = "";

        /// <summary>
        /// Optional expression used to initialise the boolean value.
        /// </summary>
        private string? expr;

        /// <summary>
        /// Gets the boolean value represented by this variable.
        /// </summary>
        public bool BoolValue { get; private set; }

        /// <summary>
        /// Parses and validates the boolean declaration parameters.
        /// </summary>
        /// <param name="Program">The current stored program.</param>
        /// <param name="Params">
        /// Parameters specifying the variable name and optional assignment expression.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the program reference is null.
        /// </exception>
        /// <exception cref="CommandException">
        /// Thrown when parameters are missing or invalid.
        /// </exception>
        public override void Set(StoredProgram Program, string Params)
        {
            program = Program ?? throw new ArgumentNullException(nameof(Program));
            if (string.IsNullOrWhiteSpace(Params))
                throw new CommandException("boolean requires a variable name");

            string p = Params.Trim();
            int eq = p.IndexOf('=');
            if (eq >= 0)
            {
                varName = p.Substring(0, eq).Trim();
                expr = p.Substring(eq + 1).Trim();
                if (expr.Length == 0) expr = null;
            }
            else
            {
                varName = p.Trim();
                expr = null;
            }

            if (!Regex.IsMatch(varName, @"^[A-Za-z_]\w*$"))
                throw new CommandException("Invalid boolean variable name: " + varName);
        }

        /// <summary>
        /// Compiles the boolean command.
        /// </summary>
        /// <remarks>
        /// No compilation logic is required for this command.
        /// </remarks>
        public override void Compile() { }

        /// <summary>
        /// Executes the boolean declaration or assignment.
        /// </summary>
        /// <remarks>
        /// If the variable already exists, its value is updated.
        /// Otherwise, a new boolean variable is created and registered
        /// with the stored program.
        /// </remarks>
        /// <exception cref="CommandException">
        /// Thrown when a type mismatch occurs or the expression is invalid.
        /// </exception>
        public override void Execute()
        {
            bool value = false; // default

            if (!string.IsNullOrWhiteSpace(expr))
            {
                value = VariableHelper.ToBool(program, expr!);
            }

            // Update existing variable if it exists
            if (program.VariableExists(varName))
            {
                object existing = program.GetVariable(varName);

                if (existing is AppBoolean bc)
                {
                    bc.BoolValue = value;
                    bc.Value = value ? 1 : 0; // Keep Evaluation.Value in sync (it's int)
                    return;
                }

                throw new CommandException("Type mismatch: variable '" + varName + "' is not boolean");
            }

            // New variable
            SetEvaluationName(this, varName);
            BoolValue = value;
            Value = value ? 1 : 0; // Evaluation expects int: 1 = true, 0 = false

            AddVariableToProgram(program, this);
        }

        /// <summary>
        /// Sets the variable name of an evaluation instance using reflection.
        /// </summary>
        /// <param name="ev">Evaluation object.</param>
        /// <param name="name">Variable name to assign.</param>
        private static void SetEvaluationName(Evaluation ev, string name)
        {
            var prop = ev.GetType().GetProperty("VarName");
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(ev, name);
                return;
            }

            prop = ev.GetType().GetProperty("VariableName");
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(ev, name);
                return;
            }

            var field = ev.GetType().GetField("varName",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(ev, name);
            }
        }

        /// <summary>
        /// Registers the evaluation variable with the stored program.
        /// </summary>
        /// <param name="program">The stored program.</param>
        /// <param name="variable">The variable to add.</param>
        private static void AddVariableToProgram(StoredProgram program, Evaluation variable)
        {
            var method = program.GetType().GetMethod("AddVariable", new[] { typeof(Evaluation) });
            if (method != null)
            {
                method.Invoke(program, new object[] { variable });
                return;
            }

            method = program.GetType().GetMethod("AddVariable");
            if (method != null)
            {
                method.Invoke(program, new object[] { variable });
                return;
            }

            throw new StoredProgramException("StoredProgram has no AddVariable method.");
        }
    }
}
