using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;

namespace tests
{
	[TestClass]
	public class MyClass {
		[TestMethod]
		public void Test1() {
			Assert.IsTrue(true);
		}

		[TestMethod]
		public void Test2() {
			Marshal.ReadByte(IntPtr.Zero);
		}
		
		[TestMethod]
		public void Test3() {
			Assert.IsTrue(false);
		}
	}
}

