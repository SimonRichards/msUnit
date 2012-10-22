using System;
using System.Runtime.Serialization;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace msUnit {

	[DataContract]
	public class TestMethod {
		private static int DefaultTimeout = 60000;

		[DataMember]
		public string Name;

		[DataMember]
		public int Timeout;

		private readonly MethodInfo _methodInfo;

		public TestMethod(MethodInfo methodInfo) {
			_methodInfo = methodInfo;
			Name = methodInfo.Name;
			var attributes = methodInfo.GetCustomAttributes(typeof(TimeoutAttribute), true);
			Timeout = attributes.Any() ?
				(int)attributes.Cast<TimeoutAttribute>().First().Properties["Timeout"]
				: DefaultTimeout;
		}

		public void Invoke(object instance) {
			_methodInfo.Invoke(instance, new object[] { });
		}

		public override string ToString() {
			return Name;
		}
	}
}

