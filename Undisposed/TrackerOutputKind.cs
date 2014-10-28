// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;

namespace Undisposed
{
	[Flags]
	public enum TrackerOutputKind
	{
		None = 0,
		Registration = 1,
		Dump = 2
	}
}

