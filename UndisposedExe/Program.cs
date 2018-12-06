// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using Undisposed;

namespace UndisposedExe
{
	class MainClass
	{
		private static void Usage()
		{
			Console.WriteLine("Usage");
			Console.WriteLine("Undisposed.exe [-o outputfile] assemblyname");
		}

		private static void ProcessFile(string inputFile, string outputFile)
		{
			Console.WriteLine("Processing {0} -> {1}", inputFile, outputFile);
			var def = Mono.Cecil.ModuleDefinition.ReadModule(inputFile);
			var moduleWeaver = new ModuleWeaver { ModuleDefinition = def };
			moduleWeaver.Execute();
			def.Write(outputFile);
		}

		public static void Main(string[] args)
		{
			if (args.Length < 1 || args[0] == "--help" || args[0] == "-h")
			{
				Usage();
				return;
			}

			var inputFile = args[args.Length - 1];
			var outputFile = string.Empty;
			var isOutputFileSet = false;
			if (args.Length >= 3)
			{
				if (args[0] == "-o" || args[0] == "--output")
				{
					outputFile = args[1];
					isOutputFileSet = true;
				}
				else
				{
					Usage();
					return;
				}
			}

			if (!isOutputFileSet)
			{
				foreach (var arg in args)
				{
					inputFile = arg;
					ProcessFile(inputFile, inputFile);
				}
			}
			else
				ProcessFile(inputFile, outputFile);
		}
	}
}
