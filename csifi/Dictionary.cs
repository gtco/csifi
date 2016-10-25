using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csifi
{
    public class Entry
    {
        public int Start { get; set; }        
        public string Value { get; set; }
        public Text Text { get; set; }

        public Entry(int start, string value, Text text)
        {
            Start = start;
            Value = value;
            Text = text;
        }

    }

    public class Dictionary : MemoryReader
    {
        private readonly int _start;
        public List<char> Separators { get; }
        public List<Entry> Entries { get; set; }

        public Dictionary(int start)
        {
            _start = start;
            Separators = new List<char>();
            Entries = new List<Entry>();
        }

        public bool Init(byte[] buffer)
        {
            int n = GetByte(buffer, _start);
            for (var i = 1; i <= n; i++)
            {
                Separators.Add((char) GetByte(buffer, _start + i));
            }

            // length
            var dl = GetByte(buffer, _start + n + 1);
            // count
            var dc = GetWord(buffer, _start + n + 2);
            // entry start
            var ds = _start + n + 4;

            Entries = new List<Entry>();

            // loop through all entries
            for (var j = 0; j < dc; j++)
            {
                var e = (j*dl) + ds;
                var text = new Text(GetWord(buffer, e));
                text.AddCharacters(GetWord(buffer, e + 2));
                Entries.Add(new Entry(e, text.GetValue(), text));
            }

            return true;
        }
    }
}
