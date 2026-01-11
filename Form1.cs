using BOOSE;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Shape_Project
{
    /// <summary>
    /// Main form for the BOOSE Graphics IDE.
    /// Handles user interaction, program parsing, execution,
    /// canvas rendering, and saving/loading of BOOSE programs.
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// BOOSE canvas used for drawing graphics.
        /// </summary>
        private BOOSE_Canvas? _canvas;

        /// <summary>
        /// Factory responsible for creating BOOSE commands.
        /// </summary>
        private AppCommandFactory? _factory;

        /// <summary>
        /// Stores the parsed BOOSE program for execution.
        /// </summary>
        private StoredProgram? _program;

        /// <summary>
        /// Parser used to validate and parse BOOSE commands/programs.
        /// </summary>
        private Parser? _parser;

        /// <summary>
        /// Indicates whether any drawing has been produced on the canvas.
        /// Used to enable or disable saving.
        /// </summary>
        private bool _hasDrawing = false;

        /// <summary>
        /// Initializes the form and sets up the BOOSE engine.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            InitializeBooseEngine();
            Debug.WriteLine(AboutBOOSE.about());
            Application.ThreadException += Application_ThreadException;
        }

        // -----------------BOOSE INITIALISATION ---------------------------

        /// <summary>
        /// Configures the canvas and initializes BOOSE components
        /// such as the canvas, command factory, parser, and program store.
        /// </summary>
        private void InitializeBooseEngine()
        {
            Canvas.SizeMode = PictureBoxSizeMode.Normal;
            Canvas.BorderStyle = BorderStyle.FixedSingle;
            Canvas.BackColor = Color.White;

            if (Canvas.Width < 400) Canvas.Width = 800;
            if (Canvas.Height < 300) Canvas.Height = 600;

            _canvas = new BOOSE_Canvas(Canvas);
            _canvas.Set(Canvas.Width, Canvas.Height);

            _factory = new AppCommandFactory(_canvas);

            _program = new AppStoredProgram(_canvas);
            _parser = new AppParser(_factory, _program);

            this.Text = "Krisha's BOOSE INTEPRETER";

            Canvas.Paint += Canvas_Paint;
            Form1_Load(null!, EventArgs.Empty);
            RefreshCanvas();
        }

        /// <summary>
        /// Handles form load logic and initializes default editor text.
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextOutput.Text))
            {
                TextOutput.Text =
                    "// BOOSE Graphics IDE\n" +
                    "// Write your drawing commands here\n" +
                    "// Example:\n" +
                    "// pen 255 0 0\n" +
                    "// circle 100";
            }

            RefreshCanvas();
        }

        /// <summary>
        /// Refreshes the PictureBox with the current canvas bitmap.
        /// </summary>
        private void RefreshCanvas()
        {
            if (_canvas == null) return;

            Bitmap bmp = _canvas.getBitmap() as Bitmap;
            if (bmp != null && !ReferenceEquals(Canvas.Image, bmp))
                Canvas.Image = bmp;

            Canvas.Invalidate();
            Canvas.Update();
        }

        /// <summary>
        /// Resets the BOOSE runtime environment by recreating
        /// the stored program and parser.
        /// </summary>
        private void ResetBooseRuntime()
        {
            if (_canvas == null || _factory == null) return;

            _program = new AppStoredProgram(_canvas);
            _parser = new AppParser(_factory, _program);
        }

        // ---------------------- RUN / CHECK ---------------------------

        /// <summary>
        /// Executes the BOOSE program written in the editor.
        /// </summary>
        private void ButtonRun_Click(object sender, EventArgs e)
        {
            string code = TextOutput.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Program text is empty. Write some commands first!",
                                "Run",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            ExecuteFullProgram(code);
        }

        /// <summary>
        /// Parses and executes the entire BOOSE program,
        /// handling both structured and line-by-line commands.
        /// </summary>
        /// <param name="rawCode">Raw program text from the editor.</param>
        private void ExecuteFullProgram(string rawCode)
        {
            var lines = rawCode
                .Replace(",", " ")
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l) &&
                           !l.StartsWith("//") &&
                           !l.StartsWith("*"))
                .ToArray();

            if (lines.Length == 0)
            {
                MessageBox.Show("No valid commands found (only comments/empty lines).",
                                "Run",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                return;
            }

            string normalizedProgram = string.Join("\n", lines);
            string allLower = string.Join(" ", lines).ToLowerInvariant();

            string[] forceStructured =
            {
                "int ", "real ", "boolean ", "array ",
                "=", "if ", "while ", "for ", "end "
            };

            bool isStructured = forceStructured.Any(k => allLower.Contains(k));

            try
            {
                _canvas!.Clear();
                ResetBooseRuntime();

                if (isStructured)
                {
                    _parser!.ParseProgram(normalizedProgram);
                    _program!.Run();
                }
                else
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];
                        ICommand cmd = _parser!.ParseCommand(line);
                        if (cmd == null)
                        {
                            MessageBox.Show($"Line {i + 1}: Unknown command '{line}'",
                                            "Run",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                            return;
                        }
                        cmd.Execute();
                    }
                }

                _hasDrawing = true;
                Save_button.Enabled = true;
                RefreshCanvas();

                MessageBox.Show("BOOSE program executed successfully!",
                                "Run",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            catch (BOOSEException ex)
            {
                MessageBox.Show($"BOOSE Error: {ex.Message}",
                                "Run",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Execution Error: {ex.Message}",
                                "Run",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Performs a syntax check on the BOOSE program
        /// without executing any drawing commands.
        /// </summary>
        private void CheckButton_Click(object sender, EventArgs e)
        {
            string code = TextOutput.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("No program to check!",
                                "Check",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            var lines = code
                .Replace(",", " ")
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l) &&
                           !l.StartsWith("//") &&
                           !l.StartsWith("*"))
                .ToArray();

            if (lines.Length == 0)
            {
                MessageBox.Show("No commands to check (only comments or empty lines).",
                                "Check",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                return;
            }

            try
            {
                _canvas!.DryRun = true;
                ResetBooseRuntime();

                string allLower = string.Join(" ", lines).ToLowerInvariant();
                string[] booseKeywords = { "int ", "real ", "array ", "while ", "if ", "else ", "end ", "for " };
                bool isFullBoose = booseKeywords.Any(k => allLower.Contains(k));

                if (!isFullBoose)
                {
                    foreach (string line in lines)
                        _parser!.ParseCommand(line);
                }
                else
                {
                    string normalized = string.Join("\n", lines);
                    _parser!.ParseProgram(normalized);
                }

                MessageBox.Show($"Syntax check passed.\nLines validated: {lines.Length}",
                                "Check",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            catch (BOOSEException ex)
            {
                MessageBox.Show($"Syntax error: {ex.Message}",
                                "Check",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Syntax error: {ex.Message}",
                                "Check",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            finally
            {
                _canvas!.DryRun = false;
            }
        }

        // ------------------CANVAS RENDERING -------------------------

        /// <summary>
        /// Paints the BOOSE canvas bitmap onto the PictureBox.
        /// </summary>
        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            if (_canvas == null) return;

            Bitmap bmp = _canvas.getBitmap() as Bitmap;
            if (bmp != null)
                e.Graphics.DrawImage(bmp, Canvas.ClientRectangle);
        }

        /// <summary>
        /// Handles uncaught exceptions occurring on the UI thread.
        /// </summary>
        private void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show($"Unhandled error: {e.Exception.Message}",
                            "Application Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
        }

        // ---------------------- SAVE IMAGE -------------------------------

        /// <summary>
        /// Saves the current canvas drawing to an image file.
        /// </summary>
        private void Save_button_Click(object sender, EventArgs e)
        {
            if (!_hasDrawing)
            {
                MessageBox.Show("Nothing has been drawn to save.",
                                "Save Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            Bitmap bmp = _canvas?.getBitmap() as Bitmap;
            if (bmp == null)
            {
                MessageBox.Show("Canvas image not available.",
                                "Save Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Save Canvas Image";
                sfd.Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|Bitmap Image (*.bmp)|*.bmp";
                sfd.FileName = "canvas.png";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string ext = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                    if (ext == ".jpg")
                        bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    else if (ext == ".bmp")
                        bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                    else
                        bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        /// <summary>
        /// Loads a BOOSE program from a file into the editor.
        /// </summary>
        private void lOADFILEToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "BOOSE Programs (*.boose)|*.boose|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            ofd.Title = "Load BOOSE Program";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    TextOutput.Text = File.ReadAllText(ofd.FileName);
                    this.Text = $"BOOSE IDE - {Path.GetFileName(ofd.FileName)} - Shape_Project";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Load failed: {ex.Message}",
                                    "Load",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }
            }
        }

        
    }
}
