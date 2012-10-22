using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace msUnit {

	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class TeamcityOutput : ITestOutput {
		private string _suite;

		public void TestSuiteStarted(string name) {
			_suite = name;
			Console.WriteLine("##teamcity[testSuiteStarted name='{0}']", name);
		}

		public void AssemblyError(Exception thrown) {
			string testName = _suite + " - (assembly error)";
			Console.WriteLine("##teamcity[testStarted name='{0}']", testName);
			Console.WriteLine("##teamcity[testFailed name='{0}' details='{1}']", testName, thrown.ToString());
		}

		public void TestStarted(string name) {
			Console.WriteLine("##teamcity[testStarted name='{0}']", name);
		}

		public void TestCompleted(TestDetails details) {
			if (!string.IsNullOrEmpty(details.StdOut)) {
				Console.WriteLine("##teamcity[testStdOut name='{0}' out='{1}']", details.Name, details.StdOut);
			}
			if (!string.IsNullOrEmpty(details.StdErr)) {
				Console.WriteLine("##teamcity[testStdErr name='{0}' out='{1}']", details.Name, details.StdErr);
			}
			if (!details.Passed) {
				Console.WriteLine("##teamcity[testFailed name='{0}' details='{1}']", details.Name, details.Thrown);
			}
			Console.WriteLine("##teamcity[testFinished name='{0}']", details.Name);
		}

		public void TestIgnored(string name, string reason) {
			Console.WriteLine("##teamcity[testIgnored name='{0}' message='{1}']", name, reason);
		}

		public void TestRunCompleted() {
		}
		
		public void TestSuiteFinished() {
			Console.WriteLine("##teamcity[testSuiteFinished  name='{0}']", _suite);
		}
		
		public void Ping() { }
	}
}
