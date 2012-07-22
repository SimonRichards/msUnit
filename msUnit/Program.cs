using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace msUnit {
    class Program {
        static void Main(string[] args) {
            var options = new Options(args);
            if (options.Error) {
                Console.Error.WriteLine(options.Usage);
                return;
            }

            foreach (var file in options.Assemblies) {
                if (!File.Exists(file)) {
                    Console.Error.WriteLine("{0} not found.", args[0]);
                    continue;
                }
                try {
                    var assembly = new TestAssembly(file, options.Filters);
                    var output = new ConsoleWriter();
                    assembly.AssemblyErrorHandler += output.AssemblyError;
                    assembly.TestCompleteHandler += output.TestCompleted;
                    assembly.Test();
                } catch(Exception e) {
                    Console.Error.WriteLine(e);
                }
            }
        }
    }
}
