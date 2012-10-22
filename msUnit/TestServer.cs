using System;
using System.Linq;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace msUnit {
	class TestServer {
		private readonly Options _options;
		private readonly Process _runner;

		public TestServer (Options options, Process runner) {
			_options = options;
			_runner = runner;
		}

		public void Start() {
			string currentTest = string.Empty; ;
			var testNotifier = new ResultPublisher();
			testNotifier.TestStartedEvent += test => currentTest = test;
			ITestRunner runner = ConnectionFactory.RegisterAsServer(new CompositeTestOutput(_options.Output, testNotifier), _options);
			IList<TestAssembly> allTests = runner.GetTestSuite();
			foreach (var assembly in allTests) {
				foreach (var testClass in assembly.Classes) {
					foreach (var testMethod in testClass.Methods) {
						var testRun = Task.Factory.StartNew(() => runner.RunTest(assembly.Name, testClass.Name, testMethod.Name));
						if (!testRun.Wait(testMethod.Timeout)) {
							_runner.Kill();
							_options.Output.TestCompleted(new TestDetails {
								Name = currentTest,
								Passed = false,
								Thrown = "Method exceeded a timeout period of " + testMethod.Timeout + "ms",
								Time = TimeSpan.FromMilliseconds(testMethod.Timeout)
							});
						}
					}
				}
			}
			runner.Exit();
		}	
	}
}