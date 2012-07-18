using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace msUnit {
    class TestAssembly {
        private readonly Assembly _testAssembly;
        private readonly IList<TestClass> _testClasses;
        private readonly TestClass _initializeClass;
        private readonly TestClass _cleanupClass;

        public TestAssembly(string assemblyName) {
            _testAssembly = Assembly.UnsafeLoadFrom(assemblyName);
            _testClasses = (
                from type in _testAssembly.GetTypes()
                where type.GetCustomAttributes(typeof(TestClass), true).Any()
                select new TestClass(type.GetConstructors(), type.FullName)).ToList();

            var initializeClasses = _testClasses.Where(testClass => testClass.HasAssemblyInitialize).ToList();
            var cleanupClasses = _testClasses.Where(testClass => testClass.HasAssemblyCleanup).ToList();

            if (initializeClasses.Count > 1) {
                throw new Exception("Only 1 method may be marked with AssembleInitializeAttribute");
            } else if (initializeClasses.Count == 1) {
                _initializeClass = initializeClasses[0];
            }

            if (cleanupClasses.Count > 1) {
                throw new Exception("Only 1 method may be marked with AssembleCleanupAttribute");
            } else if (cleanupClasses.Count == 1) {
                _cleanupClass = cleanupClasses[0];
            }
        }

        internal void Test() {
            _initializeClass.AssemblyInitialize();
            foreach (var testClass in _testClasses) {
                testClass.Test();
            }
            _cleanupClass.AssemblyCleanup();
        }
    }
}
