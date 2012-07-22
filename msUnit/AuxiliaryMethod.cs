using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace msUnit {
    interface IAuxiliaryMethod {
        bool Valid { get; }
        string Error { get; }
    }
    class AuxiliaryMethod<T> : IAuxiliaryMethod where T : Attribute {

        public bool Exists { get; private set; }
        public bool Valid { get; private set; }
        private readonly MethodInfo _method;
        public string Error { get; private set; }
        public string Name { get { return _method.Name; } }

        public AuxiliaryMethod(IEnumerable<MethodInfo> methods, bool @static = false) {
            var candidates = methods.Where(method => method.GetCustomAttributes(typeof(T), true).Any()).ToList();
            switch (candidates.Count) {
                case 0:
                    Exists = false;
                    Valid = true;
                    break;
                case 1:
                    _method = candidates.First();
                    if (_method.IsStatic != @static) {
                        Error = Name + "should" + (@static ? " " : " not ") + "be static.";
                        Exists = Valid = false;
                    } else {
                        Exists = Valid = true;
                    }
                    break;
                default:
                    Error = string.Format("Two many {0} methods in {1}", typeof(T).Name, candidates.First().DeclaringType);
                    Valid = false;
                    Exists = false;
                    break;
            }
        }

        public bool Invoke(object instance, out Exception thrown) {
            thrown = null;
            if (_method == null) {
                return true;
            }
            if (!Valid) {
                thrown = new Exception(Error);
                return false;
            }
            try {
                _method.Invoke(instance, new object[] { });
                return true;
            } catch(TargetInvocationException e) {
                thrown = e.InnerException;
                return false;
            }
        }
    }
}
