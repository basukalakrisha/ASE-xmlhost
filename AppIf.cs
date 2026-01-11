using BOOSE;
using Shape_Project;
using System;
using System.Collections;

namespace Shape_Project
{
    /// <summary>
    /// Implements an IF command. Executes THEN block if condition is true, otherwise jumps to ELSE or END IF.
    /// Supports nested IF blocks.
    /// </summary>
    public  class AppIf : ICommand
    {
        private StoredProgram program = null!;
        private string condition = "";

        /// <summary>Sets the program context and the boolean condition for the IF block.</summary>
        public void Set(StoredProgram Program, string Params)
        {
            program = Program ?? throw new ArgumentNullException(nameof(Program));
            condition = (Params ?? "").Trim();
            if (condition.Length == 0)
                throw new CommandException("if requires a condition");
        }

        /// <summary>No compilation needed for IF.</summary>
        public void Compile() { }

        /// <summary>
        /// Executes the IF command.
        /// If the condition is true, continues to THEN block.
        /// Otherwise, jumps to ELSE block (if present) or END IF.
        /// </summary>
        public void Execute()
        {
            IList cmds = FlowHelper.GetCommands(program, this);
            int me = FlowHelper.IndexOf(cmds, this);
            if (me < 0) throw new StoredProgramException("IF not found.");

            if (FlowHelper.EvalBool(program, condition)) return;

            int depth = 0, elseIndex = -1, endIndex = -1;
            for (int i = me + 1; i < cmds.Count; i++)
            {
                if (cmds[i] is AppIf) depth++;
                else if (cmds[i] is EndIfCommand) { if (depth == 0) { endIndex = i; break; } depth--; }
                else if (cmds[i] is ElseCommand && depth == 0) elseIndex = i;
            }

            if (endIndex < 0) throw new CommandException("Missing 'end if'");
            FlowHelper.Jump(program, me, elseIndex >= 0 ? elseIndex + 1 : endIndex + 1);
        }

        /// <summary>Not used; required by interface.</summary>
        public void CheckParameters(string[] Parameters) { }
    }

    /// <summary>
    /// Implements the ELSE command. Skips execution of ELSE block if IF condition was true,
    /// and jumps directly to the matching END IF.
    /// </summary>
    public sealed class ElseCommand : ICommand
    {
        private StoredProgram program = null!;

        /// <summary>Sets the program context. ELSE does not take parameters.</summary>
        public void Set(StoredProgram Program, string Params) => program = Program ?? throw new ArgumentNullException(nameof(Program));

        /// <summary>No compilation needed for ELSE.</summary>
        public void Compile() { }

        /// <summary>
        /// Executes ELSE by skipping to END IF.
        /// Handles nested IF blocks correctly.
        /// </summary>
        public void Execute()
        {
            IList cmds = FlowHelper.GetCommands(program, this);
            int me = FlowHelper.IndexOf(cmds, this);
            if (me < 0) throw new StoredProgramException("ELSE not found.");

            int depth = 0;
            for (int i = me + 1; i < cmds.Count; i++)
            {
                if (cmds[i] is AppIf) depth++;
                else if (cmds[i] is EndIfCommand) { if (depth == 0) { FlowHelper.Jump(program, me, i + 1); return; } depth--; }
            }
            throw new CommandException("Missing 'end if' for else");
        }

        /// <summary>Not used; required by interface.</summary>
        public void CheckParameters(string[] Parameters) { }
    }

    /// <summary>
    /// Implements END IF command. Marks the end of an IF block.
    /// Execution flow is managed by IF and ELSE commands; this command performs no action.
    /// </summary>
    public sealed class EndIfCommand : ICommand
    {
        /// <summary>Sets program context (ignored).</summary>
        public void Set(StoredProgram Program, string Params) { }

        /// <summary>No compilation needed.</summary>
        public void Compile() { }

        /// <summary>No execution action. Acts as a marker for IF/ELSE.</summary>
        public void Execute() { }

        /// <summary>Not used; required by interface.</summary>
        public void CheckParameters(string[] Parameters) { }
    }
}
