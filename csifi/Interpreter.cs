using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace csifi
{
    public class Interpreter : MemoryReader
    {
        public byte[] Buffer { get; set; }
        public Stack<Frame> Stack { get; set; }
        public bool Running { get; set; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private AbbreviationTable _abbreviationTable;
        private Dictionary _dictionary;
        private ObjectTable _objectTable;
        private Globals _globals;

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
            Stack = new Stack<Frame>();
            var globalVariableOffset = GetWord(Buffer, Header.GlobalVar);
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
            Stack.Push(new Frame(GetWord(Buffer, Header.InitialPc)));

            // TODO create window

            return true;
        }

        public void Run()
        {
            do
            {
                var f = Stack.Pop();
                Running = f.Execute(Buffer, _globals, _objectTable, _dictionary);

            } while (Running);
        }
     
    }
}
