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

            new TestAssembly(args[0]).Test();
        }
    }
}
