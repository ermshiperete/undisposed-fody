using System.IO;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using System.Collections.Generic;
using Undisposed;

public class ModuleWeaverTestHelper
{
	public string BeforeAssemblyPath;
	public string AfterAssemblyPath;
	public Assembly Assembly;
	public List<string> Errors;

	public ModuleWeaverTestHelper(string inputAssembly)
	{
		BeforeAssemblyPath = Path.GetFullPath(inputAssembly);
#if !DEBUG
		BeforeAssemblyPath = BeforeAssemblyPath.Replace("Debug", "Release");
#endif
		AfterAssemblyPath = BeforeAssemblyPath.Replace(".dll", "2.dll");
#if __MonoCS__
		var oldPdb = BeforeAssemblyPath + ".mdb";
		var newPdb = AfterAssemblyPath + ".mdb";
#else
		var oldPdb = BeforeAssemblyPath.Replace(".dll", ".pdb");
		var newPdb = BeforeAssemblyPath.Replace(".dll", "2.pdb");
#endif
		File.Copy(BeforeAssemblyPath, AfterAssemblyPath, true);
		File.Copy(oldPdb, newPdb, true);

		Errors = new List<string>();

		using (var symbolStream = File.OpenRead(newPdb))
		{
			var readerParameters = new ReaderParameters {
				ReadSymbols = true,
				SymbolStream = symbolStream,
				SymbolReaderProvider = new PdbReaderProvider()
			};
			var moduleDefinition = ModuleDefinition.ReadModule(AfterAssemblyPath, readerParameters);

			var weavingTask = new ModuleWeaver {
				ModuleDefinition = moduleDefinition,
				LogError = s => Errors.Add(s),
			};

			weavingTask.Execute();
			moduleDefinition.Write(AfterAssemblyPath);
		}
		Assembly = Assembly.LoadFile(AfterAssemblyPath);
	}

}
