using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace csifi.Test
{
    [TestClass]
    public class TestInputParser
    {
        [TestMethod]
        public void TestSimpleInput()
        {
            byte[] buffer = new byte[256];
            var delim = new List<char>() {',', '.', '"'};
            InputBuffer inputBuffer = new InputBuffer(0,100);
            inputBuffer.Fill("fred,go fishing", buffer);
            inputBuffer.Tokenize(delim);
            Assert.IsTrue(inputBuffer.Input.Tokens.Count == 4);
       }

        [TestMethod]
        public void TestSimpleInput2()
        {
            byte[] buffer = new byte[256];
            var delim = new List<char>() { ',', '.', '"' };
            InputBuffer inputBuffer = new InputBuffer(0, 100);
            inputBuffer.Fill("open mailbox", buffer);
            inputBuffer.Tokenize(delim);
            Assert.IsTrue(inputBuffer.Input.Tokens.Count == 2);
        }
    }
}
