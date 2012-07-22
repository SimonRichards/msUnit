using System;

namespace msUnit {
    class Program {
        static void Main(string[] args) {
            var options = new Options(args);
            if (options.Error) {
                Console.Error.WriteLine(options.Usage);
                return;
            }

            var output = new ConsoleWriter();

            foreach (var file in options.Assemblies) {
                try {
                    var assembly = new TestAssembly(file, options.Filters);
                    assembly.AssemblyErrorHandler += output.AssemblyError;
                    assembly.TestCompleteHandler += output.TestCompleted;
                    assembly.Test();
                } catch (Exception e) {
                    output.AssemblyError(new TestException(file + " invalid: " + e.Message));
                }
            }
        }
    }
}
