using System;
using System.Globalization;
using System.Text.RegularExpressions;
using BOOSE;

namespace Shape_Project
{
    /// <summary>
    /// Unrestricted real variable command.
    /// Stores full double precision and allows negative/large values.
    /// </summary>
    public  class AppReal : Evaluation
    {
        private StoredProgram program = null!;
        private string varName = "";
        private string? expr;          // null = declaration only

        /// <summary>
        /// Full-precision real value for this variable.
        /// </summary>
        public double RealValue { get; set; }

        public override void Set(StoredProgram Program, string Params)
        {
            program = Program ?? throw new ArgumentNullException(nameof(Program));

            if (string.IsNullOrWhiteSpace(Params))
                throw new CommandException("real requires a variable name");

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
                varName = p;
                expr = null;
            }

            if (!Regex.IsMatch(varName, @"^[A-Za-z_]\w*$"))
                throw new CommandException("Invalid real variable name: " + varName);
        }

        public override void Compile()
        {
            // evaluation happens at runtime
        }

        public override void Execute()
        {
            double value = 0.0;

            if (!string.IsNullOrEmpty(expr))
            {
                // unrestricted double evaluation
                value = VariableHelper.ToDouble(program, expr);
            }

            // variable already exists -> update if same type
            if (program.VariableExists(varName))
            {
                object existing = program.GetVariable(varName);
                if (existing is AppReal realVar)
                {
                    realVar.RealValue = value;
                    // keep integer Value loosely in sync for legacy int code
                    realVar.Value = (int)Math.Round(value);
                    return;
                }

                throw new CommandException("Variable already exists with different type");
            }

            // new variable
            SetEvaluationName(this, varName);

            RealValue = value;
            // legacy int value (not used for printing)
            Value = (int)Math.Round(value);

            AddVariableToProgram(program, this);
        }

        /// <summary>
        /// Helper used by expression evaluators.
        /// </summary>
        public double GetAsDouble() => RealValue;

        // ---- Reflection helpers to integrate with BOOSE ----

        private static void SetEvaluationName(Evaluation ev, string name)
        {
            var type = ev.GetType();

            var prop = type.GetProperty("VarName",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(ev, name);
                return;
            }

            prop = type.GetProperty("VariableName",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(ev, name);
                return;
            }

            var field = type.GetField("varName",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            if (field != null)
            {
                field.SetValue(ev, name);
                return;
            }

            throw new StoredProgramException("Cannot set variable name for real variable");
        }

        private static void AddVariableToProgram(StoredProgram program, Evaluation variable)
        {
            var method = program.GetType().GetMethod(
                "AddVariable",
                new[] { typeof(Evaluation) });

            if (method == null)
                throw new StoredProgramException("Cannot register variable: AddVariable method not found");

            method.Invoke(program, new object[] { variable });
        }
    }
}
