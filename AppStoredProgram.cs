using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using BOOSE;

namespace Shape_Project
{
    /// <summary>
    /// Custom implementation of StoredProgram that overrides expression evaluation
    /// and robust operator handling in BOOSE programs.
    /// Uses System.Data.DataTable.Compute for arithmetic, comparison, and logical operations
    /// after substituting variable values and normalizing the expression.
    /// </summary>
    public  class AppStoredProgram : StoredProgram
    {
        private int _programCounter;
        private const int MAX_EXECUTIONS = 50000;

        /// <summary>
        /// Initializes a new instance of the AppStoredProgram class.
        /// </summary>
        /// <param name="canvas">The canvas used for drawing commands.</param>
        public AppStoredProgram(ICanvas canvas) : base(canvas)
        {
            _programCounter = 0;
        }

        public override int PC
        {
            get => _programCounter;
            set => _programCounter = (value < 0) ? 0 : (value > Count) ? Count : value;
        }

        public override bool Commandsleft()
        {
            return _programCounter >= 0 && _programCounter < Count;
        }

        public override object NextCommand()
        {
            if (_programCounter < 0 || _programCounter >= Count)
                throw new StoredProgramException("Program counter out of bounds.");

            return this[_programCounter++];
        }

        public override void ResetProgram()
        {
            base.ResetProgram();
            _programCounter = 0;
        }

        /// <summary>
        /// CRITICAL: Runs program without 200-cycle restriction.
        /// </summary>
        public override void Run()
        {
            if (!IsValidProgram())
                throw new StoredProgramException("Program contains syntax errors.");

            int executionCount = 0;
            PC = 0;

            while (PC >= 0 && PC < Count)
            {
                int pcBefore = PC;

                if (!(this[pcBefore] is ICommand cmd))
                    throw new StoredProgramException($"Invalid command at line {pcBefore}");

                try
                {
                    cmd.Execute();
                    executionCount++;

                    // Auto-increment PC if command didn't jump
                    if (PC == pcBefore)
                        PC++;

                    if (executionCount > MAX_EXECUTIONS)
                        throw new StoredProgramException(
                            $"Execution limit exceeded ({MAX_EXECUTIONS}).");
                }
                catch (BOOSEException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new StoredProgramException(
                        $"Runtime error at line {pcBefore}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Evaluates a BOOSE expression string at runtime.
        /// Substitutes variable values (int, real, boolean), preserves array names,
        /// normalizes operators, and computes the result using DataTable.Compute.
        /// Returns the result as a string in invariant culture format.
        /// </summary>
        /// <param name="exp">The raw expression string to evaluate.</param>
        /// <returns>The string representation of the computed result.</returns>
        /// <exception cref="StoredProgramException">
        /// Thrown when the expression is empty, a variable is not found,
        /// or the computation fails.
        /// </exception>
        public override string EvaluateExpression(string exp)
        {
            if (string.IsNullOrWhiteSpace(exp))
                throw new StoredProgramException("Empty expression.");

            string normalized = NormalizeOperators(exp);

            normalized = Regex.Replace(
                normalized,
                @"\b[A-Za-z_]\w*\b",
                m =>
                {
                    string name = m.Value;

                    if (!VariableExists(name))
                        return name;

                    object v = GetVariable(name);

                    // Real variables
                    if (v is AppReal rc)
                        return rc.RealValue.ToString(CultureInfo.InvariantCulture);

                    // Boolean variables
                    if (v is AppBoolean bc)
                        return bc.BoolValue ? "True" : "False";

                    // Int variables
                    if (v is Evaluation ev)
                        return Convert.ToString(ev.Value, CultureInfo.InvariantCulture) ?? "0";

                    // Arrays should not be substituted
                    if (v is AppArray)
                        return name;

                    return name;
                });

            normalized = normalized
                .Replace("&&", " AND ")
                .Replace("||", " OR ");

            try
            {
                var dt = new DataTable { Locale = CultureInfo.InvariantCulture };
                object result = dt.Compute(normalized, "");

                return Convert.ToString(result, CultureInfo.InvariantCulture) ?? "";
            }
            catch (Exception ex)
            {
                throw new StoredProgramException(
                    "Bad expression: " + exp + " -> " + normalized + " (" + ex.Message + ")");
            }
        }

        /// <summary>
        /// Normalizes operators in the expression by adding spaces around them
        /// and correcting common spacing mistakes (e.g., "<=" instead of "< =").
        /// This ensures DataTable.Compute can parse the expression correctly.
        /// </summary>
        /// <param name="exp">The original expression string.</param>
        /// <returns>The normalized expression with proper operator spacing and formatting.</returns>
        private static string NormalizeOperators(string exp)
        {
            string s = exp;
            s = Regex.Replace(s, @"([()+\-*/<>!=])", " $1 ");
            s = Regex.Replace(s, @"\s+", " ").Trim();
            s = s.Replace("< =", "<=")
                 .Replace("> =", ">=")
                 .Replace("= =", "==")
                 .Replace("! =", "!=");
            return s;
        }
    }
}
