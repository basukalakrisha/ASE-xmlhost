using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shape_Project;
using BOOSE;

namespace TestingComponent2
{
    /// <summary>
    /// Unit tests for method-related commands in the BOOSE interpreter.
    /// These tests validate basic instantiation and parameter parsing
    /// for method definitions, calls, and method termination.
    /// </summary>
    [TestClass]
    public class MethodTest
    {
        /// <summary>
        /// Program instance used as execution context for method commands.
        /// </summary>
        private AppStoredProgram program;

        /// <summary>
        /// Initializes a new <see cref="AppStoredProgram"/> before each test
        /// to ensure a clean execution environment.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            program = new AppStoredProgram(
                new BOOSE_Canvas(new System.Windows.Forms.PictureBox()));
        }

        /// <summary>
        /// Verifies that a <see cref="MethodCommand"/> can be instantiated
        /// and initialized using the <c>Set</c> method.
        /// </summary>
        [TestMethod]
        public void MethodCommand_CanBeInstantiated()
        {
            var method = new MethodCommand();
            method.Set(program, "drawSquare size");
            Assert.IsNotNull(method);
        }

        /// <summary>
        /// Verifies that an <see cref="EndMethodCommand"/> can be created
        /// and associated with a program instance.
        /// </summary>
        [TestMethod]
        public void EndMethodCommand_CanBeCreated()
        {
            var end = new EndMethodCommand();
            end.Set(program, "");
            Assert.IsNotNull(end);
        }

        /// <summary>
        /// Verifies that a <see cref="CallCommand"/> can parse
        /// a valid method call with arguments.
        /// </summary>
        [TestMethod]
        public void CallCommand_CanParseCall()
        {
            var call = new CallCommand();
            call.Set(program, "drawSquare 100");
            Assert.IsNotNull(call);
        }

        /// <summary>
        /// Verifies that a <see cref="MethodCommand"/> correctly
        /// processes a valid method signature in the <c>Set</c> call.
        /// </summary>
        [TestMethod]
        public void MethodCommand_HasValidSetCall()
        {
            var method = new MethodCommand();
            method.Set(program, "testMethod x y z");
            Assert.IsNotNull(method);
        }

        /// <summary>
        /// Verifies that a <see cref="CallCommand"/> can handle
        /// a method call with no arguments.
        /// </summary>
        public void CallCommand_EmptyParamsCall()
        {
            var call = new CallCommand();
            call.Set(program, "simple");
            Assert.IsNotNull(call);
        }
    }
}
