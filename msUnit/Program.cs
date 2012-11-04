using System;
using System.Linq;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Reflection;

namespace msUnit {
	class Program {
		static void Main(string[] args) {
			var options = new Options(args);
			
			if (options.HasError) {
				Console.Error.WriteLine(options.Usage);
			} else if (options.IsChild) {
				new TestRunner(options).Complete.WaitOne();
			} else {
				Process runner = CreateChild(args, options.PipeName);
				new Thread(() => KeepAlive(runner)).Start();
				new TestServer(options, runner).Start();
			}
		}

		static Process CreateChild(string[] args, string pipeName) {
			return new Process {
				StartInfo = new ProcessStartInfo {
					FileName = Assembly.GetEntryAssembly().Location,
					Arguments = string.Join(" ", args) + Options.CreateParentOption(pipeName),
					UseShellExecute = false
				}
			};
		}

		static void KeepAlive(Process process) {
			while (true) {
				process.Start();
				process.WaitForExit();
				if (process.ExitCode == 0) {
					return;
				}
			}
		}
	}
}
