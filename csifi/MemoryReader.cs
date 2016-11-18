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

    public class MemoryWriter : MemoryReader
    {
        public void SetWord(byte[] buffer, int index, int value)
        {
            byte msb = (byte)(value >> 8);
            byte lsb = (byte)(value & 0xff);
            buffer[index] = msb;
            buffer[index + 1] = lsb;
        }

        public void SetByte(byte[] buffer, int index, int value)
        {
            buffer[index] = (byte)value;
        }
    }
}
