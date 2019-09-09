namespace Undisposed
{
	partial class ModuleWeaver
	{
		public void CleanReferences()
		{
			foreach (var typeDefinition in ModuleDefinition.GetTypes())
			{
				typeDefinition.CustomAttributes.RemoveDoNotTrack();
				foreach (var field in typeDefinition.Fields)
				{
					field.CustomAttributes.RemoveDoNotTrack();
				}
			}
		}
	}
}
