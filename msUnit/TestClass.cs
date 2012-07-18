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
        public string Name { get; private set; }
        private readonly object[] _noArgs;

        public TestClass(IEnumerable<ConstructorInfo> ctors, string name) {
            _validCtors = ctors.Where(ctor => !ctor.GetParameters().Any()).ToList();
            Name = name;
            _noArgs = new object[] { };
        }

        internal void Test() {
            if (_validCtors.Count != 1) {
                throw new Exception(Name + " has no no-arg ctors.");
            }
            object testInstance = _validCtors[0].Invoke(new object[] { });
            _methods = testInstance.GetType().GetMethods();
            var classInitializeMethods = _methods.SelectWithAttribute(typeof(ClassInitializeAttribute)).ToList();
            var classCleanupMethods = _methods.SelectWithAttribute(typeof(ClassCleanupAttribute)).ToList();
            var testMethods = _methods.SelectWithAttribute(typeof(TestMethodAttribute));
            
            if (classInitializeMethods.Count() > 1) {
                throw new Exception("Too many methods marked with ClassInitializeAttribute in " + Name);
            }
            if (classCleanupMethods.Count() > 1) {
                throw new Exception("Too many methods marked with ClassCleanupAttribute in " + Name);
            }

            if (classInitializeMethods.Any()) {
                classInitializeMethods[0].Invoke(testInstance, _noArgs);
            }

            foreach (var testMethod in testMethods) {
                try {
                    testMethod.Invoke(testInstance, _noArgs);
                    Console.WriteLine("Passed\t{0}", testMethod.Name);
                } catch (TargetException e) {
                    Console.WriteLine("Failed\t{0}", testMethod.Name);
                    Console.WriteLine("\t" + e.InnerException);
                }
            }

            if (classCleanupMethods.Any()) {
                classCleanupMethods[0].Invoke(testInstance, _noArgs);
            }
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
