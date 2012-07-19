﻿using System;
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

        public TestAssembly(string assemblyName) {
            _testAssembly = Assembly.UnsafeLoadFrom(assemblyName);
            _testClasses = (
                from type in _testAssembly.GetTypes()
                where type.GetCustomAttributes(typeof(TestClassAttribute), true).Any()
                select new TestClass(type.GetConstructors(), type.FullName)).ToList();

            _initializeClasses = _testClasses.Where(testClass => testClass.HasAssemblyInitialize).ToList();
            _cleanupClasses = _testClasses.Where(testClass => testClass.HasAssemblyCleanup).ToList();
        }

        internal void Test() {
            CheckInvariants();
            AssemblyInitialize();
            RunTests();
            AssemblyCleanup();
        }

        private void CheckInvariants() {
            if (!_testClasses.Any())
                AssemblyErrorHandler("No test classes found.");
            if (_initializeClasses.Count() > 1)
                AssemblyErrorHandler("Only 1 method may be marked with AssembleInitializeAttribute");
            if (_cleanupClasses.Count() > 1)
                AssemblyErrorHandler("Only 1 method may be marked with AssembleCleanupAttribute");
        }

        private void AssemblyInitialize() {
            if (_initializeClasses.Any()) {
                try {
                    _initializeClasses[0].AssemblyInitialize();
                } catch (Exception e) {
                    AssemblyErrorHandler("Assembly initialization failed: " + e.Message);
                }
            }
        }

        private void RunTests() {
            foreach (var result in _testClasses.SelectMany(testClass => testClass.Test()))
                TestCompleteHandler(result);
        }

        private void AssemblyCleanup() {
            if (_cleanupClasses.Any()) {
                try {
                    _cleanupClasses[0].AssemblyCleanup();
                } catch (Exception e) {
                    AssemblyErrorHandler("Assembly cleanup failed: " + e.Message);
                }
            }
        }

        internal event AssemblyError AssemblyErrorHandler = delegate { };
        internal event TestComplete TestCompleteHandler = delegate { };

        internal delegate void AssemblyError(string details);
        internal delegate void TestComplete(TestDetails details);
    }
}
