using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace msUnit {
	class TestException : Exception {
		public TestException(string message) : base(message) {
		}
	}
}
