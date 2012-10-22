using System;
using System.Reflection;

namespace msUnit {
	public interface IFilter {
		bool Test(Type type, MethodInfo method);
	}
}
