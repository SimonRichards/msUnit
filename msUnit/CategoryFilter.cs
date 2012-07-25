using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace msUnit {
	class CategoryFilter : IFilter {
		private readonly string _category;
		private readonly bool _inclusive;

		public CategoryFilter(string category, bool inclusive) {
			_category = category;
			_inclusive = inclusive;
		}

		public bool Test(MethodInfo method) {
			var categoryAttribute = method.GetCustomAttributes(typeof(TestCategoryAttribute), false).FirstOrDefault();
			if (categoryAttribute == null) {
				return !_inclusive;
			}
			return _inclusive == (((TestCategoryAttribute)categoryAttribute).Name == _category);
		}
	}
}
