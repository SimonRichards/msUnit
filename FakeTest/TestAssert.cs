using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FakeTest {
    [TestClass]
    public class TestAssert {
        [TestMethod]
        public void TestEquals() {
            Assert.AreEqual(2 * 5, 5 + 5, "wat?");
        }
    }
}
