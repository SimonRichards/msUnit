using System;
using System.Runtime.Serialization;

namespace Microsoft.VisualStudio.TestTools.UnitTesting 
{
	public class AssertFailedException : Exception
	{
		/// <param name="message">The error message that explains 
		/// the reason for the exception</param>
		public AssertFailedException (string message) : base(message) 
		{}

		/// <param name="message">The error message that explains 
		/// the reason for the exception</param>
		/// <param name="inner">The exception that caused the 
		/// current exception</param>
		public AssertFailedException(string message, Exception inner) :
			base(message, inner) 
		{}

		/// <summary>
		/// Serialization Constructor
		/// </summary>
		protected AssertFailedException(SerializationInfo info, 
			StreamingContext context) : base(info,context)
		{}
	}
}

