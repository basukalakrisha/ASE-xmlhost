using BOOSE;
using Shape_Project;
using System;
using System.Collections.Generic;

namespace Shape_Project
{
    /// <summary>
    /// Custom parser for the BOOSE language that extends the base <see cref="Parser"/> class.
    /// Provides enhanced parsing features including:
    /// <list type="bullet">
    /// <item><description>Support for multi-word terminators (e.g., "end if" → "endif").</description></item>
    /// <item><description>Automatic rewriting of assignments like "width = 20" into typed declarations
    /// ("int width = 20") based on existing variable types.</description></item>
    /// <item><description>Proper handling of comments, empty lines, and comma replacement.</description></item>
    /// <item><description>Full program parsing with error collection and syntax status reporting.</description></item>
    /// </list>
    /// </summary>
    public sealed class AppParser : Parser
    {
        /// <summary>
        /// Factory used to create <see cref="ICommand"/> instances.
        /// </summary>
        private readonly ICommandFactory _factory;

        /// <summary>
        /// Program storage that maintains parsed commands and variables.
        /// </summary>
        private readonly StoredProgram _program;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppParser"/> class.
        /// </summary>
        /// <param name="factory">The command factory used to create commands.</param>
        /// <param name="program">The program storage instance for parsed commands and variables.</param>
        public AppParser(CommandFactory factory, StoredProgram program) : base(factory, program)
        {
            _factory = factory;
            _program = program;
        }

        /// <summary>
        /// Parses a single line of BOOSE code into an <see cref="ICommand"/> object.
        /// </summary>
        public override ICommand ParseCommand(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;
            line = line.Trim();

            // Skip comments
            if (line.StartsWith("*") || line.StartsWith("//")) return null;

            // Replace commas in parameters with spaces
            line = line.Replace(",", " ");

            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return null;

            string commandWord = parts[0];
            string paramString = (parts.Length > 1)
                ? string.Join(" ", parts, 1, parts.Length - 1).Trim()
                : "";

            // Normalize multi-word terminators: "end if" -> "endif"
            if (commandWord.Equals("end", StringComparison.OrdinalIgnoreCase) && parts.Length >= 2)
            {
                string second = parts[1].ToLowerInvariant();
                if (second == "if") commandWord = "endif";
                else if (second == "while") commandWord = "endwhile";
                else if (second == "for") commandWord = "endfor";
                else if (second == "method") commandWord = "endmethod"; // >>> ADDED (method support)

                paramString = (parts.Length > 2)
                    ? string.Join(" ", parts, 2, parts.Length - 2).Trim()
                    : "";
            }

            // Assignment rewrite: "x = expr" -> typed assignment
            if (parts.Length > 1 && parts[1] == "=" &&
                !commandWord.Equals("int", StringComparison.OrdinalIgnoreCase) &&
                !commandWord.Equals("real", StringComparison.OrdinalIgnoreCase) &&
                !commandWord.Equals("boolean", StringComparison.OrdinalIgnoreCase))
            {
                // Must exist in variable table; no auto-creation
                if (!_program.VariableExists(commandWord))
                    throw new ParserException("Variable not declared: " + commandWord);

                string rewritten = commandWord + " " + paramString;
                object variable = _program.GetVariable(commandWord);

                if (variable is BOOSE.Int || variable is AppInt) commandWord = "int";
                else if (variable is BOOSE.Real || variable is AppReal) commandWord = "real";
                else if (variable is BOOSE.Boolean || variable is AppBoolean) commandWord = "boolean";
                else throw new ParserException("Cannot assign to variable type: " + variable.GetType().Name);

                paramString = rewritten;
            }

            ICommand cmd = _factory.MakeCommand(commandWord);
            if (cmd == null)
                throw new ParserException("Unknown command: " + commandWord);

            cmd.Set(_program, paramString);

            // Add to program if Set() did not already add it
            if (!_program.Contains(cmd))
                _program.Add(cmd);

            cmd.Compile();
            return cmd;
        }

        /// <summary>
        /// Parses an entire multi-line BOOSE program and populates the <see cref="StoredProgram"/>.
        /// </summary>
        public override void ParseProgram(string program)
        {
            if (program == null) program = "";
            program += "\n";

            _program.Clear();
            MethodHelper.Reset(_program); // >>> ADDED (method support)

            string errorText = "";
            string[] lines = program.Split('\n');

            var methodStack = new Stack<MethodCommand>(); // >>> ADDED (method support)

            for (int i = 0; i < lines.Length; i++)
            {
                string line = (lines[i] ?? "").Trim();
                if (line.Length == 0) continue;

                try
                {
                    ICommand cmd = ParseCommand(line);
                    if (cmd == null) continue;

                    if (!_program.Contains(cmd))
                        _program.Add(cmd);

                    int idx = _program.Count - 1; 

                    if (cmd is MethodCommand mc) 
                    {
                        mc.HeaderIndex = idx;
                        mc.StartIndex = idx + 1;
                        methodStack.Push(mc);
                    }
                    else if (cmd is EndMethodCommand emc) 
                    {
                        if (methodStack.Count == 0)
                            throw new ParserException("end method without matching method");

                        var start = methodStack.Pop();
                        start.EndIndex = idx;
                        emc.MethodName = start.MethodName;
                    }
                }
                catch (BOOSEException ex)
                {
                    if (!string.IsNullOrWhiteSpace(ex.Message))
                    {
                        errorText += ex.Message + " at line " + (i + 1) + "\n";
                        _program.SetSyntaxStatus(false);
                    }
                }
                catch (Exception ex)
                {
                    errorText += ex.Message + " at line " + (i + 1) + "\n";
                    _program.SetSyntaxStatus(false);
                }
            }

            if (methodStack.Count > 0)
            {
                errorText += "One or more methods are missing 'end method'\n";
                _program.SetSyntaxStatus(false);
            }

            errorText = errorText.Trim();
            if (errorText.Length != 0)
                throw new ParserException(errorText);
        }
    }
}
