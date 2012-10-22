using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace msUnit {

	/// <summary>
	/// Responsible for parsing command line arguments
	/// </summary>
	class Options {

		private enum OutputKind {
			Console,
			Teamcity
		}

		public readonly IList<string> Assemblies;
		public readonly IList<IFilter> Filters;
		public readonly bool HasError;
		public readonly bool IsChild;
		public readonly ITestOutput Output;
		public string CrashedTest;
		public string Parent;
		public readonly string PipeName;

		public Options(IList<string> args) {
			PipeName = Guid.NewGuid().ToString();
			Assemblies = new List<string>();
			Filters = new List<IFilter>();
			HasError = !args.Any();
			Action<string> argHandler = null;
			CrashedTest = string.Empty;
			IsChild = false;
			OutputKind output = OutputKind.Console;
			foreach (var arg in args) {
				switch (arg) {
					case "--from":
						argHandler = Assemblies.Add;
						break;
					case "--named":
						argHandler = s => Filters.Add(new NameFilter(s));
						break;
					case "--tagged":
						argHandler = s => Filters.Add(new CategoryFilter(s, true));
						break;
					case "--nottagged":
						argHandler = s => Filters.Add(new CategoryFilter(s, false));
						break;
					case "--parent":
						IsChild = true;
						argHandler = s => Parent = s;
						break;
					case "--startfrom":
						argHandler = s => {
							Filters.Add(new SkipUntilFilter(s));
							CrashedTest = s;
						};
						break;
					case "--teamcity":
						output = OutputKind.Teamcity;
						argHandler = null;
						break;
					default:
						if (argHandler == null) {
							HasError = true;
							return;
						}
						argHandler(arg);
						break;
				}
			}
			Output = output == OutputKind.Console ? (ITestOutput)new ConsoleWriter() : new TeamcityOutput();
		}

		public static string CreateParentOption(string pipeName) {
			return " --parent " + pipeName;
		}

		public static string CreateLastTestOption(string lastRunTest) {
			return " --startfrom " + lastRunTest;
		}

		public string Usage {
			get {
				return "MSTest " +
					"--from Assembly1.dll [Assembly2.dll ...] " +
					"[--named testName1 [testName2...]] " +
					"[--tagged category1 [category2...]] " +
					"[--nottagged category3 [category4...]]" +
					"[--teamcity]";
			}
		}
	}
}
