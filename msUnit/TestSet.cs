using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace msUnit {

	[DataContract]
	public class TestSet {

		[DataMember]
		private readonly ISet<string> _tests;

		public TestSet(IEnumerable<string> names) {
			_tests = new HashSet<string>(names);
		}

		public void RemoveTest(string name) {
			_tests.Remove(name);
		}

		public IEnumerable<string> Tests {
			get {
				return _tests;
			}
		}
	}

	[DataContract]
	public class ClassSet {

		[DataMember]
		private readonly Dictionary<string, TestSet> _classes;

		public ClassSet(IEnumerable<Tuple<string, TestSet>> classes) {
			_classes = new Dictionary<string, TestSet>();
			foreach (var tuple in classes) {
				_classes[tuple.Item1] = tuple.Item2;
			}
		}

		public IEnumerable<string> Classes {
			get {
				foreach (var @class in _classes) {
					if (!@class.Value.Tests.Any()) {
						_classes.Remove(@class.Key);
					}
				}
				return _classes.Keys;
			}
		}
		
		public TestSet this[string className] {
			get {
				return _classes[className];
			}
		}
	}
	
	[DataContract]
	public class AssemblySet {
		
		[DataMember]
		private readonly Dictionary<string, ClassSet> _assemblies;
		
		public AssemblySet(IEnumerable<Tuple<string, ClassSet>> assemblies) {
			_assemblies = new Dictionary<string, ClassSet>();
			foreach (var tuple in assemblies) {
				_assemblies[tuple.Item1] = tuple.Item2;
			}
		}
		
		public IEnumerable<string> Assemblies {
			get {
				foreach (var assembly in _assemblies) {
					if (!assembly.Value.Classes.Any()) {
						_assemblies.Remove(assembly.Key);
					}
				}
				return _assemblies.Keys;
			}
		}

		public ClassSet this[string assemblyName] {
			get {
				return _assemblies[assemblyName];
			}
		}
	}
}

