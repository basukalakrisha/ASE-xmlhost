using BOOSE;
using Shape_Project;
using System;
using System.Collections;

namespace Shape_Project
{
    /// <summary>
    /// Implements the "for" loop command in BOOSE.
    /// Creates a counted loop that iterates from a start value to an end value with an optional step.
    /// Must be paired with a matching "end for" command.
    /// </summary>
    public  class AppFor : ICommand
    {
        private StoredProgram program = null!;   // Reference to current program
        private string varName = "";             // Loop variable name
        private string startExpr = "";           // Expression for loop start value
        private string endExpr = "";             // Expression for loop end value
        private string stepExpr = "1";           // Expression for optional step value
        private int endValue;                    // Cached evaluated end value
        private int stepValue;                   // Cached evaluated step value
        private bool isFirstExecution = true;    // Tracks first execution of the loop

        /// <summary>
        /// Parses the parameters for the for command.
        /// Expected format: var = start to end [step stepValue]
        /// </summary>
        /// <param name="Program">The current program context.</param>
        /// <param name="Params">The parameter string containing the loop definition.</param>
        /// <exception cref="ArgumentNullException">Thrown when Program is null.</exception>
        /// <exception cref="CommandException">Thrown when syntax is invalid or required parts are missing.</exception>
        public void Set(StoredProgram Program, string Params)
        {
            program = Program ?? throw new ArgumentNullException(nameof(Program));
            string p = (Params ?? "").Trim();
            if (p.Length == 0) throw new CommandException("for requires parameters");

            var tokens = p.Replace(",", " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 5 || tokens[1] != "=" || !tokens[3].Equals("to", StringComparison.OrdinalIgnoreCase))
                throw new CommandException("for usage: for <var> = <start> to <end> [step <step>]");

            varName = tokens[0];
            startExpr = tokens[2];
            endExpr = tokens[4];

            // Default step = 1
            stepExpr = "1";
            for (int i = 5; i < tokens.Length - 1; i++)
            {
                if (tokens[i].Equals("step", StringComparison.OrdinalIgnoreCase))
                {
                    stepExpr = tokens[i + 1];
                    break;
                }
            }
        }

        /// <summary>
        /// No compilation steps are required for the for command.
        /// </summary>
        public void Compile() { }

        /// <summary>
        /// Executes the for command.
        /// Initializes the loop on first execution.
        /// After end for is reached, checks if loop should continue.
        /// </summary>
        /// <exception cref="StoredProgramException">Thrown if command cannot locate itself in the program.</exception>
        /// <exception cref="CommandException">Thrown on errors such as step = 0 or missing end for.</exception>
        public void Execute()
        {
            IList cmds = FlowHelper.GetCommands(program, this);
            int me = FlowHelper.IndexOf(cmds, this);
            if (me < 0) throw new StoredProgramException("FOR not found in program.");

            if (isFirstExecution)
            {
                // Initialize loop variable and bounds
                int startValue = EvaluateInt(startExpr);
                endValue = EvaluateInt(endExpr);
                stepValue = EvaluateInt(stepExpr);

                if (stepValue == 0) throw new CommandException("for step cannot be 0");

                // Create or update loop variable
                SetLoopVariable(startValue);
                isFirstExecution = false;

                // Check if we should skip loop entirely
                if (!ShouldContinue(startValue))
                {
                    int endForIndex = FindEndFor(cmds, me);
                    FlowHelper.Jump(program, me, endForIndex + 1);
                    isFirstExecution = true; // reset for next execution
                }
                return;
            }

            // After returning from end for, check if loop continues
            int currentValue = GetLoopVariableValue();
            if (!ShouldContinue(currentValue))
            {
                int endForIndex = FindEndFor(cmds, me);
                FlowHelper.Jump(program, me, endForIndex + 1);
                isFirstExecution = true; // reset for next time
            }
        }

        /// <summary>
        /// Called by EndForCommand to increment the loop variable.
        /// </summary>
        internal void IncrementLoopVariable()
        {
            int currentValue = GetLoopVariableValue();
            SetLoopVariable(currentValue + stepValue);
        }

        /// <summary>
        /// Evaluates an expression to an integer.
        /// </summary>
        /// <param name="expr">Expression string.</param>
        /// <returns>Integer result.</returns>
        private int EvaluateInt(string expr)
        {
            string result = program.EvaluateExpression(expr);
            if (int.TryParse(result, out int value)) return value;
            if (double.TryParse(result, out double dvalue)) return (int)Math.Round(dvalue);
            throw new CommandException($"Cannot evaluate '{expr}' as integer");
        }

        /// <summary>
        /// Determines if the loop should continue based on current value and step direction.
        /// </summary>
        private bool ShouldContinue(int currentValue)
            => stepValue > 0 ? currentValue <= endValue : currentValue >= endValue;

        /// <summary>
        /// Gets the current value of the loop variable from the program.
        /// </summary>
        private int GetLoopVariableValue()
        {
            if (!program.VariableExists(varName))
                throw new CommandException($"Loop variable '{varName}' not found");

            object var = program.GetVariable(varName);
            if (var is Evaluation eval)
                return eval.Value;

            throw new CommandException($"Loop variable '{varName}' must be an integer");
        }

        /// <summary>
        /// Sets the loop variable to a specific value. Creates it if it doesn't exist.
        /// </summary>
        private void SetLoopVariable(int value)
        {
            if (program.VariableExists(varName))
                program.UpdateVariable(varName, value);
            else
                AddVariableToProgram(program, new LoopVariable(varName, value));
        }

        /// <summary>
        /// Finds the matching "end for" command index.
        /// </summary>
        private int FindEndFor(IList cmds, int forIndex)
        {
            int depth = 0;
            for (int i = forIndex + 1; i < cmds.Count; i++)
            {
                if (cmds[i] is AppFor) depth++;
                else if (cmds[i] is EndForCommand)
                {
                    if (depth == 0) return i;
                    depth--;
                }
            }
            throw new CommandException("Missing 'end for'");
        }

        /// <summary>
        /// Adds a variable to the program using reflection.
        /// </summary>
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

            throw new StoredProgramException("Cannot find AddVariable method in StoredProgram");
        }

        /// <summary>
        /// Not used (required by interface).
        /// </summary>
        public void CheckParameters(string[] Parameters) { }

        /// <summary>
        /// Internal class representing a loop control variable.
        /// </summary>
        private sealed class LoopVariable : Evaluation
        {
            public LoopVariable(string name, int initialValue)
            {
                // Set variable name using reflection
                var prop = GetType().GetProperty("VarName") ?? GetType().GetProperty("VariableName");
                if (prop != null && prop.CanWrite)
                    prop.SetValue(this, name);
                else
                {
                    var field = GetType().GetField("varName",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Public);
                    field?.SetValue(this, name);
                }

                Value = initialValue;
            }

            public override void Set(StoredProgram Program, string Params) { }
            public override void Compile() { }
            public override void Execute() { }
        }
    }

    /// <summary>
    /// Implements the "end for" command in BOOSE.
    /// Marks the end of a for loop block and increments the loop variable.
    /// Jumps back to the corresponding "for" command if loop should continue.
    /// Usage: end for
    /// </summary>
    public sealed class EndForCommand : ICommand
    {
        private StoredProgram program = null!;

        /// <summary>
        /// Sets the program context. No parameters are expected.
        /// </summary>
        public void Set(StoredProgram Program, string Params)
        {
            program = Program ?? throw new ArgumentNullException(nameof(Program));
        }

        /// <summary>
        /// No compilation steps required.
        /// </summary>
        public void Compile() { }

        /// <summary>
        /// Executes the end for command:
        /// finds the matching for loop, increments its loop variable, and jumps back to it.
        /// </summary>
        public void Execute()
        {
            IList cmds = FlowHelper.GetCommands(program, this);
            int me = FlowHelper.IndexOf(cmds, this);
            if (me < 0) throw new StoredProgramException("ENDFOR not found in program.");

            AppFor matchingFor = FindMatchingFor(cmds, me);

            // Increment the loop variable
            matchingFor.IncrementLoopVariable();

            // Jump back to the for command
            int forIndex = FlowHelper.IndexOf(cmds, matchingFor);
            FlowHelper.Jump(program, me, forIndex);
        }

        /// <summary>
        /// Finds the matching "for" command for this "end for" by scanning backward.
        /// Handles nested loops correctly.
        /// </summary>
        private AppFor FindMatchingFor(IList cmds, int endForIndex)
        {
            int depth = 0;
            for (int i = endForIndex - 1; i >= 0; i--)
            {
                if (cmds[i] is EndForCommand) depth++;
                else if (cmds[i] is AppFor fc)
                {
                    if (depth == 0) return fc;
                    depth--;
                }
            }
            throw new CommandException("Missing matching 'for' for end for");
        }

        /// <summary>
        /// Not used in this implementation.
        /// </summary>
        public void CheckParameters(string[] Parameters) { }
    }
}
