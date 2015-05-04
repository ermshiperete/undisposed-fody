using System;

namespace AssemblyToProcess
{
	public class InternalClass : IDisposable
	{
		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
		}
	}
}
