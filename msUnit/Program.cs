using System;
using System.Linq;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Reflection;

namespace msUnit {
	class Program {
		static int Main(string[] args) {
			var options = new Options(args);
			if (options.Error) {
				Console.Error.WriteLine(options.Usage);
			} 
#if !DEBUG
			else if (options.RunInSeparateProcess) {
				Fork(args);
			} 
#endif
			else {
				RunAllTests(options);
			}
			return 0;
		}

		static void Fork(string[] args) {
			string pipeName = Guid.NewGuid().ToString();
			string executable = Assembly.GetEntryAssembly().Location;
			string arguments = string.Join(" ", args) + Options.CreateParentOption(pipeName);
			Process process;
			bool firstRun = true;
			string lastRunTest = string.Empty;
			var pipe = new NamedPipeServerStream(pipeName);
			var pipeReader = new Thread(() => {
				pipe.WaitForConnection();
				using (var stream = new StreamReader(pipe)) {
					while (!stream.EndOfStream) {
						lastRunTest = stream.ReadLine();
					}
				}
			});
			pipeReader.Start();
			while (true) {
				process = new Process {
					StartInfo = new ProcessStartInfo {
						FileName = executable,
						Arguments = arguments + (firstRun ? string.Empty : Options.CreateLastTestOption(lastRunTest)),
						UseShellExecute = false
					}
				};
				process.Start();
				process.WaitForExit();
				if (process.ExitCode != 0 && lastRunTest != string.Empty) {
					firstRun = false;
					Console.Error.WriteLine("Test runner crashed running {0}, recovering...", lastRunTest);
				} else {
					break;
				}
			}
			pipeReader.Abort();
		}

		static void RunAllTests(Options options) {

			AppDomain.CurrentDomain.UnhandledException += (sender, args) => Environment.Exit(1);

			var output = options.Output;
			if (options.PreviousRunCrashed) {
				output.TestCompleted(TestDetails.CreateFailure(options.CrashedTest));
			}
					
#if !DEBUG
			using (var pipe = new NamedPipeClientStream(options.Parent)) {
				pipe.Connect();
				var parentStream = new StreamWriter(pipe) { AutoFlush = true };
#endif
				foreach (var file in options.Assemblies) {
					try {
						var assembly = new TestAssembly(file, options.Filters);
						output.TestSuiteStarted(file);
						assembly.AssemblyErrorHandler += output.AssemblyError;
						assembly.TestCompleteHandler += output.TestCompleted;
						assembly.TestStartedHandler += output.TestStarted;
#if !DEBUG
						assembly.TestStartedHandler += test => parentStream.WriteLine(test);
#endif
						assembly.Test();
						output.TestSuiteFinished();
					} catch (Exception e) {
						output.AssemblyError(e);
					}
				}
				output.TestRunCompleted();
#if !DEBUG
			}
#endif
		}
	}
}