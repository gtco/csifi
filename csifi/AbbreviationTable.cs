﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace csifi
{
    public class Globals : MemoryReader
    {
        public const int Count = 240;

        private readonly int _start;
        private List<int> _variables;

        public Globals(int start)
        {
            _start = start;
            _variables = new List<int>();
        }

        public bool Init(byte[] buffer)
        {
            var offset = 0;
            for (var i = 0; i < Count; i++)
            {
                _variables.Add(GetWord(buffer, _start + offset));
                offset += 2;
            }

            return true;
        }

        public int Get(int index)
        {
            return _variables[index];
        }

        public void Set(int index, int value)
        {
            _variables[index] = value;
        }

    }


    public class AbbreviationTable : MemoryReader
    {
        // V3+
        private const int WordCount = 96;
        private readonly List<Text> _list;
        private int _start;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AbbreviationTable()
        {
            _start = 0;
            _list = new List<Text>(WordCount);
        }

        public bool Init(byte[] buffer, int start)
        {
            _start = start;

            for (var i = 0; i < WordCount; i++)
            {
                _list.Add(LoadAbbreviation(buffer, i));
            }

            return true;
        }

        public string GetAbbreviation(int index)
        {
            if (index < 0)
                return "";

            return index >= _list.Count ? "" : _list[index].GetValue();
        }

        public Text LoadAbbreviation(byte[] buffer, int index)
        {
            var text = new Text();
            var offset = _start + (index*2);
            var packed = GetWord(buffer, offset);
            var address = packed * 2;
            bool end;

            do
            {
                var w = GetWord(buffer, address);
                text.AddCharacters(w);
                end = (w & Text.EndMarker) == Text.EndMarker;
                address += 2;

            } while (!end);

            return text;
        }
    }
}
