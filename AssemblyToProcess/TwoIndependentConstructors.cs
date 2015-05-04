using System;

namespace AssemblyToProcess
{
	public class TwoIndependentConstructors : IDisposable
	{
		public TwoIndependentConstructors()
		{
		}

		public TwoIndependentConstructors(string x)
		{
		}

		public void Dispose()
		{
		}
	}
}
