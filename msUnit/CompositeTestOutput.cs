using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace msUnit {

	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class CompositeTestOutput : ITestOutput {

		private readonly List<ITestOutput> _outputs;

		public CompositeTestOutput(params ITestOutput[] outputs)
			: this(new List<ITestOutput>(outputs)) {
		}

		public CompositeTestOutput(IEnumerable<ITestOutput> outputs) {
			_outputs = new List<ITestOutput>(outputs);
		}

		public void TestSuiteStarted(string name) {
			_outputs.ForEach(output => output.TestSuiteStarted(name));
		}

		public void TestSuiteFinished() {
			_outputs.ForEach(output => output.TestSuiteFinished());
		}

		public void AssemblyError(Exception thrown) {
			_outputs.ForEach(output => output.AssemblyError(thrown));
		}

		public void TestStarted(string name) {
			_outputs.ForEach(output => output.TestStarted(name));
		}

		public void TestRunCompleted() {
			_outputs.ForEach(output => output.TestRunCompleted());
		}

		public void TestCompleted(TestDetails details) {
			_outputs.ForEach(output => output.TestCompleted(details));
		}

		public void TestIgnored(string name, string reason) {
			_outputs.ForEach(output => output.TestIgnored(name, reason));
		}

		public void Ping() { }
	}
}

