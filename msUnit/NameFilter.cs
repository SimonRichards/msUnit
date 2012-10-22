using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace msUnit
{
	class NameFilter : IFilter
	{
		private readonly string _name;

		public NameFilter (string name)
		{
			_name = name;
		}

		public bool Test (Type type, MethodInfo method)
		{
			return method.Name.Contains (_name);
		}
	}
}
