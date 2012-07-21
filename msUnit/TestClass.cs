using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace msUnit {
    class TestClass {
        private readonly Type _type;
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
        public string Name { get; private set; }
        private readonly object[] _noArgs = new object[] { };

        public TestClass(Type type) {
            _type = type;
            var validCtor = type.GetConstructor(new Type[] { });
            _ctor = validCtor == null ? (Func<object>)null : () => validCtor.Invoke(_noArgs);
            Name = type.Name;
            IList<MethodInfo> methods = _type.GetMethods();
            _testMethods = methods.Where(method => method.GetCustomAttributes(typeof(TestMethodAttribute), true).Any()).ToList();
            _assemblyInitialize = new AuxiliaryMethod<AssemblyInitializeAttribute>(methods, @static: true);
            _assemblyCleanup = new AuxiliaryMethod<AssemblyCleanupAttribute>(methods, @static: true);
            _classInitialize = new AuxiliaryMethod<ClassInitializeAttribute>(methods, @static: true);
            _classCleanup = new AuxiliaryMethod<ClassCleanupAttribute>(methods, @static: true);
            _testCleanup = new AuxiliaryMethod<TestCleanupAttribute>(methods);
            _testInitialize = new AuxiliaryMethod<TestInitializeAttribute>(methods);
            _stdOutBuffer = new StringBuilder();
            _stdErrBuffer = new StringBuilder();
        }

        internal IEnumerable<TestDetails> Test() {
            string message;
            return IsClassValid(out message) ? RunTestsAndCleanup() : FailWholeClass(message);
        }

        private IEnumerable<TestDetails> RunTestsAndCleanup() {
            var testResults = RunTests();
            Exception thrown;
            if (_classCleanup.Invoke(null, out thrown)) {
                return testResults;
            }
            var cleanupDetails = new TestDetails {
                Name = _classCleanup.Name,
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
                var details = new TestDetails { Name = testMethod.Name, Passed = true };
                timer.Restart();
                Console.SetOut(new StringWriter(_stdOutBuffer));
                Console.SetError(new StringWriter(_stdErrBuffer));
                try {
                    object instance = _ctor();
                    details.Passed = _testInitialize.Invoke(instance, out details.Thrown);
                    if (details.Passed) {
                        testMethod.Invoke(instance, _noArgs);
                        details.Passed = _testCleanup.Invoke(instance, out details.Thrown);
                    }
                } catch (TargetInvocationException e) {
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

            foreach (var auxMethod in new IAuxiliaryMethod[] { _classCleanup, _classInitialize, _testCleanup, _testInitialize }) {
                if (!auxMethod.Valid) {
                    message = auxMethod.Error;
                    return false;
                }
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
            get { return _assemblyCleanup.Exists; }
        }

        public bool HasAssemblyInitialize {
            get { return _assemblyInitialize.Exists; }
        }

        internal bool AssemblyInitialize(out Exception thrown) {
            Debug.Assert(HasAssemblyInitialize);
            return _assemblyInitialize.Invoke(null, out thrown);
        }

        internal bool AssemblyCleanup(out Exception thrown) {
            Debug.Assert(HasAssemblyCleanup);
            return _assemblyCleanup.Invoke(null, out thrown);
        }
    }
}
