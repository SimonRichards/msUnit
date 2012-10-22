using System;
using System.Linq;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.ServiceModel;

namespace msUnit {

	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	class TestRunner : ITestRunner {
		private readonly Options _options;
		public readonly ManualResetEvent Complete;
		private IList<TestAssembly> _assemblies;
		private readonly ITestOutput _output;

		public TestRunner(Options options) {
			Complete = new ManualResetEvent(false);
			_options = options;
			_output = ConnectionFactory.RegisterAsClient(this, options);
		}

		public IList<TestAssembly> GetTestSuite() {
			return _assemblies ?? (_assemblies = _options.Assemblies.Select(file => new TestAssembly(file, _options.Filters)).ToList());
		}

		public void RunTest(string assemblyName, string className, string testName) {
			TestAssembly testAssembly = _assemblies.First(assembly => assembly.Name == assemblyName);
			string failure;
			_output.TestStarted(testName);
			var timer = Stopwatch.StartNew();
			if (testAssembly.IsSane(out failure) && testAssembly.AssemblyInitialize(out failure)) {
				var testClass = testAssembly[className];
				if (testClass.IsSane(out failure) && testClass.ClassInitialize(out failure)) {
					var result = testClass.RunTest(testClass[testName]);
					_output.TestCompleted(result);
				}
			} else {
				_output.TestCompleted(new TestDetails {
					Name = testName,
					Passed = false,
					Thrown = failure,
					Time = timer.Elapsed
				});
			}
		}

		public void ClassCleanup(string assemblyName, string className) {
			string failure;
			TestAssembly testAssembly = _assemblies.First(assembly => assembly.Name == assemblyName);
			if (testAssembly.IsSane(out failure)) {
				var testClass = testAssembly[className];
				if (testClass.IsSane(out failure) && testClass.HasAssemblyCleanup) {
					if (!testClass.ClassCleanup(out failure)) {
						var cleanupDetails = new TestDetails {
							Name = testClass + ".ClassCleanup",
							Passed = false,
							Thrown = failure,
							Time = TimeSpan.Zero
						};
						_output.TestStarted(cleanupDetails.Name);
						_output.TestCompleted(cleanupDetails);
					}
				}
			}
		}

		public void AssemblyCleanup(string assemblyName) {
			string failure;
			TestAssembly testAssembly = _assemblies.First(assembly => assembly.Name == assemblyName);
			if (testAssembly.IsSane(out failure)) {
				if (!testAssembly.AssemblyCleanup(out failure)) {
					var cleanupDetails = new TestDetails {
						Name = "AssemblyCleanup",
						Passed = false,
						Thrown = failure,
						Time = TimeSpan.Zero
					};
					_output.TestStarted(cleanupDetails.Name);
					_output.TestCompleted(cleanupDetails);
				}
			}
		}

		public void Exit() {
			Complete.Set();
		}

		public void Ping() { }
	}
}