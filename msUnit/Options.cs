using System;
using System.Collections.Generic;
using System.Linq;

namespace msUnit {
	class Options {
		public IList<string> Assemblies { get; private set; }

		public IList<IFilter> Filters { get; private set; }

		public bool Error { get; private set; }

		public Options(IList<string> args) {
			Assemblies = new List<string>();
			Filters = new List<IFilter>();
			Error = !args.Any();
			Action<string> argHandler = null;
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
