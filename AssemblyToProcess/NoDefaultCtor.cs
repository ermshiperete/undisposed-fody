using System;

namespace AssemblyToProcess
{
	public class NoDefaultCtor : IDisposable
	{
		public NoDefaultCtor(string x)
		{
		}

		public void Dispose()
		{
		}
	}
}
