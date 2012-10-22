using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Serialization;

namespace msUnit {

	[DataContract]
	public class TestClass {
		private readonly Type _type;
		private readonly Func<object> _ctor;
		private readonly AuxiliaryMethod<AssemblyInitializeAttribute> _assemblyInitialize;
		private readonly AuxiliaryMethod<AssemblyCleanupAttribute> _assemblyCleanup;
		private readonly AuxiliaryMethod<ClassInitializeAttribute> _classInitialize;
		private readonly AuxiliaryMethod<ClassCleanupAttribute> _classCleanup;
		private readonly AuxiliaryMethod<TestInitializeAttribute> _testInitialize;
		private readonly AuxiliaryMethod<TestCleanupAttribute> _testCleanup;
		private readonly IDictionary _context = new Dictionary<int, int>();
		
		private InitialisationState _initialisation = InitialisationState.NotInitialised;
		private string _initialisationException;

		[DataMember]
		public readonly List<TestMethod> Methods;

		[DataMember]
		public string Name { get; set; }

		private readonly object[] _noArgs = new object[] { };

		public TestClass(Type type, IEnumerable<IFilter> filters) {
			_type = type;
			var validCtor = type.GetConstructor(new Type[] { });
			_ctor = validCtor == null ? (Func<object>)null : () => validCtor.Invoke(_noArgs);
			Name = type.Name;
			List<MethodInfo> methods = new List<MethodInfo>();
			for (var testType = type; testType != typeof(object); testType = testType.BaseType) {
				foreach (var method in testType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
					methods.Insert(0, method);
				}
			}
			Methods = (
                from method in methods
                where method.GetCustomAttributes(typeof(TestMethodAttribute), true).Any()
                where filters.All(filter => filter.Test(type, method))
                select new TestMethod(method)).ToList();
			_assemblyInitialize = new AuxiliaryMethod<AssemblyInitializeAttribute>(methods,  @static: true);
			_assemblyCleanup = new AuxiliaryMethod<AssemblyCleanupAttribute>(methods,  @static: true);
			_classInitialize = new AuxiliaryMethod<ClassInitializeAttribute>(methods,  @static: true);
			_classCleanup = new AuxiliaryMethod<ClassCleanupAttribute>(methods,  @static: true);
			_testCleanup = new AuxiliaryMethod<TestCleanupAttribute>(methods);
			_testInitialize = new AuxiliaryMethod<TestInitializeAttribute>(methods);
		}

		public TestDetails RunTest(TestMethod testMethod) {
			var stdOutBuffer = new StringBuilder();
			var stdErrBuffer = new StringBuilder();
			var stdOut = Console.Out;
			var stdError = Console.Error;
			var details = new TestDetails { Name = _type.FullName + "." + testMethod.Name, Passed = true };
			Console.SetOut(new StringWriter(stdOutBuffer));
			Console.SetError(new StringWriter(stdErrBuffer));
			object instance = null;
			var timer = Stopwatch.StartNew();
			try {
				instance = _ctor();
				details.Passed = _testInitialize.Invoke(instance, out details.Thrown);
				if (details.Passed) {
					testMethod.Invoke(instance);
				}
			} catch (Exception e) {
				details.Passed = false;
				details.Thrown = e.InnerException.ToString();
			}
			details.Passed = details.Passed && _testCleanup.Invoke(instance, out details.Thrown);
			details.Time = timer.Elapsed;
			details.StdOut = stdOutBuffer.ToString();
			details.StdErr = stdErrBuffer.ToString();
			Console.SetOut(stdOut);
			Console.SetError(stdError);
			return details;
		}

		public bool IsSane(out string reason) {
			if (_ctor == null) {
				reason = new TestException(Name + " has no no-arg ctors.").ToString();
				return false;
			}
			var invalid = new IAuxiliaryMethod[] {
                _classCleanup,
                _classInitialize,
                _testCleanup,
                _testInitialize
            }.FirstOrDefault(m => !m.Valid);
			if (invalid != null) {
				reason = new TestException(invalid.Error).ToString();
				return false;
			}
			reason = string.Empty;
			return true;
		}

		public bool ClassInitialize(out string failure) {
			if (_initialisation == InitialisationState.Initialised) {
				failure = null;
				return true;
			}
			if (!_classInitialize.Valid) {
				failure = new TestException(_classInitialize.Error).ToString();
				return false;
			} 
			if (_initialisation == InitialisationState.InitialisationFailed) {
				failure = _initialisationException.ToString();
				return false;
			} 
			if (_classInitialize.Invoke(null, out failure, new [] { new TestContext(_context) })) {
				_initialisation = InitialisationState.Initialised;
				return true;
			}
			_initialisationException = failure;
			_initialisation = InitialisationState.InitialisationFailed;
			return true;
		}

		public bool ClassCleanup(out string failure) {
			if (_classCleanup.Valid) {
				return _classCleanup.Invoke(null, out failure, new[] { new TestContext(_context) });
			}
			failure = string.Empty;
			return true;
		}

		public bool HasAssemblyCleanup {
			get { return _assemblyCleanup.Exists; }
		}

		public bool HasAssemblyInitialize {
			get { return _assemblyInitialize.Exists; }
		}

		public bool AssemblyInitialize(out string thrown) {
			Debug.Assert(HasAssemblyInitialize);
			return _assemblyInitialize.Invoke(null, out thrown, new TestContext(_context));
		}

		public bool AssemblyCleanup(out string thrown) {
			Debug.Assert(HasAssemblyCleanup);
			return _assemblyCleanup.Invoke(null, out thrown);
		}
		
		public TestMethod this[string testName] {
			get {
				return Methods.First(method => method.Name == testName);
			}
		}

		public override string ToString() {
			return Name;
		}
	}
}
