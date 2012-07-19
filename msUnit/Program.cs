using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace msUnit {
    class Program {
        static void Main(string[] args) {
            if (args.Length != 1) {
                Console.WriteLine("Please supply an assembly to run");
                return;
            }

            if (!File.Exists(args[0])) {
                Console.WriteLine("{0} not found.", args[0]);
            }

            try {
                var assembly = new TestAssembly(args[0]);
                var output = new ConsoleWriter();
                assembly.AssemblyErrorHandler += output.AssemblyError;
                assembly.TestCompleteHandler += output.TestCompleted;
                assembly.Test();
            } catch(Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }
}
