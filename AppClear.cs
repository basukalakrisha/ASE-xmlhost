using BOOSE;
using System;
namespace Shape_Project
{
    /// <summary>
    /// BOOSE command: <c>clear</c>.
    /// Clears the drawing canvas but does NOT move the pen.
    /// </summary>
    public class AppClear : CanvasCommand
    {
        /// <summary>
        /// Default constructor for factory.
        /// </summary>
        public AppClear() : base() { }

        /// <summary>
        /// Constructor with canvas (matches CanvasCommand base).
        /// </summary>
        public AppClear(BOOSE_Canvas canvas) : base(canvas) { }

        /// <summary>
        /// Executes the clear command by wiping the canvas to white.
        /// </summary>
        public override void Execute()
        {
            if (canvas != null)
                canvas.Clear();
            else
                throw new CanvasException("No canvas available for Clear command.");
        }

        /// <summary>
        /// Clear takes no parameters.
        /// </summary>
        public override void CheckParameters(string[] paramList)
        {
            //if (paramList.Length > 0)
                //throw new CanvasException("Clear command takes no parameters.");
        }
    }
}