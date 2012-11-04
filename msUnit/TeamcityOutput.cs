using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace msUnit {
	public class TeamcityOutput : ITestOutput {
		private string _suite;

		public void TestSuiteStarted(string name) {
			_suite = Escape(name);
			Console.WriteLine("##teamcity[testSuiteStarted name='{0}']", _suite);
		}

		public void AssemblyError(Exception thrown) {
			string testName = _suite + " - (assembly error)";
			Console.WriteLine("##teamcity[testStarted name='{0}']", testName);
			Console.WriteLine("##teamcity[testFailed name='{0}' details='{1}']", testName, Escape(thrown.ToString()));
			Console.WriteLine("##teamcity[testFinished name='{0}']", testName);
		}

		public void TestStarted(string name) {
			Console.WriteLine("##teamcity[testStarted name='{0}']", Escape(name));
		}

		public void TestCompleted(TestDetails details) {
			if (!string.IsNullOrEmpty(details.StdOut)) {
				Console.WriteLine("##teamcity[testStdOut name='{0}' out='{1}']", Escape(details.Name), Escape(details.StdOut));
			}
			if (!string.IsNullOrEmpty(details.StdErr)) {
				Console.WriteLine("##teamcity[testStdErr name='{0}' out='{1}']", Escape(details.Name), Escape(details.StdErr));
			}
			if (!details.Passed) {
				Console.WriteLine("##teamcity[testFailed name='{0}' details='{1}']", Escape(details.Name), Escape(details.Thrown.ToString()));
			}
			Console.WriteLine("##teamcity[testFinished name='{0}']", Escape(details.Name));
		}

		public void TestIgnored(string name, string reason) {
			Console.WriteLine("##teamcity[testIgnored name='{0}' message='{1}']", Escape(name), Escape(reason));
		}

		public void TestRunCompleted() {
		}
		
		public void TestSuiteFinished() {
			Console.WriteLine("##teamcity[testSuiteFinished  name='{0}']", _suite);
		}
		
		private static string Escape(string input) {
			var substitutions = new[] {
				new { Invalid = "|", Escaped = "||" },
				new { Invalid = "\n", Escaped = "|n" },
				new { Invalid = "\r", Escaped = "|r" },
				new { Invalid = "]", Escaped = "|]" },
				new { Invalid = "'", Escaped = "|'" } };
			
			foreach (var substution in substitutions) {
				input = input.Replace(substution.Invalid, substution.Escaped);
			}
			return input;
		}
	}
}
