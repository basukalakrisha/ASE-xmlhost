using BOOSE;
using System;

namespace Shape_Project
{
    /// <summary>
    /// Implements the "while" command in BOOSE.
    /// Evaluates a condition and repeatedly executes the loop body
    /// while the condition remains true.
    /// </summary>
    public sealed class AppWhile : ICommand
    {
        private StoredProgram program = null!;
        private string condition = "";

        /// <summary>
        /// Initializes the while command with the program context and condition.
        /// </summary>
        /// <param name="Program">The current stored program.</param>
        /// <param name="Params">The condition expression to evaluate.</param>
        /// <exception cref="CommandException">
        /// Thrown when no condition is provided.
        /// </exception>
        public void Set(StoredProgram Program, string Params)
        {
            program = Program ?? throw new ArgumentNullException(nameof(Program));
            condition = (Params ?? "").Trim();

            if (condition.Length == 0)
                throw new CommandException("while requires a condition");
        }

        /// <summary>
        /// Compiles the while command.
        /// No pre-compilation is required as the condition
        /// is evaluated dynamically at runtime.
        /// </summary>
        public void Compile() { }

        /// <summary>
        /// Executes the while command.
        /// If the condition is false, execution jumps to the
        /// statement after the matching end while.
        /// If true, execution continues into the loop body.
        /// </summary>
        public void Execute()
        {
            bool isTrue = EvaluateCondition();

            if (!isTrue)
            {
                // Skip loop: jump after matching end while
                int endWhileIndex = FindEndWhile();
                program.PC = endWhileIndex + 1;
            }
            else
            {
                // Enter loop body
                program.PC = program.PC + 1;
            }
        }

        /// <summary>
        /// Evaluates the while condition expression.
        /// Supports integer, real, and boolean expressions.
        /// </summary>
        /// <returns>
        /// True if the evaluated condition is non-zero or true; otherwise false.
        /// </returns>
        /// <exception cref="CommandException">
        /// Thrown if condition evaluation fails.
        /// </exception>
        private bool EvaluateCondition()
        {
            try
            {
                string result = program.EvaluateExpression(condition).Trim();

                if (int.TryParse(result, out int intVal))
                    return intVal != 0;

                if (double.TryParse(result, out double dblVal))
                    return dblVal != 0.0;

                if (bool.TryParse(result, out bool boolVal))
                    return boolVal;

                string lower = result.ToLowerInvariant();
                if (lower == "true") return true;
                if (lower == "false") return false;

                return false;
            }
            catch (Exception ex)
            {
                throw new CommandException($"While condition error: {ex.Message}");
            }
        }

        /// <summary>
        /// Finds the index of the matching end while command,
        /// correctly handling nested while loops.
        /// </summary>
        /// <returns>The index of the matching EndWhileCommand.</returns>
        /// <exception cref="CommandException">
        /// Thrown if no matching end while is found.
        /// </exception>
        private int FindEndWhile()
        {
            int depth = 0;

            for (int i = program.PC + 1; i < program.Count; i++)
            {
                if (program[i] is AppWhile)
                    depth++;
                else if (program[i] is EndWhileCommand)
                {
                    if (depth == 0)
                        return i;
                    depth--;
                }
            }

            throw new CommandException("Missing 'end while'");
        }

        /// <summary>
        /// Parameter validation is handled in Set.
        /// </summary>
        public void CheckParameters(string[] Parameters) { }
    }

    /// <summary>
    /// Implements the "end while" command in BOOSE.
    /// Causes execution to jump back to the matching while command.
    /// </summary>
    public sealed class EndWhileCommand : ICommand
    {
        private StoredProgram program = null!;

        /// <summary>
        /// Initializes the end while command with the program context.
        /// </summary>
        /// <param name="Program">The current stored program.</param>
        public void Set(StoredProgram Program, string Params)
        {
            program = Program ?? throw new ArgumentNullException(nameof(Program));
        }

        /// <summary>
        /// No compilation logic is required for end while.
        /// </summary>
        public void Compile() { }

        /// <summary>
        /// Executes the end while command.
        /// Jumps execution back to the matching while command
        /// so the loop condition can be re-evaluated.
        /// </summary>
        public void Execute()
        {
            int whileIndex = FindMatchingWhile();
            program.PC = whileIndex;
        }

        /// <summary>
        /// Finds the matching while command for this end while,
        /// correctly handling nested loops.
        /// </summary>
        /// <returns>The index of the matching AppWhile command.</returns>
        /// <exception cref="CommandException">
        /// Thrown if no matching while is found.
        /// </exception>
        private int FindMatchingWhile()
        {
            int depth = 0;

            for (int i = program.PC - 1; i >= 0; i--)
            {
                if (program[i] is EndWhileCommand)
                    depth++;
                else if (program[i] is AppWhile)
                {
                    if (depth == 0)
                        return i;
                    depth--;
                }
            }

            throw new CommandException("Missing matching 'while'");
        }

        /// <summary>
        /// End while does not accept parameters.
        /// </summary>
        public void CheckParameters(string[] Parameters) { }
    }
}