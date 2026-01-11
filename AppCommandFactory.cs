using System;
using BOOSE;

namespace Shape_Project
{
    /// <summary>
    /// Custom command factory for the BOOSE interpreter.
    /// Responsible for mapping command keywords to their
    /// corresponding application-specific command implementations.
    /// </summary>
    public class AppCommandFactory : BOOSE.CommandFactory
    {
        /// <summary>
        /// Shared canvas instance used by all drawing commands.
        /// </summary>
        private readonly BOOSE_Canvas _canvas; 

        /// <summary>
        /// Creates a new command factory bound to a BOOSE canvas.
        /// </summary>
        /// <param name="canvas">Canvas used by drawing-related commands.</param>
        public AppCommandFactory(BOOSE_Canvas canvas)
        {
            _canvas = canvas;
        }

        /// <summary>
        /// Creates and returns an ICommand instance based on the command keyword.
        /// Overrides the default BOOSE command factory to provide
        /// application-specific command implementations.
        /// </summary>
        /// <param name="commandType">The BOOSE command keyword.</param>
        /// <returns>An ICommand corresponding to the command keyword.</returns>
        public override ICommand MakeCommand(string commandType)
        {
            switch (commandType.ToLowerInvariant())
            {
                // Basic Drawing command

                case "pen":
                    return new AppPen(_canvas);

                case "circle":
                    return new AppCircle(_canvas);

                
                case "rect":
                    return new AppRect(_canvas);

                case "clear":
                    return new AppClear(_canvas); // Factory Design

                case "moveto":
                    return new AppMoveTo(_canvas);

                case "drawto":
                    return new AppDrawTo(_canvas);

                case "write":
                    return new AppWrite(_canvas);

                // BOOSE Variables

                case "int":
                    return new AppInt();

                case "real":
                    return new AppReal();

                case "boolean":
                    return new AppBoolean();

                case "array":
                    return new AppArray();

                case "poke":
                    return new AppPoke();

                case "peek":
                    return new AppPeek();

                // Looping Structure

                case "if":
                    return new AppIf();

                case "else":
                    return new ElseCommand();

                case "endif":
                    return new EndIfCommand();

                case "while":
                    return new AppWhile();

                case "endwhile":
                    return new EndWhileCommand();

                case "for":
                    return new AppFor();

                case "endfor":
                    return new EndForCommand();


                // Method Command
                case "method":
                    return new MethodCommand();

                case "call":
                     return new CallCommand();

                case "endmethod":
                     return new EndMethodCommand();



                default:
                    return base.MakeCommand(commandType);


            }
        }
    }
}
