using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace msUnit {
    class ConsoleWriter {

        public void AssemblyError(Exception thrown) {
            Console.WriteLine(thrown.Message);
        }

        public void TestCompleted(TestDetails details) {
            var originalColour = Console.ForegroundColor;
            if (details.Passed) {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Passed\t" + details.Name);
            } else {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Failed\t" + details.Name);
                Console.WriteLine(details.Thrown.Message);
            }
            Console.ForegroundColor = originalColour;
        }
    }
}
