using System.Diagnostics;

namespace AssemblyToProcess
{
	public class DerivedFromExternalClass : Process
	{
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}
