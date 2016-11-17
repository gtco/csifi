using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace csifi
{
    public class Interpreter : MemoryReader
    {
        public byte[] Buffer { get; set; }
        public Stack<Frame> Stack { get; set; }
        public bool Running { get; set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private AbbreviationTable _abbreviationTable;
        private Dictionary _dictionary;
        private ObjectTable _objectTable;
        private Globals _globals;
        private Dictionary<Instruction, Func<Instruction, Frame, Frame>> _functions = new Dictionary<Instruction, Func<Instruction, Frame, Frame>>();

        public bool LoadFile(string filename)
        {
            try
            {
                Buffer = File.ReadAllBytes(filename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        public bool Init()
        {
            InitFunctionTable();

            Stack = new Stack<Frame>();
            var initialPc = GetWord(Buffer, Header.InitialPc);

            // create abbreviation table
            _abbreviationTable = new AbbreviationTable();
            if (!_abbreviationTable.Init(Buffer, GetWord(Buffer, Header.AbbrTable)))
            {
                Logger.Error("Failed to load abbreviation table");
                return false;
            }

            // create dictionary
            _dictionary = new Dictionary(GetWord(Buffer, Header.Dictionary));
            if (!_dictionary.Init(Buffer))
            {
                Logger.Error("Failed to load dictionary");
                return false;
            }

            // create object table
            _objectTable = new ObjectTable(GetWord(Buffer, Header.ObjectTable));
            if (!_objectTable.Init(Buffer, _abbreviationTable))
            {
                Logger.Error("Failed to load object table");
                return false;
            }

            // load global variables
            _globals = new Globals(GetWord(Buffer, Header.GlobalVar));
            if (!_globals.Init(Buffer))
            {
                Logger.Error("Failed to load global variables");
                return false;
            }

            Logger.Debug($"Starting execution at {initialPc}");

            // Create initial frame and push it on stack
            Stack.Push(new Frame(initialPc));

            // TODO create window

            return true;
        }

        public void Run()
        {
            Running = true;

            do
            {
                var frame = Stack.Peek();
                var i =  frame.GetNextInstruction(Buffer);

                if (i != null)
                {
                    var func = _functions[i];
                    Logger.Debug(func.Method.Name);
                    func.Invoke(i, frame);
                }
                else
                {
                    Logger.Error($"Halting with empty instruction at {frame.PC}");
                    Running = false;
                }
                
            } while (Running);
        }

        private void InitFunctionTable()
        {
            _functions.Add(new Instruction(0X00, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X01, InstructionType.TwoOp), JumpEqual);
            _functions.Add(new Instruction(0X02, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X03, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X04, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X05, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X06, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X07, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X08, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X09, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X0A, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X0B, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X0C, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X0D, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X0E, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X0F, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X10, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X11, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X12, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X13, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X14, InstructionType.TwoOp), Add);
            _functions.Add(new Instruction(0X15, InstructionType.TwoOp), Sub);
            _functions.Add(new Instruction(0X16, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X17, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X18, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X19, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X1A, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X1B, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X1C, InstructionType.TwoOp), NotImplemented);

            _functions.Add(new Instruction(0X00, InstructionType.OneOp), JumpZero);
            _functions.Add(new Instruction(0X01, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X02, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X03, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X04, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X05, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X06, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X07, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X08, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X09, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X0A, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X0B, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X0C, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X0D, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X0E, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X0F, InstructionType.OneOp), NotImplemented);

            _functions.Add(new Instruction(0X00, InstructionType.ZeroOp), Rtrue);
            _functions.Add(new Instruction(0X01, InstructionType.ZeroOp), Rfalse);
            _functions.Add(new Instruction(0X02, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X03, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X04, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X05, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X06, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X07, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X08, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X09, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X0A, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X0B, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X0C, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X0D, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X0E, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X0F, InstructionType.ZeroOp), NotImplemented);

            _functions.Add(new Instruction(0x00, InstructionType.Var), CallFv);
            _functions.Add(new Instruction(0x01, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0x02, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0x03, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0x04, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0x05, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0x06, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0x07, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0x08, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0x09, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X0A, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X0B, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X0C, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X0D, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X0E, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X0F, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X10, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X11, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X12, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X13, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X14, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X15, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X16, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X17, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X18, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X19, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X1A, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X1B, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X1C, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X1D, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X1E, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0X1F, InstructionType.Var), NotImplemented);

        }

        private void SaveResult(int index, int value, Frame f)
        {
            if (index < 0x10)
            {
                f.SetLocal(index, value);
            }
            else if (index < 0xff)
            {
                _globals.Set(index - 16, value);
            }
            else
            {
                Logger.Error($"StoreResult failed, invalid destination = {index}");
            }
        }

        private int GetVariableValue(int index, Frame f)
        {
            if (index < 0x10)
            {
                return f.GetLocal(index);
            }

            if (index < 0xff)
            {
                return _globals.Get(index-16);
            }

            Logger.Error($"GetVariableValue failed, invalid index = {index}");
            throw new ArgumentOutOfRangeException();
        }

        private int GetValue(Operand operand, Frame frame)
        {
            if (operand.Type == OperandType.LargeConst || operand.Type == OperandType.SmallConst)
            {
                return operand.Value;
            }

            if (operand.Type == OperandType.Variable)
            {
                return GetVariableValue(operand.Value, frame);
            }

            throw new ArgumentOutOfRangeException();
        }

        private Frame Add(Instruction i, Frame f)
        {
            var sum = (GetValue(i.Operands[0], f) + GetValue(i.Operands[1], f)) % 0x10000;
            int dest = f.GetByte(Buffer, f.PC++);
            SaveResult(dest, sum, f);
            Logger.Debug($"ADD: {dest} = {sum}");
            f.PrintLocals();
            return f;
        }

        private Frame Sub(Instruction i, Frame f)
        {
            var difference = (GetValue(i.Operands[0], f) - GetValue(i.Operands[1], f)) % 0x10000;
            int dest = f.GetByte(Buffer, f.PC++);
            SaveResult(dest, difference, f);
            Logger.Debug($"SUB: {dest} = {difference}");
            f.PrintLocals();
            return f;
        }

        public Frame Rtrue(Instruction i, Frame f)
        {
            Stack.Pop();
            var nextFrame = Stack.Peek();
            int dest = GetByte(Buffer, nextFrame.PC++);
            Logger.Debug("RTRUE, dest " + dest + " value 1 ");
            SaveResult(dest, 1, f);
            f.PrintLocals();
            return nextFrame;
        }

        public Frame Rfalse(Instruction i, Frame f)
        {
            Stack.Pop();
            var nextFrame = Stack.Peek();
            int dest = GetByte(Buffer, nextFrame.PC++);
            Logger.Debug("RFALSE, dest " + dest + " value 0 ");
            SaveResult(dest, 0, f);
            f.PrintLocals();
            return nextFrame;
        }

        private Frame JumpEqual(Instruction i, Frame f)
        {
            int b = GetByte(Buffer, f.PC++);
            /*
             * If bit 7 of the first byte is 0, a branch occurs on false; if 1, then
             * branch is on true.
             */
            var condt = ((b & 0x80) == 0x80);
            var offset = GetBranchOffset(b,f);
            var a = GetValue(i.Operands[0], f) & 0xff;
            Logger.Debug("JE : a = " + a);
            var b1 = GetValue(i.Operands[1], f) & 0xff;
            Logger.Debug("JE : b1 = " + b1);
            var eq = (a == b1);
            if (!eq && i.Operands.Count > 2)
            {

                var b2 = GetValue(i.Operands[2], f);
                Logger.Debug("JE : b2 = " + a);
                eq = (a == b2);
            }
            if (!eq && i.Operands.Count > 3)
            {
                var b3 = GetValue(i.Operands[3], f);
                Logger.Debug("JE : b3 = " + a);
                eq = (a == b3);
            }

            return DoBranch(eq, condt, offset, i, f);
        }

        public Frame JumpZero(Instruction i, Frame f)
        {
            int b = GetByte(Buffer, f.PC++);
            /*
             * If bit 7 of the first byte is 0, a branch occurs on false; if 1, then
             * branch is on true.
             */
            var condt = ((b & 0x80) == 0x80);
            var offset = GetBranchOffset(b, f);
            var v = GetValue(i.Operands[0], f) & 0xff;
            Logger.Debug("JZ, v = " + v);
            var eq = (v == 0);
            return DoBranch(eq, condt, offset, i, f);
        }

        public int GetBranchOffset(int control, Frame f)
        {
            var offset = control & 0x3f;
            if ((control & 0x040) == 0)
            {
                // if "bit 6" is not set, address consists of the six (low) bits
                // of the first byte plus the next 8 bits.
                int n = GetByte(Buffer, f.PC++);
                offset = (offset << 8) + n;
                if ((offset & 0x02000) > 0)
                {
                    offset |= 0xc000;
                }
            }
            return offset;
        }

        public Frame DoBranch(bool eq, bool condt, int offset, Instruction i, Frame f)
        {
            if (eq == condt)
            {
                if (offset == 0)
                {
                    Logger.Debug(i.Opcode + ": doBranch, RFALSE");
                    return Rfalse(i, f);
                }

                if (offset == 1)
                {
                    Logger.Debug(i.Opcode + ": doBranch, RTRUE");
                    return Rtrue(i, f);
                }

                var addr = f.PC + offset - 2;
                Logger.Debug($" {i.Opcode} : jumping: {addr:X4}, offset " + offset);
                f.PC = addr;
                return f;
            }
            else
            {
                Logger.Debug(i.Opcode + " : branch failed: eq=" + eq + ", condt=" + condt);
            }

            return null;
        }

        private Frame CallFv(Instruction instruction, Frame currentFrame)
        {
            var pc = (instruction.Operands[0].Value) * 2;

            if (pc == 0)
            {
                Logger.Error($"call_fv: raddr is null, pc = {currentFrame.PC}");
                SaveResult(GetByte(Buffer, currentFrame.PC++), 0, currentFrame);
            }

            int c = GetByte(Buffer, pc++);
            Logger.Debug($"call_fv: n {(instruction.Operands.Count - 1)}, L {c}");
            var f = new Frame(pc);
            Logger.Debug($"CALL : From ${currentFrame.PC} To {f.PC - 1}, Locals [{c}]");
            f.CopyLocals(Buffer, c, currentFrame, instruction.Operands);
            Stack.Push(f);

            return Stack.Peek();
        }

        private Frame NotImplemented(Instruction instruction, Frame currentFrame)
        {
            Logger.Error($"Opcode [{instruction.Opcode:X2}] is not implented.");
            throw new NotImplementedException();
        }

    }
}
