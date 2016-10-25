using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.LayoutRenderers.Wrappers;

namespace csifi
{

    public class Character : IEquatable<Character>
    {
        public const int Alphabet0 = 0;
        public const int Alphabet1 = 1;
        public const int Alphabet2 = 2;

        public static string LowerCase = " ^^^^^abcdefghijklmnopqrstuvwxyz";
        public static string UpperCase = " ^^^^^ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string Punctuation = " ^^^^^^^0123456789.,!?_#’\"/\\-:()";
        public static string[] CharacterMap = { LowerCase, UpperCase, Punctuation };

        public byte Value { get; }

        public Character(byte value)
        {
            Value = value;
        }

        public char DecodeCharacter(int alphabet)
        {
            return CharacterMap[alphabet][Value];
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Character);
        }
        public bool Equals(Character other)
        {
            return other != null && Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public class Text 
    {
        public const int FirstChar = 0x7C00;
        public const int SecondChar = 0x03E0;
        public const int ThirdChar = 0x001F;
        public const int EndMarker = 0x8000;

        public List<Character> Characters { get; set; }

        public Text()
        {
            Characters = new List<Character>();
        }

        public Text(int word)
        {
            Characters = new List<Character>
            {
                new Character((byte) ((word & FirstChar) >> 10)),
                new Character((byte)((word & SecondChar) >> 5)),
                new Character((byte)(word & ThirdChar))
            };
        }

        public void AddCharacters(int word)
        {
            Characters.AddRange(new List<Character>
            {
                new Character((byte) ((word & FirstChar) >> 10)),
                new Character((byte) ((word & SecondChar) >> 5)),
                new Character((byte) (word & ThirdChar))
            });
        }

        public string GetValue(AbbreviationTable abbreviationTable = null)
        {
            var alphabet = Character.Alphabet0;
            var tableOffset = 0;
            int start = 0;

            string str = "";

            if (Characters[0].Value == 5 && Characters[1].Value == 6 && Characters.Count >=5)
            {
                var x = ((Characters[2].Value & 0x1f) << 5) + (Characters[3].Value & 0x1f);
                str += ((char) x);
                start = 4;
            }

            for (int i = start; i < Characters.Count; i++)
            {
                if (abbreviationTable != null && alphabet == Character.Alphabet2)
                {
                    var n = tableOffset + Characters[i].Value;
                    str += abbreviationTable.GetAbbreviation(n);
                    tableOffset = 0;
                    alphabet = Character.Alphabet0;
                }
                else 
                {
                    switch (Characters[i].Value)
                    {
                        case 0:
                            str += " ";
                            break;
                        case 1:
                            tableOffset = 0;
                            alphabet = Character.Alphabet2;
                            break;
                        case 2:
                            tableOffset = 32;
                            alphabet = Character.Alphabet2;
                            break;
                        case 3:
                            tableOffset = 64;
                            alphabet = Character.Alphabet2;
                            break;
                        case 4:
                            alphabet = Character.Alphabet1;
                            break;
                        case 5:
                            alphabet = Character.Alphabet2;
                            break;
                        default:
                            str += Characters[i].DecodeCharacter(alphabet);
                            alphabet = Character.Alphabet0;
                            break;
                    }
                }
            }

            return str;
        }
    }
}
