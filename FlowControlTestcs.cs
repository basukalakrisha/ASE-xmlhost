using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shape_Project;
using BOOSE;

namespace TestingComponent2
{
    /// <summary>
    /// Unit tests for flow control–related helper functionality.
    /// Specifically validates boolean expression evaluation used
    /// by conditional commands such as if, while, and for.
    /// </summary>
    [TestClass]
    public class FlowControlTest
    {
        /// <summary>
        /// Instance of <see cref="AppStoredProgram"/> used for testing.
        /// Provides expression evaluation and runtime context.
        /// </summary>
        private AppStoredProgram program;

        /// <summary>
        /// Initializes a fresh program instance before each test.
        /// Ensures tests are isolated and do not share state.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            program = new AppStoredProgram(
                new BOOSE_Canvas(new System.Windows.Forms.PictureBox()));
        }

        /// <summary>
        /// Verifies that a boolean literal "true" is correctly evaluated
        /// as <c>true</c>.
        /// </summary>
        [TestMethod]
        public void FlowHelper_EvalBool_TrueLiteral_Works()
        {
            bool result = FlowHelper.EvalBool(program, "true");
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Verifies that a boolean literal "false" is correctly evaluated
        /// as <c>false</c>.
        /// </summary>
        [TestMethod]
        public void FlowHelper_EvalBool_FalseLiteral_Works()
        {
            bool result = FlowHelper.EvalBool(program, "false");
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Verifies that a simple comparison expression evaluates
        /// to <c>true</c>.
        /// </summary>
        [TestMethod]
        public void FlowHelper_EvalBool_SimpleComparison_Works()
        {
            bool result = FlowHelper.EvalBool(program, "( 8 > 3 )");
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Verifies that a compound boolean condition using logical AND
        /// evaluates correctly.
        /// </summary>
        [TestMethod]
        public void FlowHelper_EvalBool_CompoundCondition_Works()
        {
            bool result = FlowHelper.EvalBool(program, "( 5 > 2 ) && ( 3 < 10 )");
            Assert.IsTrue(result);
        }
    }
}
