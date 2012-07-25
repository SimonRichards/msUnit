using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace msUnit {
	class TestAssembly {
		private readonly Assembly _testAssembly;
		private readonly IList<TestClass> _testClasses;
		private readonly IList<TestClass> _initializeClasses;
		private readonly IList<TestClass> _cleanupClasses;
		private readonly IList<IFilter> _filters; 

		public TestAssembly(string assemblyName, IList<IFilter> filters) {
			_filters = filters;
			_testAssembly = Assembly.UnsafeLoadFrom(assemblyName);
			var testCompleted = new Action<string>(s => TestStartedHandler(s));
			_testClasses = (
                from type in _testAssembly.GetTypes()
                where type.GetCustomAttributes(typeof(TestClassAttribute), true).Any()
				where !type.IsAbstract
                select new TestClass(type, _filters, testCompleted)).ToList();

			_initializeClasses = _testClasses.Where(testClass => testClass.HasAssemblyInitialize).ToList();
			_cleanupClasses = _testClasses.Where(testClass => testClass.HasAssemblyCleanup).ToList();
		}

		public void Test() {
			CheckInvariants();
			AssemblyInitialize();
			RunTests();
			AssemblyCleanup();
		}

		private void CheckInvariants() {
			if (!_testClasses.Any())
				AssemblyErrorHandler(new TestException("No test classes found."));
			if (_initializeClasses.Count() > 1)
				AssemblyErrorHandler(new TestException("Only 1 method may be marked with AssembleInitializeAttribute"));
			if (_cleanupClasses.Count() > 1)
				AssemblyErrorHandler(new TestException("Only 1 method may be marked with AssembleCleanupAttribute"));
		}

		private void AssemblyInitialize() {
			if (_initializeClasses.Any()) {
				Exception thrown;
				if (!_initializeClasses[0].AssemblyInitialize(out thrown)) {
					AssemblyErrorHandler(thrown);
				}
			}
		}

		private void RunTests() {
			foreach (var result in _testClasses.SelectMany(testClass => testClass.Test()))
				TestCompleteHandler(result);
		}

		private void AssemblyCleanup() {
			if (_cleanupClasses.Any()) {
				Exception thrown;
				if (!_cleanupClasses[0].AssemblyCleanup(out thrown)) {
					AssemblyErrorHandler(thrown);
				}
			}
		}

		public event TestStarted TestStartedHandler = delegate { };
		public event AssemblyError AssemblyErrorHandler = delegate { };
		public event TestComplete TestCompleteHandler = delegate { };

		public delegate void TestStarted(string name);
		public delegate void AssemblyError(Exception details);
		public delegate void TestComplete(TestDetails details);
	}
}
