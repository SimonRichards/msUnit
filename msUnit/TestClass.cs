using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace msUnit {
    class TestClass {
        private readonly IList<ConstructorInfo> _validCtors;
        private IList<MethodInfo> _methods;
        private IList<MethodInfo> _classInitializeMethods;
        private IList<MethodInfo> _classCleanupMethods;
        private IList<MethodInfo> _testMethods;
        public string Name { get; private set; }
        private readonly object[] _noArgs;
        private object _testInstance;

        public TestClass(IEnumerable<ConstructorInfo> ctors, string name) {
            _validCtors = ctors.Where(ctor => !ctor.GetParameters().Any()).ToList();
            Name = name;
            _noArgs = new object[] { };
        }

        internal IEnumerable<TestDetails> Test() {
            string message;
            if (!TryConstructClass(out message)) {
                return FailWholeClass(message);
            }

            StoreMethods();

            if (!IsClassValid(out message))
                return FailWholeClass(message);
            else if (TryInitializeClass(out message))
                return FailWholeClass(message);
            else
                return RunTestsAndCleanup();
        }

        private void StoreMethods() {
            _methods = _testInstance.GetType().GetMethods();
            _classInitializeMethods = _methods.SelectWithAttribute(typeof (ClassInitializeAttribute)).ToList();
            _classCleanupMethods = _methods.SelectWithAttribute(typeof (ClassCleanupAttribute)).ToList();
            _testMethods = _methods.SelectWithAttribute(typeof (TestMethodAttribute)).ToList();
        }

        private bool TryConstructClass(out string message) {
            try {
                _testInstance = _validCtors[0].Invoke(_noArgs);
                message = string.Empty;
                return true;
            } catch (Exception e) {
                message = e.InnerException.Message;
                return false;
            }
        }

        private bool TryInitializeClass(out string message) {
            if (_classInitializeMethods.Any()) {
                try {
                    _classInitializeMethods[0].Invoke(_testInstance, _noArgs);
                } catch (Exception e) {
                    message = e.InnerException.Message;
                    return false;
                }
            }
            message = string.Empty;
            return true;
        }

        private IEnumerable<TestDetails> RunTestsAndCleanup() {
            var testResults = RunTests();
            if (!_classCleanupMethods.Any()) {
                yield break;
            }
            var cleanupDetails = new TestDetails();
            bool cleanupFailed = false;
            try {
                _classCleanupMethods[0].Invoke(_testInstance, _noArgs);
            } catch (Exception e) {
                cleanupDetails = new TestDetails {
                    Name = _classCleanupMethods[0].Name,
                    Passed = false,
                    Thrown = e.InnerException,
                    Time = TimeSpan.Zero
                };
                cleanupFailed = true;
            }
            foreach (var test in testResults) {
                yield return test;
            }
            if (cleanupFailed) {
                yield return cleanupDetails;
            }
        }

        private IEnumerable<TestDetails> RunTests() {
            var timer = new Stopwatch();
            foreach (var testMethod in _testMethods) {
                var details = new TestDetails { Name = testMethod.Name, Passed = true };
                timer.Restart();
                try {
                    testMethod.Invoke(_testInstance, _noArgs);
                } catch (TargetException e) {
                    details.Passed = false;
                    details.Thrown = e.InnerException;
                }
                details.Time = timer.Elapsed;
                yield return details;
            }
        } 

        private bool IsClassValid(out string message) {
            if (_validCtors.Count != 1) {
                message = Name + " has no no-arg ctors.";
                return false;
            }
            if (_classInitializeMethods.Count() > 1 || _classCleanupMethods.Count() > 1) {
                message = "Too many ClassInitializers or ClassCleanups in " + Name;
                return false;
            }
            message = string.Empty;
            return true;
        }

        public IEnumerable<TestDetails> FailWholeClass(string message) {
            return _testMethods.Select(method => new TestDetails {
                Passed = false,
                Thrown = new Exception(message),
                Time = TimeSpan.Zero,
                Name = method.Name
            });
        } 

        public bool HasAssemblyCleanup {
            get { return false; }
        }

        public bool HasAssemblyInitialize {
            get { return false; }
        }

        internal void AssemblyInitialize() {
            Debug.Assert(HasAssemblyInitialize);
        }

        internal void AssemblyCleanup() {
            Debug.Assert(HasAssemblyCleanup);
        }
    }
}
