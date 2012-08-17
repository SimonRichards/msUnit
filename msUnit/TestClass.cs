﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;

namespace msUnit {
	class TestClass {
		private readonly Type _type;
		private readonly Action<string> _testStarted;
		private readonly Func<object> _ctor;
		private readonly IList<MethodInfo> _testMethods;
		private readonly AuxiliaryMethod<AssemblyInitializeAttribute> _assemblyInitialize;
		private readonly AuxiliaryMethod<AssemblyCleanupAttribute> _assemblyCleanup;
		private readonly AuxiliaryMethod<ClassInitializeAttribute> _classInitialize;
		private readonly AuxiliaryMethod<ClassCleanupAttribute> _classCleanup;
		private readonly AuxiliaryMethod<TestInitializeAttribute> _testInitialize;
		private readonly AuxiliaryMethod<TestCleanupAttribute> _testCleanup;
		private readonly StringBuilder _stdOutBuffer;
		private readonly StringBuilder _stdErrBuffer;
		private readonly IDictionary _context = new Dictionary<int, int>();

		public string Name { get; private set; }

		private readonly object[] _noArgs = new object[] { };

		public TestClass(Type type, IEnumerable<IFilter> filters, Action<string> testStartedHandler) {
			_type = type;
			_testStarted = testStartedHandler;
			var validCtor = type.GetConstructor(new Type[] { });
			_ctor = validCtor == null ? (Func<object>)null : () => validCtor.Invoke(_noArgs);
			Name = type.Name;
			List<MethodInfo> methods = new List<MethodInfo>();
			for (var testType = type; testType != typeof(object); testType = testType.BaseType) {
				foreach (var method in testType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
					methods.Insert(0, method);
				}
			}
			_testMethods = (
                from method in methods
                where method.GetCustomAttributes(typeof(TestMethodAttribute), true).Any()
                where filters.All(filter => filter.Test(type, method))
                select method).ToList();
			_assemblyInitialize = new AuxiliaryMethod<AssemblyInitializeAttribute>(methods,  @static: true);
			_assemblyCleanup = new AuxiliaryMethod<AssemblyCleanupAttribute>(methods,  @static: true);
			_classInitialize = new AuxiliaryMethod<ClassInitializeAttribute>(methods,  @static: true);
			_classCleanup = new AuxiliaryMethod<ClassCleanupAttribute>(methods,  @static: true);
			_testCleanup = new AuxiliaryMethod<TestCleanupAttribute>(methods);
			_testInitialize = new AuxiliaryMethod<TestInitializeAttribute>(methods);
			_stdOutBuffer = new StringBuilder();
			_stdErrBuffer = new StringBuilder();
		}

		public IEnumerable<TestDetails> Test() {
			if (_testMethods.Any()) {
				string message;
				return IsClassValid(out message) ? RunTestsAndCleanup() : FailWholeClass(message);
			}
			return new TestDetails[] { };
		}

		private IEnumerable<TestDetails> RunTestsAndCleanup() {
			Exception thrown;
			if (!_classInitialize.Invoke(null, out thrown, new [] { new TestContext(_context) })) {
				return FailWholeClass(_classInitialize.Error);
			}
			var testResults = RunTests();
			
			if (_classCleanup.Invoke(null, out thrown)) {
				return testResults;
			}
			var cleanupDetails = new TestDetails {
                Name = _classCleanup + "." + _classCleanup.Name,
                Passed = false,
                Thrown = thrown,
                Time = TimeSpan.Zero
            };
			return testResults.Concat(new[] { cleanupDetails });
		}

		private IEnumerable<TestDetails> RunTests() {
			var timer = new Stopwatch();
			var stdOut = Console.Out;
			var stdError = Console.Error;
			foreach (var testMethod in _testMethods) {
				var details = new TestDetails { Name = _type.FullName + "." + testMethod.Name, Passed = true };
				_testStarted(details.Name);
				Console.SetOut(new StringWriter(_stdOutBuffer));
				Console.SetError(new StringWriter(_stdErrBuffer));
				timer.Restart();
				try {
					object instance = _ctor();
					details.Passed = _testInitialize.Invoke(instance, out details.Thrown);
					if (details.Passed) {
						testMethod.Invoke(instance, _noArgs);
						details.Passed = _testCleanup.Invoke(instance, out details.Thrown);
					}
				} catch (Exception e) {
					details.Passed = false;
					details.Thrown = e.InnerException;
				}
				details.Time = timer.Elapsed;
				details.StdOut = _stdOutBuffer.ToString();
				details.StdErr = _stdErrBuffer.ToString();
				Console.SetOut(stdOut);
				Console.SetError(stdError);
				yield return details;
				_stdOutBuffer.Clear();
				_stdErrBuffer.Clear();
			}
		}

		private bool IsClassValid(out string message) {
			if (_ctor == null) {
				message = Name + " has no no-arg ctors.";
				return false;
			}
			var invalid = new IAuxiliaryMethod[] {
                _classCleanup,
                _classInitialize,
                _testCleanup,
                _testInitialize
            }.FirstOrDefault(m => !m.Valid);
			if (invalid != null) {
				message = invalid.Error;
				return false;
			}
			message = string.Empty;
			return true;
		}

		private IEnumerable<TestDetails> FailWholeClass(string message) {
			return _testMethods.Select(method => new TestDetails {
                Passed = false,
                Thrown = new TestException(message),
                Time = TimeSpan.Zero,
				Name = _type.FullName + "." + method.Name
            }
			);
		}

		public bool HasAssemblyCleanup {
			get { return _assemblyCleanup.Exists; }
		}

		public bool HasAssemblyInitialize {
			get { return _assemblyInitialize.Exists; }
		}

		public bool AssemblyInitialize(out Exception thrown) {
			Debug.Assert(HasAssemblyInitialize);
			return _assemblyInitialize.Invoke(null, out thrown, new TestContext(_context));
		}

		public bool AssemblyCleanup(out Exception thrown) {
			Debug.Assert(HasAssemblyCleanup);
			return _assemblyCleanup.Invoke(null, out thrown);
		}
	}
}
