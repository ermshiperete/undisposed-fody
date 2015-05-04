using System;

namespace Undisposed
{
	[AttributeUsage(
		AttributeTargets.Class | AttributeTargets.Field,
		AllowMultiple = false,
		Inherited = false)]
	public class DoNotTrackAttribute : Attribute
	{
	}
}
