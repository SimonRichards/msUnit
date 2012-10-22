using System;
using System.ServiceModel;

namespace msUnit {

	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	class ConsoleWriter : ITestOutput {

		private readonly bool _canMoveCursor;
		private readonly DateTime _start;
		private int _count;
		private int _passedCount;
		private const string _testingFormat = "Testing\t{0}...";

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
			PrintWithColour(ConsoleColor.DarkRed, thrown.Message);
		}

		public void TestStarted(string name) {
			if (_canMoveCursor) {
				PrintWithColour(ConsoleColor.Gray,_testingFormat, name);
			}
		}

		public void TestRunCompleted() {
			PrintWithColour(_passedCount == _count ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed,
			                "{0}/{1} tests passed in {2}", _passedCount, _count, DateTime.Now - _start);
		}

		public void TestCompleted(TestDetails details) {
			if (_canMoveCursor) {
				--Console.CursorTop;
				Console.WriteLine(new string(' ', string.Format(_testingFormat, details.Name).Length));
				--Console.CursorTop;
			}
			++_count;
			if (details.Passed) {
				PrintWithColour(ConsoleColor.DarkGreen, "Passed\t" + details.Name);
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
				Console.ResetColor();
			}
		}

		public void TestSuiteStarted(string name) {
			PrintWithColour(ConsoleColor.Yellow, "Testing assembly: {0}", name);
		}

		public void TestIgnored(string name, string reason) {
			PrintWithColour(ConsoleColor.Gray, "Ignoring test: {0}, {1}", name, reason);
		}

		public void TestSuiteFinished() {
		}
		
		private void PrintWithColour(ConsoleColor colour, string format, params object[] args) {
			Console.ForegroundColor = colour;
			Console.WriteLine(format, args);
			Console.ResetColor();
		}


		public void Ping() {}
	}
}
