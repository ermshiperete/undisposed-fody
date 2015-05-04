using System;

namespace AssemblyToProcess
{
	public class TwoConstructorsCallingEachOther : IDisposable
	{
		public TwoConstructorsCallingEachOther()
			: this(string.Empty)
		{
		}

		public TwoConstructorsCallingEachOther(string x)
		{
		}

		public void Dispose()
		{
		}
	}
}
