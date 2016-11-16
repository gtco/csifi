using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csifi
{
    public class InstructionSet
    {
        public Dictionary<Instruction, Func<Instruction, Frame, Globals, ObjectTable, Dictionary, int>> Functions = new Dictionary<Instruction, Func<Instruction, Frame, Globals, ObjectTable, Dictionary, int>>();

        public InstructionSet()
        {
            Functions.Add(new Instruction(0, InstructionType.Var), CallFv);
        }

        private int CallFv(Instruction instruction, Frame frame, Globals globals, ObjectTable objectTable, Dictionary dictionary)
        {
            int pa = instruction.Operands[0].Value;

            if (pa == 0)
            {
                
            }

//            int rpc = frame.GetPackedAddress()

        }

    }
}
