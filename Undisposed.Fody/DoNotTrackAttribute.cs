using System;

namespace Undisposed
{
	[AttributeUsage(
		AttributeTargets.Class | AttributeTargets.Field,
		Inherited = false)]
	public class DoNotTrackAttribute : Attribute
	{
	}
}
