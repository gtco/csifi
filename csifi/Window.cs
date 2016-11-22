using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csifi
{
    public class Window
    {
        public void Print(params string[] strings)
        {
            foreach (var s in strings)
            {
                Console.Write(s);
            }
        }

        public void NewLine()
        {
            Console.WriteLine();
        }
    }
}
