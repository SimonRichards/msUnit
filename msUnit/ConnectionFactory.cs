using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace msUnit {

	/// <summary>
	/// ConnectionFactory provides an ITestOutput to the test runner and an ITestRunner to the test server.
	/// The current implementation does not seem very robust, a NamedPipe binding would be preferable but
	/// this is not currently working on Mono.
	/// </summary>
	static class ConnectionFactory {

		private const int StartPort = 15436;

		public static ITestOutput RegisterAsClient(ITestRunner runner, Options options) {
			ServiceHost host = new ServiceHost(runner);
			int i;
			for (i = 1; i < 51; i += 10) {
				try {
					host.AddServiceEndpoint(typeof(ITestRunner), BindingFactory(), "http://localhost:" + (StartPort + i) + "/");
					break;
				} catch (AddressAlreadyInUseException) {
				}
			}
			host.Open();
			var start = DateTime.Now;
			Exception final = null;
			var res = new ChannelFactory<ITestOutput>(BindingFactory(), "http://localhost:" + (StartPort + i - 1) + "/").CreateChannel();
			while (DateTime.Now - start < TimeSpan.FromSeconds(5)) {
				try {
					res.Ping();
					return res;
				} catch (Exception e) {
					final = e;
				}
			}
			throw final;
		}

		public static ITestRunner RegisterAsServer(ITestOutput output, Options options) {
			var host = new ServiceHost(output);
			int i;
			for (i = 0; i < 50; i += 10) {
				try {
					host.AddServiceEndpoint(typeof(ITestOutput), BindingFactory(), "http://localhost:" + (StartPort + i) + "/");
					break;
				} catch (AddressAlreadyInUseException) {
				}
			}
			host.Open();
			var start = DateTime.Now;
			Exception final = null;
			var res = new ChannelFactory<ITestRunner>(BindingFactory(), "http://localhost:" + (StartPort + i + 1) + "/").CreateChannel();
			while (DateTime.Now - start < TimeSpan.FromSeconds(5)) {
				try {
					res.Ping();
					return res;
				} catch (Exception e) {
					final = e;
				}
			}
			throw final;
		}

		public static Binding BindingFactory() {
			return new BasicHttpBinding();
		}
	}
}
