using System;
using System.ServiceModel;

namespace msUnit {

	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class ResultPublisher : ITestOutput {
		
		public event Action<string> TestSuiteStartedEvent = delegate { };
		public event Action TestSuiteFinishedEvent = delegate { };
		public event Action<Exception> AssemblyErrorEvent = delegate { };
		public event Action<string> TestStartedEvent = delegate { };
		public event Action TestRunCompletedEvent = delegate { };
		public event Action<TestDetails> TestCompletedEvent = delegate { };
		public event Action<string, string> TestIgnoredEvent = delegate { };

		public void TestSuiteStarted(string name) {
			TestSuiteStartedEvent(name);
		}

		public void TestSuiteFinished() {
			TestSuiteFinishedEvent();
		}

		public void AssemblyError(Exception thrown) {
			AssemblyErrorEvent(thrown);
		}

		public void TestStarted(string name) {
			TestStartedEvent(name);
		}

		public void TestRunCompleted() {
			TestRunCompletedEvent();
		}

		public void TestCompleted(TestDetails details) {
			TestCompletedEvent(details);
		}

		public void TestIgnored(string name, string reason) {
			TestIgnoredEvent(name, reason);
		}

		public void Ping() { }
	}
}

