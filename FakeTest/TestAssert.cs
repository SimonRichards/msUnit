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

        [TestMethod]
        public void TestFailure() {
            Assert.AreEqual(1, 2, "one does not equal two");
        }

        [TestMethod, TestCategory("Don't run")]
        public void TestSkip() {
            throw new Exception("This test should be skipped");
        }
    }
}
