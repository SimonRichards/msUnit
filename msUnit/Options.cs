using System;
using System.Collections.Generic;
using System.Linq;

namespace msUnit {
	class Options {

		public readonly IList<string> Assemblies;
		public readonly IList<IFilter> Filters;
		public readonly bool Error;
		public readonly bool RunInSeparateProcess;
		public readonly bool SkipUntilFirst;
		public string FirstTest;
		public string Parent;

		public Options(IList<string> args) {
			Assemblies = new List<string>();
			Filters = new List<IFilter>();
			Error = !args.Any();
			Action<string> argHandler = null;
			RunInSeparateProcess = true;
			SkipUntilFirst = false;
			FirstTest = string.Empty;
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
					RunInSeparateProcess = false;
					argHandler = s => Parent = s;
					break;
				case "--startfrom":
					SkipUntilFirst = true;
					argHandler = s => Filters.Add(new SkipUntilFilter(s));
					break;
				default:
					if (argHandler == null) {
						Error = true;
						return;
					}
					argHandler(arg);
					break;
				}
			}
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
					"[--nottagged category3 [category4...]]";
			}
		}
	}
}
