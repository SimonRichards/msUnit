using System;
using System.ServiceModel;
using System.Collections.Generic;

namespace msUnit {
	[ServiceContract]
	public interface ITestRunner {

		[OperationContract]
		IList<TestAssembly> GetTestSuite();

		[OperationContract]
		void RunTest(string assemblyName, string className, string test);

		[OperationContract]
		void ClassCleanup(string assemblyName, string className);

		[OperationContract]
		void AssemblyCleanup(string assemblyName);
		
		[OperationContract]
		void Exit();

		[OperationContract]
		void Ping();
	}
}