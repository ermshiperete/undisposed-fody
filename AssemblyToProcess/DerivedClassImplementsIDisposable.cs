using System;

namespace AssemblyToProcess
{
	public class DerivedClassImplementsIDisposable : NoDefaultCtor, IDisposable
	{
		public DerivedClassImplementsIDisposable()
			: base(string.Empty)
		{
		}

		void IDisposable.Dispose()
		{
		}
	}
}
