using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace csifi.Test
{
    [TestClass]
    public class TestInstructionParser
    {
        [TestMethod]
        public void TestCallInstructionOne()
        {
            var buffer = new[] { (byte)0xe0, (byte)0x3f, (byte)0x28, (byte)0x68, (byte)0x00 };
            var instruction = new Instruction(buffer[0]);
            instruction.Read(1, buffer);
            Assert.IsTrue(instruction.Operands.Count == 1);
            Assert.IsTrue(instruction.Opcode == 0);
            Assert.IsTrue(instruction.Type == InstructionType.Var);
        }

        [TestMethod]
        public void TestCallInstructionTwo()
        {
            var buffer = new[] {(byte)0xe0, (byte)0x1f, (byte)0x4a, (byte)0x98, (byte)0xae, (byte)0x00 };
            var instruction = new Instruction(buffer[0]);
            instruction.Read(1, buffer);
            Assert.IsTrue(instruction.Operands.Count == 2);
            Assert.IsTrue(instruction.Opcode == 0);
            Assert.IsTrue(instruction.Type == InstructionType.Var);
        }

        [TestMethod]
        public void TestCallInstructionThree()
        {
            var buffer = new[] { (byte)0xe0, (byte)0x27, (byte)0x2a, (byte)0x43, (byte)0x01, (byte)0x01, (byte)0x03 };
            var instruction = new Instruction(buffer[0]);
            instruction.Read(1, buffer);
            Assert.IsTrue(instruction.Operands.Count == 3);
            Assert.IsTrue(instruction.Opcode == 0);
            Assert.IsTrue(instruction.Type == InstructionType.Var);
        }

        [TestMethod]
        public void TestRtrueInstruction()
        {
            var buffer = new[] { (byte)0xb0 };
            var instruction = new Instruction(buffer[0]);
            instruction.Read(1, buffer);
            Assert.IsTrue(instruction.Operands.Count == 0);
            Assert.IsTrue(instruction.Opcode == 0);
            Assert.IsTrue(instruction.Type == InstructionType.ZeroOp);

        }

        [TestMethod]
        public void TestIncInstruction()
        {
            var buffer = new[] { (byte)0x95, (byte)0x03 };
            var instruction = new Instruction(buffer[0]);
            instruction.Read(1, buffer);
            Assert.IsTrue(instruction.Operands.Count == 1);
            Assert.IsTrue(instruction.Opcode == 5);
            Assert.IsTrue(instruction.Type == InstructionType.OneOp);

        }

        [TestMethod]
        public void TestStoreInstruction()
        {
            var buffer = new[] { (byte)0x2d, (byte)0x01, (byte)0x65 };
            var instruction = new Instruction(buffer[0]);
            instruction.Read(1, buffer);
            Assert.IsTrue(instruction.Operands.Count == 2);
            Assert.IsTrue(instruction.Opcode == 13);
            Assert.IsTrue(instruction.Type == InstructionType.TwoOp);

        }

    }
}
