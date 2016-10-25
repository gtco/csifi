using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
    }

    public class Frame
    {
        public int PC { get; set; }
        public Stack<int> Stack { get; set; }
        public List<Local> Locals { get; set; }
        public InvocationMethod InvocationMethod { get; set; }
        public int ArgumentCount { get; set; }

        public Frame(int pc)
        {
            PC = pc;
            Stack = new Stack<int>();
            Locals = new List<Local>();
            InvocationMethod = InvocationMethod.Function;
            ArgumentCount = 0;
        }

        public bool Execute()
        {
            return false;
        }
    }

}
