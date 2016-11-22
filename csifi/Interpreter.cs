using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace csifi
{
    public class Interpreter : MemoryWriter
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
        private Window _window;

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
            _window = new Window();

            return true;
        }

        public void Run()
        {
            Running = true;
            int count = 0;

            do
            {
                var frame = Stack.Peek();
                Logger.Debug($"count:{++count} pc:{frame.PC} ({frame.PC:X4})");
                var i =  frame.GetNextInstruction(Buffer);

                if (i != null)
                {
                    var func = _functions[i];
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
            _functions.Add(new Instruction(0X02, InstructionType.TwoOp), Jl);
            _functions.Add(new Instruction(0X03, InstructionType.TwoOp), Jg);
            _functions.Add(new Instruction(0X04, InstructionType.TwoOp), DecChk);
            _functions.Add(new Instruction(0X05, InstructionType.TwoOp), IncChk);
            _functions.Add(new Instruction(0X06, InstructionType.TwoOp), Jin);
            _functions.Add(new Instruction(0X07, InstructionType.TwoOp), Test);
            _functions.Add(new Instruction(0X08, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X09, InstructionType.TwoOp), And);
            _functions.Add(new Instruction(0X0A, InstructionType.TwoOp), TestAttribute);
            _functions.Add(new Instruction(0X0B, InstructionType.TwoOp), SetAttribute);
            _functions.Add(new Instruction(0X0C, InstructionType.TwoOp), ClearAttribute);
            _functions.Add(new Instruction(0X0D, InstructionType.TwoOp), Store);
            _functions.Add(new Instruction(0X0E, InstructionType.TwoOp), InsertObject);
            _functions.Add(new Instruction(0X0F, InstructionType.TwoOp), Loadw);
            _functions.Add(new Instruction(0X10, InstructionType.TwoOp), Loadb);
            _functions.Add(new Instruction(0X11, InstructionType.TwoOp), GetProp);
            _functions.Add(new Instruction(0X12, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X13, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X14, InstructionType.TwoOp), Add);
            _functions.Add(new Instruction(0X15, InstructionType.TwoOp), Sub);
            _functions.Add(new Instruction(0X16, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X17, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X18, InstructionType.TwoOp), Mod);
            _functions.Add(new Instruction(0X19, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X1A, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X1B, InstructionType.TwoOp), NotImplemented);
            _functions.Add(new Instruction(0X1C, InstructionType.TwoOp), NotImplemented);

            _functions.Add(new Instruction(0X00, InstructionType.OneOp), JumpZero);
            _functions.Add(new Instruction(0X01, InstructionType.OneOp), GetSibling);
            _functions.Add(new Instruction(0X02, InstructionType.OneOp), GetChild);
            _functions.Add(new Instruction(0X03, InstructionType.OneOp), GetParent);
            _functions.Add(new Instruction(0X04, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X05, InstructionType.OneOp), Inc);
            _functions.Add(new Instruction(0X06, InstructionType.OneOp), Dec);
            _functions.Add(new Instruction(0X07, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X08, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X09, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X0A, InstructionType.OneOp), PrintObject);
            _functions.Add(new Instruction(0X0B, InstructionType.OneOp), Return);
            _functions.Add(new Instruction(0X0C, InstructionType.OneOp), Jump);
            _functions.Add(new Instruction(0X0D, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X0E, InstructionType.OneOp), NotImplemented);
            _functions.Add(new Instruction(0X0F, InstructionType.OneOp), NotImplemented);

            _functions.Add(new Instruction(0X00, InstructionType.ZeroOp), Rtrue);
            _functions.Add(new Instruction(0X01, InstructionType.ZeroOp), Rfalse);
            _functions.Add(new Instruction(0X02, InstructionType.ZeroOp), Print);
            _functions.Add(new Instruction(0X03, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X04, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X05, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X06, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X07, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X08, InstructionType.ZeroOp), RetPopped);
            _functions.Add(new Instruction(0X09, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X0A, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X0B, InstructionType.ZeroOp), NewLine);
            _functions.Add(new Instruction(0X0C, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X0D, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X0E, InstructionType.ZeroOp), NotImplemented);
            _functions.Add(new Instruction(0X0F, InstructionType.ZeroOp), NotImplemented);

            _functions.Add(new Instruction(0x00, InstructionType.Var), CallFv);
            _functions.Add(new Instruction(0x01, InstructionType.Var), Storew);
            _functions.Add(new Instruction(0x02, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0x03, InstructionType.Var), PutProp);
            _functions.Add(new Instruction(0x04, InstructionType.Var), Sread);
            _functions.Add(new Instruction(0x05, InstructionType.Var), PrintChar);
            _functions.Add(new Instruction(0x06, InstructionType.Var), PrintNum);
            _functions.Add(new Instruction(0x07, InstructionType.Var), NotImplemented);
            _functions.Add(new Instruction(0x08, InstructionType.Var), Push);
            _functions.Add(new Instruction(0x09, InstructionType.Var), Pull);
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
            //f.PrintLocals();
            return f;
        }

        private Frame Sub(Instruction i, Frame f)
        {
            var difference = (GetValue(i.Operands[0], f) - GetValue(i.Operands[1], f)) % 0x10000;
            int dest = f.GetByte(Buffer, f.PC++);
            SaveResult(dest, difference, f);
            Logger.Debug($"SUB: {dest} = {difference}");
            //f.PrintLocals();
            return f;
        }

        public Frame Loadb(Instruction i, Frame f)
        {
            int result = f.GetByte(Buffer, GetValue(i.Operands[0], f) + GetValue(i.Operands[1], f));
            int dest = f.GetByte(Buffer, f.PC++);
            SaveResult(dest, result, f);
            Logger.Debug($"LOADB : result [{result}] dest [{dest}]");
            return f;
        }

        public Frame Loadw(Instruction i, Frame f)
        {
            /*
             * LOADW baddr n <result> --- 2OP:$F The result is the word at baddr +2 *n
             * LOADB baddr n <result> --- 2OP:$10 The result is the byte at baddr +n.
             * 
             */
            int array = GetValue(i.Operands[0], f); //arg1.getValue(this));
            int index = GetValue(i.Operands[1], f) & 0xffff;
            int result = GetWord(Buffer, array + (index * 2));
            int dest = GetByte(Buffer, f.PC++); //map.getByte(routine.next()));
            SaveResult(dest, result, f);
            Logger.Debug($"LOADW : array {array}, index {index}, result {result}, dest {dest}");

            return f;
        }

        public Frame Storew(Instruction i, Frame f)
        {
            var addr = GetValue(i.Operands[0], f);
            var mult = GetValue(i.Operands[1], f);
            var value = GetValue(i.Operands[2], f);
            addr = addr + 2 * mult;
            SetWord(Buffer, addr, value);
            Logger.Debug($"STOREW : storing {value} at addr {addr} plus 2 times {mult}");
            return f;
        }

        public Frame Store(Instruction i, Frame f)
        {
            var destination = GetValue(i.Operands[0], f);
            var value = GetValue(i.Operands[1], f);
            SaveResult(destination, value, f);
            Logger.Debug($"STORE : destination [{destination}], value [{value}]");
            return f;
        }

        public Frame PutProp(Instruction i, Frame f)
        {
            var objectNumber = GetValue(i.Operands[0], f);
            var propertyNumber = GetValue(i.Operands[1], f);
            var propertyValue = GetValue(i.Operands[2], f);
            var o = _objectTable.GetObject(objectNumber - 1);

            if (o.Properties.ContainsKey(propertyNumber))
            {
                o.Properties[propertyNumber] = new List<int>() { propertyValue };
            }
            else
            {
                Logger.Error($"PUT_PROP : No Property [{propertyNumber}] for Object [{objectNumber}]");
            }

            return f;
        }

        public Frame RetPopped(Instruction i, Frame f)
        {
            int value = f.GetLocal(0);
            Stack.Pop();
            var nextFrame = Stack.Peek();
            int dest = GetByte(Buffer, nextFrame.PC++);
            Logger.Debug("RET_POPPED : dest =" + dest + ", value = " + value);
            SaveResult(dest, value, nextFrame);
            nextFrame.PrintLocals();
            return nextFrame;
        }

        public Frame Rtrue(Instruction i, Frame f)
        {
            Stack.Pop();
            var nextFrame = Stack.Peek();
            int dest = GetByte(Buffer, nextFrame.PC++);
            Logger.Debug("RTRUE, dest " + dest + " value 1 ");
            SaveResult(dest, 1, nextFrame);
            nextFrame.PrintLocals();
            return nextFrame;
        }

        public Frame Rfalse(Instruction i, Frame f)
        {
            Stack.Pop();
            var nextFrame = Stack.Peek();
            int dest = GetByte(Buffer, nextFrame.PC++);
            Logger.Debug("RFALSE, dest " + dest + " value 0 ");
            SaveResult(dest, 0, nextFrame);
            nextFrame.PrintLocals();
            return nextFrame;
        }

        public Frame Jump(Instruction i, Frame f)
        {
            var n = GetValue(i.Operands[0], f);
            if ((n & 0x8000) == 0x8000)
            {
                // negative, convert two's complement
                n = -(--n ^ 0xFFFF);
            }

            var addr = f.PC + n - 2;
            f.PC = addr;
            Logger.Debug($"JUMP : arg [{i}] offset [" + n + "] addr [" + addr + "]");
            return f;
        }

        public Frame And(Instruction i, Frame f)
        {
            var a = GetValue(i.Operands[0], f);
            var b = GetValue(i.Operands[1], f);
            var result = a & b;
            var dest = f.GetByte(Buffer, f.PC++);
            SaveResult(dest, result, f);
            Logger.Debug($"AND : {a} & {b} result [{result}] dest [{dest}]");
            return f;
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
                Logger.Debug("JE : b2 = " + b2);
                eq = (a == b2);
            }
            if (!eq && i.Operands.Count > 3)
            {
                var b3 = GetValue(i.Operands[3], f);
                Logger.Debug("JE : b3 = " + b3);
                eq = (a == b3);
            }

            return Branch(eq, condt, offset, i, f);
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
            return Branch(eq, condt, offset, i, f);
        }

        public Frame IncChk(Instruction i, Frame f)
        {
            int b = GetByte(Buffer, f.PC++);
            bool condt = ((b & 0x80) == 0x80);
            int offset = GetBranchOffset(b,f);
            int local = GetValue(i.Operands[0], f); 
            int n = f.GetLocal(local);
            n = n + 1;
            n &= 0xffff;
            SaveResult(local, n, f);

            int j = GetValue(i.Operands[1], f); 
            bool eq = n > j;
            return Branch(eq, condt, offset, i, f);
        }

        public Frame DecChk(Instruction i, Frame f)
        {
            int b = GetByte(Buffer, f.PC++);
            bool condt = ((b & 0x80) == 0x80);
            int offset = GetBranchOffset(b, f);
            int local = GetValue(i.Operands[0], f);
            int n = f.GetLocal(local);
            n = n - 1;
            SaveResult(local, n, f);

            int j = GetValue(i.Operands[1], f); 
            bool eq = n > j;
            return Branch(eq, condt, offset, i, f);
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

        public Frame Branch(bool eq, bool condt, int offset, Instruction i, Frame f)
        {
            if (eq == condt)
            {
                if (offset == 0)
                {
                    Logger.Debug(i.Opcode + ": Branch, RFALSE");
                    return Rfalse(i, f);
                }

                if (offset == 1)
                {
                    Logger.Debug(i.Opcode + ": Branch, RTRUE");
                    return Rtrue(i, f);
                }

                var addr = f.PC + offset - 2;
                Logger.Debug($" {i.Opcode} : jumping: {addr} ({addr:X4}), offset " + offset);
                f.PC = addr;
                return f;
            }
            else
            {
                Logger.Debug(i.Opcode + " : Branch failed: eq=" + eq + ", condt=" + condt);
            }

            return null;
        }

        private Frame CallFv(Instruction instruction, Frame currentFrame)
        {
            var pc = GetValue(instruction.Operands[0], currentFrame) * 2;

            if (pc == 0)
            {
                Logger.Error($"call_fv: raddr is null, pc = {currentFrame.PC}");
                SaveResult(GetByte(Buffer, currentFrame.PC++), 0, currentFrame);
                return currentFrame;
            }

            int c = GetByte(Buffer, pc++);
            Logger.Debug($"call_fv: n {(instruction.Operands.Count - 1)}, L {c}");

            var f = new Frame(pc);
            Logger.Debug($"CALL : From ${currentFrame.PC} To {f.PC - 1}, Locals [{c}]");
            for (var i = 1; i <= c; i++)
            {
                var localValue = GetWord(Buffer, f.PC);
                f.PC += 2;

                if (i < instruction.Operands.Count)
                {
                    var v = GetValue(instruction.Operands[i], currentFrame);
                    f.SetLocal(i, v);
                    Logger.Debug($"Copy [{i}] {v}");
                }
                else
                {
                    f.SetLocal(i, localValue);
                    Logger.Debug($"Copy [{i}] {localValue} (local value)");
                }
            }

            Stack.Push(f);
           
            return Stack.Peek();
        }

        public Frame Return(Instruction instruction, Frame f)
        {
            // TODO check the call stack to determine invocation method
            var value = GetValue(instruction.Operands[0], f);
            Stack.Pop();
            var nextFrame = Stack.Peek();
            int dest = GetByte(Buffer, nextFrame.PC++);
            Logger.Debug($"RET : dest {dest} value {value}");
            SaveResult(dest, value, nextFrame);
            nextFrame.PrintLocals();

            return nextFrame;
        }

        public Frame TestAttribute(Instruction instruction, Frame f)
        {
            var obj = GetValue(instruction.Operands[0], f) & 0xff;
            var value = GetValue(instruction.Operands[1], f);
//            bool eq = table.testAttribute(obj, value);
            var o = _objectTable.GetObject(obj);
            var eq = o.TestAttribute(value);
            var b = GetByte(Buffer, f.PC++);

            /*
             * If bit 7 of the first byte is 0, a branch occurs on false; if 1, then
             * branch is on true.
             */
            var condt = ((b & 0x80) == 0x80);
            var offset = GetBranchOffset(b,f);
            var addr = f.PC + offset - 2;

            Logger.Debug($"TEST_ATTR : Testing Object = {o.Name}, Attribute = {value}, Result = {eq}, Condt = {condt}, Destination = {addr}");

            return Branch(eq, condt, offset, instruction, f);
        }

        public Frame SetAttribute(Instruction instruction, Frame f)
        {
            var obj = GetValue(instruction.Operands[0], f) & 0xff;
            var index = GetValue(instruction.Operands[1], f);
            var o = _objectTable.GetObject(obj);

            o.SetAttribute(index);

            return f;
        }

        public Frame ClearAttribute(Instruction instruction, Frame f)
        {
            var obj = GetValue(instruction.Operands[0], f) & 0xff;
            var index = GetValue(instruction.Operands[1], f);
            var o = _objectTable.GetObject(obj);

            o.ClearAttribute(index);

            return f;
        }

        public Frame NewLine(Instruction instruction, Frame f)
        {
            _window.NewLine();
            return f;
        }

        public Frame InsertObject(Instruction instruction, Frame f)
        {
            var target = _objectTable.GetObject(GetValue(instruction.Operands[0], f));
            var destination = _objectTable.GetObject(GetValue(instruction.Operands[1], f));
            destination.AddChild(target);
            Logger.Debug("INSERT_OBJ, Success : [" + target.Name + "] -> [" + destination.Name + "]");
            return f;
        }

        public Frame PrintObject(Instruction instruction, Frame f)
        {
            int i = GetValue(instruction.Operands[0], f);
            string n = _objectTable.GetObject(i).Name;
            Logger.Debug("print_obj=" + n);
            _window.Print(n);
            return f;
        }

        public Frame PrintChar(Instruction i, Frame f)
        {
            int n = GetValue(i.Operands[0], f);
            char c = (char)n;
            Logger.Debug("print_char=" + c);
            _window.Print(c.ToString());
            return f;
        }

        public Frame PrintNum(Instruction i, Frame f)
        {
            var n = GetValue(i.Operands[0], f);
            var s = n.ToString();
            Logger.Debug("print_num=" + s);
            _window.Print(s);
            return f;
        }

        public Frame Print(Instruction i, Frame f)
        {
            bool end;
            Text text = new Text();

            do
            {
                var w = GetWord(Buffer, f.PC);
                text.AddCharacters(w);
                f.PC += 2; 
                end = (w & 0x8000) == 0x8000;

            } while (!end);

            string s = text.GetValue(_abbreviationTable);
            _window.Print(s);
            Logger.Debug("print_text=" + s);

            return f;
        }

        public Frame Push(Instruction i, Frame f)
        {
            var n = GetValue(i.Operands[0], f);
            SaveResult(0, n, f);
            return f;
        }

        public Frame Pull(Instruction i, Frame f)
        {
            var n = f.GetLocal(0);
            var local = GetValue(i.Operands[0], f);
            SaveResult(local, n, f);
            return f;
        }

        public Frame Jin(Instruction i, Frame f)
        {
            int b = GetByte(Buffer, f.PC++);
            var condt = ((b & 0x80) == 0x80);
            var offset = GetBranchOffset(b, f);
            var child = GetValue(i.Operands[0], f);
            var parent = GetValue(i.Operands[1], f);
            var eq = _objectTable.IsParent(parent, child);
            Logger.Debug("JIN: child = " + child + ", parent = " + parent + ", isParent = " + eq);
            return Branch(eq, condt, offset, i, f);
        }

        public Frame Jl(Instruction i, Frame f)
        {
            int b = GetByte(Buffer, f.PC++);
            bool condt = ((b & 0x80) == 0x80);
            int offset = GetBranchOffset(b, f);
            int s = GetValue(i.Operands[0], f);
            int t = GetValue(i.Operands[1], f);
            Logger.Debug("JL : s=" + s + ", t=" + t);
            bool eq = (s < t);
            return Branch(eq, condt, offset, i, f);
        }

        public Frame Jg(Instruction i, Frame f)
        {
            int b = GetByte(Buffer, f.PC++);
            bool condt = ((b & 0x80) == 0x80);
            int offset = GetBranchOffset(b, f);
            int s = GetValue(i.Operands[0], f);
            int t = GetValue(i.Operands[1], f);
            Logger.Debug("JG : s=" + s + ", t=" + t);
            bool eq = (s > t);
            return Branch(eq, condt, offset, i, f);
        }

        public Frame Mod(Instruction i, Frame f)
        {
            var s = GetValue(i.Operands[0], f);
            var t = GetValue(i.Operands[1], f);
            int dest = GetByte(Buffer, f.PC++);

            var m = Math.Floor((decimal) s / (decimal) t);
            int result = (int) ((s - t) * m);
            result %= 0x10000;
            SaveResult(dest, result, f);
            return f;
        }

        public Frame Test(Instruction i, Frame f)
        {
            int b = GetByte(Buffer, f.PC++);
            bool condt = ((b & 0x80) == 0x80);
            int offset = GetBranchOffset(b, f);
            int s = GetValue(i.Operands[0], f);
            int t = GetValue(i.Operands[1], f);
            Logger.Debug("TEST : s=" + s + ", t=" + t);
            bool eq = (s & t) == t;
            return Branch(eq, condt, offset, i, f);
        }

        public Frame GetParent(Instruction i, Frame f)
        {
            var index = GetValue(i.Operands[0], f);
            var obj = _objectTable.GetObject(index);
            var parent = _objectTable.GetObject(obj.Parent);
            var dest = f.GetByte(Buffer, f.PC++);
            Logger.Debug("GET_PARENT [" + obj.Name + "] -> [" + parent.Name + "]");
            SaveResult(dest, obj.Parent, f);
            return f;
        }

        public Frame GetProp(Instruction i, Frame f)
        {
            /*
             * get_prop obj prop <result>� 2OP:$11
                The result is the 1st word (if the property length is 2) or byte (if it is one) of property prop
                on object obj, if it is present. Otherwise the result is the default property word stored in the
                property defaults table. The result is unspecified if the property is present but does not have
                length 1 or 2.	
             */
            var obj = GetValue(i.Operands[0], f);
            var prop = GetValue(i.Operands[1], f);
            var o = _objectTable.GetObject(obj);

            Logger.Debug("GET_PROP : Getting property [" + prop + "] for Object [" + obj + ":" + o.Name + "], Property");

            var op = _objectTable.GetObjectProperty(obj, prop);
            var dest = f.GetByte(Buffer, f.PC++);
            SaveResult(dest, op, f);
            return f;
        }

        public Frame GetChild(Instruction i, Frame f)
        {
            var index = GetValue(i.Operands[0], f);
            var obj = _objectTable.GetObject(index);
            var child = _objectTable.GetObject(obj.Child);
            var dest = f.GetByte(Buffer, f.PC++);
            Logger.Debug("GET_CHILD [" + obj.Name + "] -> [" + child.Name + "]");
            SaveResult(dest, obj.Child, f);

            int label = f.GetByte(Buffer, f.PC++);
            bool condt = ((label & 0x80) == 0x80);
            bool eq = obj.Child != 0;
            int offset = GetBranchOffset(label,f);
            Logger.Debug("GET_CHILD, condt = " + condt + ", eq = " + eq + ", offset = " + offset);
            return Branch(eq, condt, offset, i, f);
        }

        public Frame GetSibling(Instruction i, Frame f)
        {
            var index = GetValue(i.Operands[0], f);
            var obj = _objectTable.GetObject(index);
            var sibling = _objectTable.GetObject(obj.Sibling);
            var dest = f.GetByte(Buffer, f.PC++);
            Logger.Debug("GET_SIBLING [" + obj.Name + "] -> [" + sibling.Name + "]");
            SaveResult(dest, obj.Sibling, f);

            int label = f.GetByte(Buffer, f.PC++);
            bool condt = ((label & 0x80) == 0x80);
            bool eq = obj.Sibling != 0;
            int offset = GetBranchOffset(label, f);
            Logger.Debug("GET_SIBLING, condt = " + condt + ", eq = " + eq + ", offset = " + offset);
            return Branch(eq, condt, offset, i, f);
        }


        public Frame Inc(Instruction i, Frame f)
        {
            // TODO Increment the value of the variable with number var by 1, modulo $10000
            var local = GetValue(i.Operands[0], f);
            var n = f.GetLocal(local);
            n = n + 1;

            if (n > 0xffff)
            {
                Logger.Error($"INC Overflow at {f.PC}, local {local}");
                n = n % 0x10000;
            }

            SaveResult(local, n, f);
            return f;
        }

        public Frame Dec(Instruction i, Frame f)
        {
            var local = GetValue(i.Operands[0], f);
            var n = f.GetLocal(local);
            n = n - 1;

            if (n > 0xffff)
            {
                Logger.Error($"DEC Overflow at {f.PC}, local {local}");
                n = n % 0x10000;
            }

            SaveResult(local, n, f);
            return f;
        }

        public Frame Sread(Instruction i, Frame f)
        {
            Logger.Error($"SREAD [{i.Opcode:X2}] is not implemented.");
            throw new NotImplementedException();
        }

        private Frame NotImplemented(Instruction instruction, Frame currentFrame)
        {
            Logger.Error($"Opcode [{instruction.Opcode:X2}] is not implemented.");
            throw new NotImplementedException();
        }

    }
}
