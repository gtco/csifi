using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csifi
{
    public class InputToken
    {
        public string String { get; set; }
        public int Position { get; set; }
        public bool IsSeparator { get; set; }
        public Text Text { get; set; }
    }

    public class ParseTable
    {
        public int Count { get; set; }
        public List<InputToken> InputTokens { get; set; }
    }

    public class Input
    {
        private readonly string _input;
        public List<string> Words { get; set; }

        public Input(string input)
        {
            _input = input;
            Words = new List<string>();
        }

        public void Parse(List<char> delimiters)
        {
            var tokens = _input.Split(' ');

            foreach (var t in tokens)
            {
                bool b = false;
                foreach (var s in delimiters)
                {
                    if (t.Contains(s))
                    {
                        Words.Add(t.Substring(0, t.IndexOf(s)));
                        Words.Add(s.ToString());
                        Words.Add(t.Substring(t.IndexOf(s) + 1));
                        b = true;
                    }
                }

                if (!b)
                {
                    Words.Add(t);
                }
            }
        }


    }
}
