using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace msUnit {
    class ConsoleWriter {

        public void AssemblyError(string details) {
            Console.WriteLine(details);
        }

        public void TestCompleted(TestDetails details) {
            if (details.Passed) {
                Console.WriteLine("Passed\t" + details.Name);
            } else {
                Console.WriteLine("Failed\t" + details.Name);
                Console.WriteLine(details.Thrown.Message);
            }
        }
    }
}
