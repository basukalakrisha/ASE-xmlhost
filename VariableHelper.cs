using System;
using System.Globalization;
using BOOSE;

namespace Shape_Project
{
    /// <summary>
    /// Helper methods for type conversion in BOOSE programs.
    /// Uses the program's EvaluateExpression but keeps double precision.
    /// </summary>
    public static class VariableHelper 
    {
        /// <summary>
        /// Converts an expression to an integer using the program's evaluator.
        /// </summary>
        public static int ToInt(StoredProgram program, string expr)
        {
            if (program == null) throw new ArgumentNullException(nameof(program));

            string s = program.EvaluateExpression(expr);

            if (int.TryParse(s, NumberStyles.Integer,
                             CultureInfo.InvariantCulture, out int v))
                return v;

            if (double.TryParse(s, NumberStyles.Float,
                                CultureInfo.InvariantCulture, out double d))
                return (int)Math.Round(d);

            throw new CommandException("int cannot parse: " + s);
        }

        /// <summary>
        /// Converts an expression to a double using the program's evaluator.
        /// Returns the exact double value written by EvaluateExpression,
        /// so reals like 15.5 or 174.044086 are preserved.
        /// </summary>
        public static double ToDouble(StoredProgram program, string expr)
        {
            if (program == null) throw new ArgumentNullException(nameof(program));

            string s = program.EvaluateExpression(expr);

            if (double.TryParse(s, NumberStyles.Float,
                                CultureInfo.InvariantCulture, out double v))
                return v;

            throw new CommandException("real cannot parse: " + s);
        }

        /// <summary>
        /// Converts an expression to a boolean using the program's evaluator.
        /// Accepts True/False, 1/0, yes/no.
        /// </summary>
        public static bool ToBool(StoredProgram program, string expr)
        {
            if (program == null) throw new ArgumentNullException(nameof(program));

            string s = program.EvaluateExpression(expr).Trim();

            if (bool.TryParse(s, out bool b))
                return b;

            if (int.TryParse(s, NumberStyles.Integer,
                             CultureInfo.InvariantCulture, out int i))
                return i != 0;

            s = s.ToLowerInvariant();
            if (s == "yes") return true;
            if (s == "no") return false;

            throw new CommandException("boolean cannot parse: " + s);
        }
    }
}
