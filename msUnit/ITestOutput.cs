using System;
namespace msUnit {
	public interface ITestOutput {
		void TestSuiteStarted(string name);
		void TestSuiteFinished();
		void AssemblyError(Exception thrown);
		void TestStarted(string name);
		void TestRunCompleted();
		void TestCompleted(TestDetails details);
		void TestIgnored(string name, string reason);
	}
}
