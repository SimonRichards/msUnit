using System.Reflection;

namespace msUnit {
	interface IFilter {
		bool Test(MethodInfo method);
	}
}
