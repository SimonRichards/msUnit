using System;
using System.ServiceModel;

namespace msUnit {

	/// <remarks>
	/// All implementing class must have the following attribute:
	/// [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	/// </remarks>
	[ServiceContract]
	public interface ITestOutput {

		[OperationContract]
		void TestSuiteStarted(string name);

		[OperationContract]
		void TestSuiteFinished();

		[OperationContract]
		void AssemblyError(Exception thrown);

		[OperationContract]
		void TestStarted(string name);

		[OperationContract]
		void TestRunCompleted();

		[OperationContract]
		void TestCompleted(TestDetails details);

		[OperationContract]
		void TestIgnored(string name, string reason);

		[OperationContract]
		void Ping();
	}
}
