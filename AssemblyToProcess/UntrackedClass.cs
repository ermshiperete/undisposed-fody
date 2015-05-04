// Copyright (c) 2015 Applied Systems, Inc.

using System;
using Undisposed;

namespace AssemblyToProcess
{
	[DoNotTrack]
	public class UntrackedClass : IDisposable
	{
		public void Dispose()
		{
		}
	}
}
