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
        public InputToken(int start, int dictionaryAddress, bool isSeparator, int index, string text)
        {
            Start = start;
            DictionaryAddress = dictionaryAddress;
            IsSeparator = isSeparator;
            Index = index;
            Text = text;
        }

        public string Text { get; set; }
        public int Index { get; set; }
        public bool IsSeparator { get; set; }
        public int DictionaryAddress { get; set; }
        public int Start { get; set; }
    }

    public class Input
    {
        public string Text { get; }
        public List<InputToken> Tokens { get; set; }

        public Input(string text)
        {
            Text = text;
            Tokens = new List<InputToken>();
        }
    }

    public class InputBuffer : MemoryWriter
    {
        private readonly int _start;
        private readonly int _limit;
        private string _text;
        public Input Input { get; set; }

        public InputBuffer(int start, int limit)
        {
            _start = start;
            _limit = limit;
        }

        public void Fill(string text, byte[] buffer)
        {
            _text = text;

            if (_text.Length > _limit)
                throw new ArgumentException();

            var n = _start + 1;
            foreach (var ch in _text.ToLower())
            {
                SetByte(buffer, n++, ch);
            }

            // FINALIZE NULL TERMINATED INFORM STRING ARRAY
            SetByte(buffer, n, 0);
        }

        public void Tokenize(List<char> delimiters)
        {
            Input = new Input(_text);

            int begin = 1;
            string token = "";
            int position = 1;

            for (int i = 0; i < _text.Length; i++)
            {
                char c = _text[i];

                if (delimiters.Contains(c) || c == ' ')
                {
                    if (!string.IsNullOrEmpty(token))
                        Input.Tokens.Add(new InputToken(begin, 0, false, position++, token));

                    // Ignore spaces, but include other delimiters in token list
                    if (c != ' ')
                    {
                        begin = i + 1;
                        token = c.ToString();
                        Input.Tokens.Add(new InputToken(begin, 0, true, position++, token));
                    }

                    token = "";
                    begin = i + 2;
                }
                else
                {
                    token += c;
                }
            }

            if (!string.IsNullOrEmpty(token))
                Input.Tokens.Add(new InputToken(begin, 0, true, position, token));
        }
    }

    public class ParseBufferBlock
    {
        /*
        4 byte blocks of 
		  byte 1 (byte address of the 1st word as found in dictionary, 0 if not found)
		  byte 2 
		  byte 3 (# of letters in word)
		  byte 4 (position of first letter of word in input buffer)
        */

        public byte B1 { get; set; }
        public byte B2{ get; set; }
        public byte B3 { get; set; }
        public byte B4 { get; set; }

        public ParseBufferBlock(int addr, byte count, byte position)
        {
            B1 = (byte) (addr >> 8);
            B2 = (byte) (addr & 0xff);
            B3 = count;
            B4 = position;
        }
    }

    public class ParseBuffer : MemoryWriter
    {
        private readonly int _start;
        private int _limit;
        private int _wcount;

        private List<ParseBufferBlock> _blocks;

        public ParseBuffer(int start, int limit)
        {
            _start = start;
            _limit = limit;
            _wcount = 0;
            _blocks = new List<ParseBufferBlock>();
        }

        public bool Fill(InputBuffer buffer, Dictionary dictionary)
        {
            _wcount = 0;
            _blocks = new List<ParseBufferBlock>();

            foreach (var t in buffer.Input.Tokens)
            {
                t.DictionaryAddress = dictionary.GetEntryAddress(t.Text);
                var item = new ParseBufferBlock(t.DictionaryAddress, (byte) t.Text.Length, (byte) t.Start);
                _blocks.Add(item);
            }

            _wcount = _blocks.Count;

            return true;
        }

        public void Write(byte[] buffer)
        {
            SetByte(buffer, _start + 1, _wcount);

            var bs = _start + 2;
            for (var i = 0; i < _wcount; i++)
            {
                SetByte(buffer, bs, _blocks[i].B1);
                SetByte(buffer, (bs + 1), _blocks[i].B2);
                SetByte(buffer, (bs + 2), _blocks[i].B3);
                SetByte(buffer, (bs + 3), _blocks[i].B4);
                bs += 4;
            }
        }
    }

}
