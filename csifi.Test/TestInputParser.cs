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
            Input i = new Input("fred,go fishing");

            i.Parse(new List<char>() {',', '.', '"'});

            Assert.IsTrue(i.Words.Count == 4);
        }
    }
}
