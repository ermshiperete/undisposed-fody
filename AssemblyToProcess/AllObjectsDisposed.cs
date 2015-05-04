using System;

namespace AssemblyToProcess
{
	public class AllObjectsDisposed : IDisposable
	{
		public AllObjectsDisposed()
		{
			Console.WriteLine("Created AllObjectsDisposed");
		}

		public void Dispose()
		{
			Console.WriteLine("Disposed AllObjectsDisposed");
		}
	}
}
