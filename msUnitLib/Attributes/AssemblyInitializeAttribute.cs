namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
	using System;

	[AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=true)]
	public class AssemblyInitializeAttribute : Attribute
	{
	}
}
