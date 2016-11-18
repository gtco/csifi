using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace csifi
{
    public class GameObject : MemoryReader
    {
        private byte[] _attributes;
        public int Parent { get; set; }
        public int Sibling { get; set; }
        public int Child { get; set; }
        public int Index { get; }
        public int Header { get; }
        public string Name { get; set; }
        public Dictionary<int, List<int>> Properties { get; set; }

        public GameObject(int index, byte[] attributes, int parent, int sibling, int child, int header)
        {
            _attributes = attributes;
            Parent = parent;
            Sibling = sibling;
            Child = child;
            Index = index;
            Header = header;
            Name = "";
        }

        public bool Init(byte[] buffer, AbbreviationTable abbreviationTable)
        {
            int len = GetByte(buffer, Header);
            int index = Header + 1;

            // load object name
            if (len > 0)
            {
                Text t = new Text();

                for (int i = 0; i < len; i++)
                {
                    t.AddCharacters(GetWord(buffer, index));
                    index += 2;
                }

                Name = t.GetValue(abbreviationTable);
            }

            int x = GetByte(buffer, index);
            Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();

            // load object properties
            while (x != 0)
            {
                List<int> list = new List<int>();
                int id = x & 0x1f;
                int numberOfBytes = (x >> 5) + 1;

                for (int i = 0; i < numberOfBytes; i++)
                {
                    list.Add(GetByte(buffer, ++index));
                }

                dictionary.Add(id, list);
                x = GetByte(buffer, ++index);
            }

            Properties = dictionary;
            return true;
        }

        public bool TestAttribute(int index)
        {
            if (_attributes != null && _attributes.Length > index)
            {
                return (_attributes[index] == 1);
            }

            return false;
        }

        public void AddChild(GameObject obj)
        {
            if (Child == 0)
            {
                Child = obj.Index;
                obj.Parent = Index;
                obj.Sibling = 0;
            }
            else
            {
                var s = Child;
                Child = obj.Index;
                obj.Parent = Index;
                obj.Sibling = s;
            }
        }
    }

    public class ObjectTable : MemoryReader
    {
        private const int MaxObjects = 255;
        private const int PropertyDefaultCount = 31;
        private const int ObjectLength = 9;

        private readonly int _start;

        private int[] _defaultProperties;

        private List<GameObject> _objects;
        
        public ObjectTable(int start)
        {
            _start = start;
            _defaultProperties = new int[PropertyDefaultCount];
            _objects = new List<GameObject>();
        }

        public GameObject GetObject(int index)
        {
            return _objects[index];
        }

        public int[] GetDefaultProperties()
        {
            return _defaultProperties;
        }

        public bool Init(byte[] buffer, AbbreviationTable abbreviationTable)
        {
            // read default attributes
            for (int i = 0; i < PropertyDefaultCount; i++)
            {
                _defaultProperties[i] = GetWord(buffer, _start + (i*2));
            }

            bool done = false;
            int j = 0;

            // loop through all objects
            while (!done && j < MaxObjects)
            { 
                int attr = _start + (PropertyDefaultCount*2) + (j * ObjectLength);

                if (_objects.Any() && (attr == _objects[0].Header))
                {
                    done = true;
                }
                else
                {
                    byte[] attributes = new byte[4];
                    for (int k = 0; k < 4; k++)
                    {
                        attributes[k] = GetByte(buffer, attr + k);
                    }

                    int parent = GetByte(buffer, attr + 4);
                    int sibling = GetByte(buffer, attr + 5);
                    int child = GetByte(buffer, attr + 6);
                    int header = GetWord(buffer, attr + 7);

                    var obj = new GameObject(j, attributes, parent, sibling, child, header);

                    if (obj.Init(buffer, abbreviationTable))
                    {
                        _objects.Add(obj);
                    }

                    j++;
                }
            }

            return true;
        }

    }
}
