using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;

namespace msUnit {

	[DataContract]
	public class TestAssembly {
		private readonly Assembly _testAssembly;
		private readonly IList<TestClass> _initializeClasses;
		private readonly IList<TestClass> _cleanupClasses;
		private readonly IList<IFilter> _filters;

		private bool _initialised = false;

		[DataMember]
		public readonly IList<TestClass> Classes;

		[DataMember]
		public readonly string Name;

		public TestAssembly(string assemblyName, IList<IFilter> filters) {
			Name = assemblyName;
			_filters = filters;
			_testAssembly = Assembly.UnsafeLoadFrom(assemblyName);
			Classes = (
                from type in _testAssembly.GetTypes()
                where type.GetCustomAttributes(typeof(TestClassAttribute), true).Any()
				where !type.IsAbstract
				select new TestClass(type, _filters)).ToList();

			_initializeClasses = Classes.Where(testClass => testClass.HasAssemblyInitialize).ToList();
			_cleanupClasses = Classes.Where(testClass => testClass.HasAssemblyCleanup).ToList();
		}

		public bool IsSane(out string reason) {
			if (!Classes.Any()) {
				reason = "No test classes found.";
			} else if (_initializeClasses.Count() > 1) {
				reason = "Only 1 method may be marked with AssemblyInitializeAttribute";
			} else if (_cleanupClasses.Count() > 1) {
				reason = "Only 1 method may be marked with AssemblyCleanupAttribute";
			} else {
				reason = null;
				return true;
			}
			return false;
		}

		public bool AssemblyInitialize(out string thrown) {
			if (!_initialised && _initializeClasses.Any()) {
				_initialised = true;
				return _initializeClasses.First().AssemblyInitialize(out thrown);
			}
			thrown = null;
			return true;
		}

		public bool AssemblyCleanup(out string thrown) {
			if (_cleanupClasses.Any()) {
				return _cleanupClasses.First().AssemblyCleanup(out thrown);
			}
			thrown = null;
			return true;
		}

		public TestClass this[string className] {
			get {
				return Classes.First(@class => @class.Name == className);
			}
		}

		public override string ToString() {
			return Name;
		}
	}
}
