using BOOSE;
using System;
using System.Collections;
using System.Reflection;

namespace Shape_Project
{
    /// <summary>
    /// Provides helper methods for control flow commands in BOOSE.
    /// Used by IF, ELSE, WHILE, FOR commands to find commands, evaluate expressions, and jump in program.
    /// </summary>
    public static class FlowHelper
    {
        /// <summary>
        /// Finds the command list in a StoredProgram containing the given callingCommand.
        /// Uses reflection to locate fields or properties of type IList.
        /// </summary>
        /// <param name="program">The program to search in.</param>
        /// <param name="callingCommand">The command instance to locate.</param>
        /// <returns>The IList of commands containing callingCommand.</returns>
        public static IList GetCommands(StoredProgram program, ICommand callingCommand)
        {
            if (program == null) throw new ArgumentNullException(nameof(program));
            if (callingCommand == null) throw new ArgumentNullException(nameof(callingCommand));

            var flags = BindingFlags.Instance | BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            Type type = program.GetType();

            while (type != null)
            {
                foreach (var field in type.GetFields(flags))
                {
                    if (typeof(IList).IsAssignableFrom(field.FieldType) &&
                        field.GetValue(program) is IList list)
                    {
                        for (int i = 0; i < list.Count; i++)
                            if (ReferenceEquals(list[i], callingCommand))
                                return list;
                    }
                }

                foreach (var prop in type.GetProperties(flags))
                {
                    if (typeof(IList).IsAssignableFrom(prop.PropertyType) &&
                        prop.CanRead && prop.GetValue(program) is IList list)
                    {
                        for (int i = 0; i < list.Count; i++)
                            if (ReferenceEquals(list[i], callingCommand))
                                return list;
                    }
                }

                type = type.BaseType;
            }

            throw new StoredProgramException("Cannot find command list inside StoredProgram.");
        }

        /// <summary>
        /// Returns the index of a command in the provided command list, or -1 if not found.
        /// </summary>
        public static int IndexOf(IList commands, ICommand command)
        {
            for (int i = 0; i < commands.Count; i++)
                if (ReferenceEquals(commands[i], command))
                    return i;
            return -1;
        }

        /// <summary>
        /// Evaluates a boolean expression using the program's evaluator.
        /// Supports true/false, 1/0, and integer/double values.
        /// </summary>
        public static bool EvalBool(StoredProgram program, string expr)
        {
            if (program == null) throw new ArgumentNullException(nameof(program));
            if (string.IsNullOrWhiteSpace(expr)) return false;

            try
            {
                string result = program.EvaluateExpression(expr).Trim();

                if (result.Equals("True", StringComparison.OrdinalIgnoreCase)) return true;
                if (result.Equals("False", StringComparison.OrdinalIgnoreCase)) return false;
                if (int.TryParse(result, out int intVal)) return intVal != 0;
                if (double.TryParse(result, out double dblVal)) return dblVal != 0;

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Evaluates an integer expression using the program's evaluator.
        /// </summary>
        public static int EvalInt(StoredProgram program, string expr)
        {
            return Convert.ToInt32(program.EvaluateExpression(expr));
        }

        /// <summary>
        /// Changes the program counter so the next executed command is at the specified index.
        /// Used to implement jumps for loops, IF/ELSE, and FOR commands.
        /// </summary>
        /// <param name="program">The program to update.</param>
        /// <param name="currentIndex">The current command index (unused here).</param>
        /// <param name="targetIndex">The index to jump to.</param>
        public static void Jump(StoredProgram program, int currentIndex, int targetIndex)
        {
            if (program == null) throw new ArgumentNullException(nameof(program));
            program.PC = targetIndex; // Program runner respects updated PC
        }
    }
}
