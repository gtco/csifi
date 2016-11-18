using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace csifi
{
    public enum InstructionType
    {
        ZeroOp,
        OneOp,
        TwoOp,
        Var,
        Ext
    }

    public enum OperandType
    {
        LargeConst,
        SmallConst,
        Variable,
        Omitted
    };


    public class Operand
    {
        public OperandType Type { get; set; }
        public int Value { get; set; }

        public Operand(OperandType type, int value)
        {
            Type = type;
            Value = value;
        }
    }

    public class Instruction : MemoryReader, IEquatable<Instruction>
    {
        public int Opcode { get; set; }
        public InstructionType Type { get; set; }
        public List<Operand> Operands { get; set; }
        public int PC { get; set; }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Instruction(int opcode)
        {
            Opcode = opcode;
            Operands = new List<Operand>();
        }

        public Instruction(int opcode, InstructionType instructionType)
        {
            Opcode = opcode;
            Type = instructionType;
            Operands = new List<Operand>();
        }

        public bool Read(int pc, byte[] buffer)
        {
            PC = pc;
            if (Opcode == 0xBE)
            {
                Type = InstructionType.Ext;
                // Error: Unsupported                
                Logger.Error("Opcode 0xBE unsupported");
            }
            else if (Opcode >= 0xC0)
            {
                // Form: Variable
                if ((Opcode & 0x20) == 0x20)
                {
                    Type = InstructionType.Var;
                    // VAR count                    
                    Logger.Debug($"{Opcode:X2} Variable : VAR count");
                }
                else
                {
                    Type = InstructionType.TwoOp;
                    // 2OP count
                    Logger.Debug($"{Opcode:X2} Variable : 2OP count");
                }

                ReadVariableOperands(buffer);
                Opcode = Opcode & 0x1f;

            }
            else if (Opcode >= 0x80)
            {
                // Form: Short  
                var operandType = (OperandType)((uint)(Opcode & 0x30) >> 4);
                if (operandType == OperandType.Omitted)
                {
                    Type = InstructionType.ZeroOp;
                    // 0OP count
                    Logger.Debug($"{Opcode:X2} Short : 0OP count");
                }
                else
                {
                    Type = InstructionType.OneOp;
                    // 1OP count
                    Logger.Debug($"{Opcode:X2} Short : 1OP count");
                    if (operandType == OperandType.LargeConst)
                    {
                        Operands.Add(new Operand(operandType, GetWord(buffer, PC)));
                        PC += 2;
                    }
                    else
                    {
                        Operands.Add(new Operand(operandType, GetByte(buffer, PC++)));
                    }
                }

                Opcode = Opcode & 0xf;
            }
            else
            {
                Type = InstructionType.TwoOp;
                // Form: Long
                // 2OP Count
                Logger.Debug($"{Opcode:X2} Long : 2OP count");
                var b = ReadTwoOperands(buffer);

                Opcode = Opcode & 0x1f;
            }

            return true;
        }

        private bool ReadTwoOperands(byte[] buffer)
        {
            int op1 = GetByte(buffer, PC++);



            int op2 = GetByte(buffer, PC++);

            Operands.Add((Opcode & 0x040) == 0 ? new Operand(OperandType.SmallConst, op1) : new Operand(OperandType.Variable, op1));
            Operands.Add((Opcode & 0x020) == 0 ? new Operand(OperandType.SmallConst, op2) : new Operand(OperandType.Variable, op2));

            return false;
        }

        private bool ReadVariableOperands(byte[] buffer)
        {
            int types = GetByte(buffer, PC++);
            var value = 0;

            for (var j = 6; j >= 0; j -= 2)
            {
                var t = (OperandType)(((uint)types >> j) & (uint)OperandType.Omitted);

                if (t == OperandType.Omitted) continue;
                switch (t)
                {
                    case OperandType.LargeConst:
                        value = GetWord(buffer, PC);
                        PC += 2;
                        break;
                    case OperandType.SmallConst:
                    case OperandType.Variable:
                        value = GetByte(buffer, PC++);
                        break;
                    case OperandType.Omitted:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Operands.Add(new Operand(t, value));
            }

            return false;
        }

        public bool Equals(Instruction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Opcode == other.Opcode && Type == other.Type;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Opcode*397) ^ (int) Type;
            }
        }
    }

}
