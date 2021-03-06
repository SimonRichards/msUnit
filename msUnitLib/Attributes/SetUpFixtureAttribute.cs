// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org
// ****************************************************************

using System;

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
	/// <summary>
	/// SetUpFixtureAttribute is used to identify a SetUpFixture
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
	public class SetUpFixtureAttribute : Attribute
	{
	}
}
