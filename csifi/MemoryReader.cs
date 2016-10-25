using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csifi
{
    public class MemoryReader
    {
        public byte GetByte(byte[] buffer, int index)
        {
            return buffer[index];
        }
        public int GetWord(byte[] buffer, int index)
        {
            return ((buffer[index] & 0xff) << 8) + (buffer[index + 1] & 0xff);
        }
    }
}
