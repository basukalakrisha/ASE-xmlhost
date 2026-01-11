using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shape_Project;
using BOOSE;
using System;

namespace TestingComponent2
{
    /// <summary>
    /// Unit tests for variable-related commands in the BOOSE interpreter.
    /// These tests verify correct creation, assignment, expression evaluation,
    /// and value preservation for int, real, and boolean variables.
    /// </summary>
    [TestClass]
    public class VariablesTest
    {
        /// <summary>
        /// Program instance used to store and retrieve variables during tests.
        /// </summary>
        private AppStoredProgram program;

        /// <summary>
        /// Canvas instance required by the <see cref="AppStoredProgram"/>.
        /// </summary>
        private BOOSE_Canvas canvas;

        /// <summary>
        /// Initializes a fresh program and canvas before each test
        /// to ensure test isolation and a clean runtime state.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            canvas = new BOOSE_Canvas(new System.Windows.Forms.PictureBox());
            program = new AppStoredProgram(canvas);
        }

        /// <summary>
        /// Verifies that assigning an integer value using <see cref="AppInt"/>
        /// correctly stores the expected value in the program.
        /// </summary>
        [TestMethod]
        public void AppInt_Assignment_SetsCorrectValue()
        {
            var cmd = new AppInt();
            cmd.Set(program, "score = 100");
            cmd.Compile();
            cmd.Execute();
            var stored = (AppInt)program.GetVariable("score");
            Assert.AreEqual(100, stored.Value);
        }

        /// <summary>
        /// Verifies that assigning a real number preserves decimal precision
        /// when using <see cref="AppReal"/>.
        /// </summary>
        [TestMethod]
        public void AppReal_Assignment_PreservesDecimals()
        {
            var cmd = new AppReal();
            cmd.Set(program, "pi = 3.1415926535");
            cmd.Execute();
            var stored = (AppReal)program.GetVariable("pi");
            Assert.AreEqual(3.1415926535, stored.RealValue, 0.0000001);
        }

        /// <summary>
        /// Verifies that a boolean expression evaluates to true
        /// and is correctly stored using <see cref="AppBoolean"/>.
        /// </summary>
        [TestMethod]
        public void AppBoolean_Assignment_SetsTrue()
        {
            var cmd = new AppBoolean();
            cmd.Set(program, "flag = 1 > 0");
            cmd.Execute();
            var stored = (AppBoolean)program.GetVariable("flag");
            Assert.IsTrue(stored.BoolValue);
        }

        /// <summary>
        /// Verifies that a compound real expression preserves precision
        /// during evaluation and assignment.
        /// </summary>
        [TestMethod]
        public void AppReal_CompoundExpression_PreservesPrecision()
        {
            var cmd = new AppReal();
            cmd.Set(program, "result = 12.75 * 2.0 + 3.5");
            cmd.Execute();
            var stored = (AppReal)program.GetVariable("result");
            Assert.AreEqual(29.0, stored.RealValue, 0.00001);
        }

        /// <summary>
        /// Verifies that multiplication of real numbers
        /// is evaluated and assigned correctly.
        /// </summary>
        [TestMethod]
        public void AppReal_MultiplicationAssignment()
        {
            var cmd = new AppReal();
            cmd.Set(program, "area = 5.5 * 4.0");
            cmd.Execute();

            var stored = (AppReal)program.GetVariable("area");
            Assert.AreEqual(22.0, stored.RealValue, 0.00001);
        }

        /// <summary>
        /// Verifies that a compound boolean condition using logical AND
        /// evaluates to true and is stored correctly.
        /// </summary>
        [TestMethod]
        public void AppBoolean_CompoundCondition_SetsTrue()
        {
            var cmd = new AppBoolean();
            cmd.Set(program, "valid = (10 > 5) && (8 < 12)");
            cmd.Execute();

            var stored = (AppBoolean)program.GetVariable("valid");
            Assert.IsTrue(stored.BoolValue);
        }

        /// <summary>
        /// Verifies that subtraction of real numbers
        /// is evaluated and assigned correctly.
        /// </summary>
        [TestMethod]
        public void AppReal_SubtractionAssignment()
        {
            var cmd = new AppReal();
            cmd.Set(program, "balance = 100.75 - 25.25");
            cmd.Execute();

            var stored = (AppReal)program.GetVariable("balance");
            Assert.AreEqual(75.5, stored.RealValue, 0.00001);
        }
    }
}
