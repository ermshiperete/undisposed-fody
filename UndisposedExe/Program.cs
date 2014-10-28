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

		public static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Usage();
				return;
			}

			string inputFile = args[args.Length - 1];
			string outputFile;
			if (args.Length >= 3)
			{
				if (args[0] == "-o" || args[0] == "--output")
				{
					outputFile = args[1];
				}
				else
				{
					Usage();
					return;
				}
			}
			else
				outputFile = inputFile;

			var def = Mono.Cecil.ModuleDefinition.ReadModule(inputFile);
			var moduleWeaver = new ModuleWeaver();
			moduleWeaver.ModuleDefinition = def;
			moduleWeaver.Execute();
			def.Write(outputFile);
		}
	}
}
