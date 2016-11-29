using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace csifi
{

    public class ObjectProperty
    {
        public int Index { get; set; }
        public int Address { get; set; }
        public List<int> List { get; set; }
    }

    public class GameObject : MemoryReader
    {
        private byte[] _attributes;
        public int Parent { get; set; }
        public int Sibling { get; set; }
        public int Child { get; set; }
        public int Index { get; }
        public int Header { get; }
        public string Name { get; set; }
        //        public Dictionary<int, List<int>> Properties { get; set; }
         public Dictionary<int, ObjectProperty> ObjectProperties { get; set; }

        public char[] Attributes { get; set; }

        public GameObject(int index, byte[] attributes, int parent, int sibling, int child, int header)
        {
            _attributes = attributes;
            Parent = parent;
            Sibling = sibling;
            Child = child;
            Index = index;
            Header = header;
            Name = "";
            Attributes = new char[32];
            ObjectProperties = new Dictionary<int, ObjectProperty>();
        }

        public static GameObject Empty()
        {
            return new GameObject(0, new []{(byte)0x0}, 0,0,0,0);
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

            // load object properties
            while (x != 0)
            {
                List<int> list = new List<int>();
                int id = x & 0x1f;
                int numberOfBytes = (x >> 5) + 1;

                ObjectProperties.Add(id, new ObjectProperty()
                {
                    //TODO Verify Address
                    Address = index + 1,
                    Index = id
                });

                for (int i = 0; i < numberOfBytes; i++)
                {
                    list.Add(GetByte(buffer, ++index));
                }

                ObjectProperties[id].List = list;

                x = GetByte(buffer, ++index);
            }

            LoadAttributes();

            return true;
        }

        private void LoadAttributes()
        {
            var s = "";

            for (var n = 0; n < 4; n++)
            {
                s += Convert.ToString(_attributes[n], 2).PadLeft(8, '0');
            }

            Attributes = s.ToCharArray();
        }

        public bool TestAttribute(int index)
        {
            if (Attributes != null && Attributes.Length > index)
            {
                return Attributes[index] == '1';
            }

            return false;
        }

        public void SetAttribute(int index)
        {
            if (Attributes != null && Attributes.Length > index)
            {
                Attributes[index] = '1';
            }
        }

        public void ClearAttribute(int index)
        {
            if (Attributes != null && Attributes.Length > index)
            {
                Attributes[index] = '0';
            }
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ObjectTable(int start)
        {
            _start = start;
            _defaultProperties = new int[32];
            _objects = new List<GameObject>();
            _objects.Add(GameObject.Empty());
            _defaultProperties[0] = int.MaxValue;
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
            for (int i = 0; i < 31; i++)
            {
                _defaultProperties[i+1] = GetWord(buffer, _start + (i*2));
            }

            bool done = false;
            int j = 0;
            int n = 1;

            // loop through all objects
            while (!done && j < MaxObjects)
            { 
                int attr = _start + (PropertyDefaultCount*2) + (j * ObjectLength);

                if (_objects.Count > 1 && (attr == _objects[1].Header))
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

                    var obj = new GameObject(n++, attributes, parent, sibling, child, header);

                    if (obj.Init(buffer, abbreviationTable))
                    {
                        _objects.Add(obj);
                    }

                    j++;
                }
            }

            return true;
        }

        public bool IsParent(int p, int c)
        {
            var child = _objects[c];
            if (p == 0 && child.Parent == 0)
            {
                return true;
            }

            return (child.Parent == p);
        }

        public int GetObjectProperty(int obj, int prop)
        {
            var n = 0;
            var o = _objects[obj];

            if (o.ObjectProperties != null && o.ObjectProperties.ContainsKey(prop))
            {
                var list = o.ObjectProperties[prop].List;
                if (list.Count > 1)
                {
                    //TODO check length of list
                    n = (short)((list[0] << 8) & 0xff00) | (list[1] & 0xff);
                }
                else if (list.Count > 0)
                {
                    n = list[0];
                }
            }
            else
            {
                n = _defaultProperties[prop];
            }

            return n;
        }

        public int GetObjectPropertyAddress(int obj, int prop)
        {
            var o = _objects[obj];
            if (o.ObjectProperties == null || !o.ObjectProperties.ContainsKey(prop)) throw new ArgumentException();
            var addr = o.ObjectProperties[prop].Address;
            return addr;
        }

    }
}
