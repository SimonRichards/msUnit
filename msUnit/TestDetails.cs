using System;
using System.Text;

namespace msUnit {
	public enum Result {
		Pass,
		Fail
	}

	public struct TestDetails {
		public bool Passed;
		public string Name;
		public string Thrown;
		public TimeSpan Time;
		public string StdOut;
		public string StdErr;

		public static TestDetails CreateFailure(string name) {
			return new TestDetails {
				Name = name,
				Passed = false,
				Thrown = "An unknown error caused the test runner to exit",
				Time = TimeSpan.MinValue,
				StdOut = string.Empty,
				StdErr = string.Empty
			};
		}
	}
}
