using System;
using System.Reflection;

namespace msUnit {
	interface IFilter {
		bool Test(Type type, MethodInfo method);
	}
}
