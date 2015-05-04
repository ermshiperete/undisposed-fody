using System;

namespace AssemblyToProcess
{
	public class ObjectNotDisposed : IDisposable
	{
		private readonly AllObjectsDisposed _allObjectsDisposed;

		public ObjectNotDisposed()
		{
			_allObjectsDisposed = new AllObjectsDisposed();
		}

		public void Dispose()
		{
		}
	}
}
