using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace csifi
{
    public class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Wrong number of arguments");
                Console.ReadKey();
                return;
            }

            var i = new Interpreter();

            if (i.LoadFile(args[0]))
            {
                Logger.Debug($"Successfully loaded {i.Buffer.Length} bytes from file {args[0]} ");
            }

            if (i.Init())
            {
                i.Run();
            }

            Console.ReadKey();
        }
    }
}
