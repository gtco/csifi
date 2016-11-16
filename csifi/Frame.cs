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
        public int PC { get; set; }
        public Stack<int> Stack { get; set; }
        public List<Local> Locals { get; set; }
        public InvocationMethod InvocationMethod { get; set; }
        public int ArgumentCount { get; set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly InstructionSet Set = new InstructionSet();

        public Frame(int pc)
        {
            PC = pc;
            Stack = new Stack<int>();
            Locals = new List<Local>();
            InvocationMethod = InvocationMethod.Function;
            ArgumentCount = 0;
        }

        internal bool Execute(byte[] buffer, Globals globals, ObjectTable objectTable, Dictionary dictionary)
        {
            Logger.Debug($"{PC:X4}");
            var i = new Instruction(GetByte(buffer, PC++));

            if (!i.Read(PC, buffer))
            {
                Logger.Error($"Unknown Opcode {i:X2}");
                return false;
            }

            i.Invoke(this, globals, objectTable, dictionary);

            return true;
        }
    }

}
