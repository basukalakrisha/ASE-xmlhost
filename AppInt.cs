using BOOSE;
using System;
using System.Globalization;

namespace Shape_Project
{
    /// <summary>
    /// Implements the "int" command for integer variable declaration and assignment.
    /// Supports both declaration (int x) and assignment (int x = 10 or x = x + 5).
    /// </summary>
    public class AppInt : Evaluation
    {
        /// <summary>
        /// Compiles the int command by registering the variable with the program.
        /// This is called during the parsing phase.
        /// </summary>
        public override void Compile()
        {
            base.Compile();

            // Register the variable if it doesn't exist yet
            if (!Program.VariableExists(VarName))
                Program.AddVariable(this);
        }

        /// <summary>
        /// Executes the int command by evaluating the expression and updating the variable.
        /// This is called during program execution.
        /// </summary>
        public override void Execute()
        {
            base.Execute();

            // Case 1: Just declaration with no value (int height)
            if (string.IsNullOrWhiteSpace(Expression))
            {
                Value = 0;
                Program.UpdateVariable(VarName, 0);
                return;
            }

            // Case 2: Assignment with expression (int height = 150 or height = height - 15)
            try
            {
                //
                // This handles "height - 15" by getting the CURRENT value of height first
                int result = VariableHelper.ToInt(Program, Expression);

                // Update the Evaluation.Value property (used by BOOSE internally)
                Value = result;

                // Update the program's variable storage so other commands see the new value
                Program.UpdateVariable(VarName, result);
            }
            catch (CommandException)
            {
                // Re-throw CommandException as-is
                throw;
            }
            catch (Exception ex)
            {
                // Wrap other exceptions with context
                throw new CommandException(
                    $"Error evaluating int expression '{VarName} = {Expression}': {ex.Message}");
            }
        }
    }
}