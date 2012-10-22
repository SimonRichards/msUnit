using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace msUnit {
	interface IAuxiliaryMethod {
		bool Valid { get; }

		string Error { get; }

		bool Invoke(object instance, out string thrown, params object[] args);
	}

	class AuxiliaryMethod<T> : IAuxiliaryMethod where T : Attribute {

		public bool Exists { get; private set; }

		public bool Valid { get; private set; }

		private readonly IList<MethodInfo> _methods;

		public string Error { get; private set; }

		public string Name { get { return _methods.Any() ? _methods[0].DeclaringType + "." + _methods[0].Name : ""; } }

		public AuxiliaryMethod(IEnumerable<MethodInfo> methods, bool @static = false, bool inherit = true) {
			_methods = new List<MethodInfo>();
			var candidates = methods.Where(method => method.GetCustomAttributes(typeof(T), true).Any()).ToList();
			switch (candidates.Count) {
			case 0:
				Exists = false;
				Valid = true;
				break;
			case 1:
				_methods.Add(candidates.First());
				if (candidates.First().IsStatic == @static) {
					Exists = Valid = true;
				} else {
					Error = Name + "should" + (@static ? " " : " not ") + "be static.";
					Exists = Valid = false;
				}
				break;
			default:
				if (inherit) {
					foreach (var candidate in candidates) {
						_methods.Add(candidate);
					}
					Valid = Exists = true;
				} else {
					Error = string.Format("Two many {0} methods in {1}", typeof(T).Name, candidates.First().DeclaringType);
					Valid = Exists = false;
				}
				break;
			}
		}

		public bool Invoke(object instance, out string thrown, params object[] args) {
			thrown = null;
			if (!Valid) {
				thrown = new TestException(Error).ToString();
				return false;
			}
			try {
				foreach (var method in _methods) {
					if (method.GetParameters().Length == args.Length) {
						method.Invoke(instance, args.ToArray());
					} else {
						thrown = new TestException(Name + " had wrong number of arguments.").ToString();
						return false;
					}
				}
			} catch (TargetInvocationException e) {
				thrown = e.InnerException.ToString();
				return false;
			}
			return true;
		}
	}
}
