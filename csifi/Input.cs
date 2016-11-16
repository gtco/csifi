using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace csifi
{
    public class InputToken
    {
        public string Value { get; set; }
        public int Position { get; set; }
        public bool IsSeparator { get; set; }
        public int Index { get; set; }
    }

    public class Input
    {
        private readonly string _input;
        public List<InputToken> Tokens { get; set; }

        public Input(string input)
        {
            _input = input;
            Tokens = new List<InputToken>();
        }

        public void Parse(List<char> delimiters)
        {
            var tokens = _input.Split(' ');
            var position = 0;

            foreach (var t in tokens)
            {
                bool hasDelimiters = false;
                foreach (var d in delimiters)
                {
                    if (t.Contains(d))
                    {
                        Tokens.Add(new InputToken() { IsSeparator = false, Position = position++, Value = t.Substring(0, t.IndexOf(d))});
                        Tokens.Add(new InputToken() { IsSeparator = true, Position = position++, Value = d.ToString() });
                        Tokens.Add(new InputToken() { IsSeparator = false, Position = position++, Value = t.Substring(t.IndexOf(d) + 1) });
                        hasDelimiters = true;
                    }
                }

                if (!hasDelimiters)
                {
                    Tokens.Add(new InputToken() { IsSeparator = false, Position = position++, Value = t });
                }
            }
        }

        public void Lookup(Dictionary dictionary)
        {
            foreach (var t in Tokens)
            {
                t.Index = dictionary.Lookup(t.Value);
            }
        }
    }
}
