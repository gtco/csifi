using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace csifi.Test
{
    [TestClass]
    public class TextTest
    {
        [TestMethod]
        public void TestCharacterDecode()
        {
            byte a = 8;
            var character = new Character(a);
            var c = character.DecodeCharacter(Character.Alphabet0);
            Assert.AreEqual(c, 'c');
            c = character.DecodeCharacter( Character.Alphabet1);
            Assert.AreEqual(c, 'C');
            a += 6;
            character = new Character(a);
            c = character.DecodeCharacter(Character.Alphabet2);
            Assert.AreEqual(c, '0');
        }

        [TestMethod]
        public void TestCreateFromWord()
        {
            byte first = 1;
            byte second = 2;
            byte third = 3;

            var word = third + (second << 5) + (first << 10);

            var text = new Text(word);

            Assert.AreEqual(text.Characters.Count, 3);
            Assert.AreEqual(text.Characters[0], new Character(first));
            Assert.AreEqual(text.Characters[1], new Character(second));
            Assert.AreEqual(text.Characters[2], new Character(third));
        }

    }
}
