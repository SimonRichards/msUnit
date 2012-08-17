using msUnit;
using System;

public class SkipUntilFilter : IFilter {
	private readonly string _crashingTest;
	bool found = false;

	public SkipUntilFilter(string crashingTest) {
		_crashingTest = crashingTest;
	}

	public bool Test(Type type, System.Reflection.MethodInfo method) {
		if (found) {
			return true;
		}
		if (_crashingTest == type.FullName + '.' + method.Name) {
			found = true;
		}
		return false;
	}                                                                                                                                                                                                           
}
