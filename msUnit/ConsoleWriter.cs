using System;

namespace msUnit {
	class ConsoleWriter {

		private bool _canMoveCursor;

		public ConsoleWriter() {
			try {
				Console.CursorTop++;
				Console.CursorTop--;
				_canMoveCursor = true;
			} catch {
				_canMoveCursor = false;
			}
		}

		public void AssemblyError(Exception thrown) {
			Console.WriteLine(thrown.Message);
		}

		public void TestStarted(string name) {
			if (_canMoveCursor) {
				Console.WriteLine("Testing\t{0}...", name);
				Console.CursorTop--;
			}
		}

		public void TestRunCompleted() {

		}

		public void TestCompleted(TestDetails details) {
			var originalColour = Console.ForegroundColor;
			if (details.Passed) {
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("Passed\t" + details.Name);
			} else {
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine("Failed\t" + details.Name);
				Console.WriteLine(details.Thrown);
				if (!string.IsNullOrWhiteSpace(details.StdOut)) { 
					Console.WriteLine("stdout:");
					Console.WriteLine(details.StdOut);
				}
				if (!string.IsNullOrWhiteSpace(details.StdErr)) { 
					Console.WriteLine("stderr:");
					Console.WriteLine(details.StdErr);
				}
			}
			Console.ForegroundColor = originalColour;
		}
	}
}
