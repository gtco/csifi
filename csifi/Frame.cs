using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace csifi
{
    public enum InvocationMethod
    {
        Function = 1,
        Procedure = 2,
        Interrupt = 3
    };

    public class Local
    {
        public int Index { get; set; }
    };

    public class Frame : MemoryReader
    {
        public const int LocalCount = 15;

        public int PC { get; set; }
        public Stack<int> Stack { get; set; }
        public List<int> Locals { get; set; }
        public InvocationMethod InvocationMethod { get; set; }
        public int ArgumentCount { get; set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        const int DefVal = int.MaxValue / 2;
        private Guid _guid;


        public Frame(int pc)
        {
            PC = pc;
            Stack = new Stack<int>();
            Locals = new List<int>();

            // Local variables numbered from 1 - 15
            for (var i = 0; i <= LocalCount; i++)
            {
                Locals.Add(DefVal);
            }

            InvocationMethod = InvocationMethod.Function;
            ArgumentCount = 0;
            _guid = new Guid();
        }

        public Instruction GetNextInstruction(byte[] buffer)
        {
            Logger.Debug($"pc: {PC} ({PC:X4})");
            var i = new Instruction(GetByte(buffer, PC++));

            if (!i.Read(PC, buffer))
            {
                Logger.Error($"Unknown Opcode {i:X2}");
                return null;
            }

            PC = i.PC;
            return i;
        }

        public void SetLocal(int index, int value)
        {
            if (index != 0)
            {
                Locals[index] = value;
            }
            else
            {
                Stack.Push(value);
            }
        }

        public int GetLocal(int index)
        {
            return index != 0 ? Locals[index] : Stack.Pop();
        }

        private int GetValueForOperand(Operand operand)
        {
            return (operand.Type != OperandType.Variable) ? operand.Value : GetLocal(operand.Value);
        }

        public void CopyLocals(byte[] buffer, int c, Frame frame, List<Operand> operands)
        {
            for (var i = 1; i <= c; i++)
            {
                var localValue = GetWord(buffer, PC);
                PC += 2;

                if (i < operands.Count)
                {
                    //TODO check out of bounds errors
                    var v = frame.GetValueForOperand(operands[i]);
                    SetLocal(i, v);
                    Logger.Debug($"Copy [{i}] {v}");
                }
                else
                {
                    SetLocal(i, localValue);
                    Logger.Debug($"Copy [{i}] {localValue} (local value)");
                }
            }
        }

        public void PrintLocals()
        {
            var s = "";
            if (Stack.Count > 0)
            {
                s += ("Stack (" + Stack.Peek() + ") ");
            }

            for (var j = 1; j <= LocalCount; j++)
            {
                if (j != 1)
                {
                    s += ", ";
                }
                s += (GetLocal(j) != DefVal) ? $"{j}:{GetLocal(j)}" : "";
            }
            Logger.Debug("Locals (" + s + ")");
        }

    }
}
