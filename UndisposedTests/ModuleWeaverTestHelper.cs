// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Undisposed;

public class ModuleWeaverTestHelper
{
	public string BeforeAssemblyPath;
	public string AfterAssemblyPath;
	public Assembly Assembly;
	public List<string> Errors;

	private static bool IsUnix => Environment.OSVersion.Platform == PlatformID.Unix;

	public ModuleWeaverTestHelper(string inputAssembly)
	{
		BeforeAssemblyPath = Path.GetFullPath(inputAssembly);
#if !DEBUG
		BeforeAssemblyPath = BeforeAssemblyPath.Replace("Debug", "Release");
#endif
		AfterAssemblyPath = BeforeAssemblyPath.Replace(".dll", "2.dll");
		var useMdb = IsUnix && File.Exists(BeforeAssemblyPath + ".mdb");
		var oldPdb =  useMdb ?BeforeAssemblyPath + ".mdb" : BeforeAssemblyPath.Replace(".dll", ".pdb");
		var newPdb = useMdb ?
			AfterAssemblyPath + ".mdb" : AfterAssemblyPath.Replace(".dll", ".pdb");

		Errors = new List<string>();

		ModuleDefinition moduleDefinition;
		using (var symbolStream = File.OpenRead(oldPdb))
		{
			var readerParameters = new ReaderParameters
			{
				ReadSymbols = true,
				SymbolStream = symbolStream,
				SymbolReaderProvider = useMdb ?
					(ISymbolReaderProvider)new MdbReaderProvider() : new PdbReaderProvider()
			};
			moduleDefinition = ModuleDefinition.ReadModule(BeforeAssemblyPath, readerParameters);

			var weavingTask = new ModuleWeaver
			{
				ModuleDefinition = moduleDefinition,
				LogError = s => Errors.Add(s)
			};

			weavingTask.Execute();
		}
		using (var symbolStream = File.Open(newPdb, FileMode.OpenOrCreate))
		{
			var writerParameters = new WriterParameters
			{
				WriteSymbols = true,
				SymbolStream = symbolStream,
				SymbolWriterProvider =
					useMdb ? (ISymbolWriterProvider) new MdbWriterProvider() : new PdbWriterProvider()
			};
			moduleDefinition.Write(AfterAssemblyPath, writerParameters);
		}
		Assembly = Assembly.LoadFile(AfterAssemblyPath);
	}
}
