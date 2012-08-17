using System;

namespace msUnit {
	class ConsoleWriter {

		private readonly bool _canMoveCursor;
		private readonly DateTime _start;
		private int _count;
		private int _passedCount;
		private static ConsoleColor _success = ConsoleColor.DarkGreen;
		private static ConsoleColor _failure = ConsoleColor.DarkRed;
		private static string _testingString = "Testing\t{0}...";

		public ConsoleWriter() {
			if (Console.BufferWidth == 0) {
				_canMoveCursor = false;
			} else {
				try {
					Console.WriteLine();
					Console.CursorTop--;
					_canMoveCursor = true;
				} catch (Exception) {
					_canMoveCursor = false;
				}
			}
			_start = DateTime.Now;
		}

		public void AssemblyError(Exception thrown) {
			Console.WriteLine(thrown.Message);
		}

		public void TestStarted(string name) {
			if (_canMoveCursor) {
				using (new TemporaryConsoleColor(ConsoleColor.Gray)) {
					Console.WriteLine(_testingString, name);
				}
			}
		}

		public void TestRunCompleted() {
			using (new TemporaryConsoleColor(_passedCount == _count ? _success : _failure)) {
				Console.WriteLine("{0}/{1} tests passed in {2}", _passedCount, _count, DateTime.Now - _start);
			}
		}

		public void TestCompleted(TestDetails details) {
			if (_canMoveCursor) {
				--Console.CursorTop;
				Console.WriteLine(new string(' ', string.Format(_testingString, details.Name).Length));
				--Console.CursorTop;
			}
			++_count;
			using (new TemporaryConsoleColor(details.Passed ? _success : _failure)) {
				if (details.Passed) {
					Console.ForegroundColor = ConsoleColor.DarkGreen;
					Console.WriteLine("Passed\t" + details.Name);
					++_passedCount;
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
			}
		}

		private class TemporaryConsoleColor : IDisposable {
			private readonly ConsoleColor _originalColor;

			public TemporaryConsoleColor(ConsoleColor newColour) {
				_originalColor = Console.ForegroundColor;
				Console.ForegroundColor = newColour;
			}

			public void Dispose() {
				Console.ForegroundColor = _originalColor;
			}
		}
	}
}
