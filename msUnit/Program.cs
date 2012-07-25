using System;
using System.Linq;

namespace msUnit {
	class Program {
		static void Main(string[] args) {
			var options = new Options(args);
			if (options.Error) {
				Console.Error.WriteLine(options.Usage);
				return;
			}

			var output = new ConsoleWriter();

			foreach (var assembly in options.Assemblies.Select(file => new TestAssembly(file, options.Filters))) {
				assembly.AssemblyErrorHandler += output.AssemblyError;
				assembly.TestCompleteHandler += output.TestCompleted;
				assembly.TestStartedHandler += output.TestStarted;
				assembly.Test();
			}
			output.TestRunCompleted();
		}
	}
}
