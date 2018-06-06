// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Linq;
using Mono.Cecil;
using System.Reflection;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using System.IO;
using Fody;

namespace Undisposed
{
	public partial class ModuleWeaver: BaseModuleWeaver
	{
		public ModuleWeaver()
		{
			// Init logging delegates to make testing easier
			LogInfo = s => { };
			LogWarning = s => { };
			LogError = s => { };

			AddinDirectoryPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);
		}

		public override void Execute()
		{
			FindCoreReferences();

			foreach (var type in ModuleDefinition
				.GetTypes()
				.Where(x =>
					x.IsClass() &&
					!x.IsAbstract &&
					!x.IsGeneratedCode() &&
					!x.CustomAttributes.ContainsDoNotTrack()))
			{
				var disposeMethods = type.Methods
					.Where(x => !x.IsStatic && (x.Name == "Dispose" || x.Name == "System.IDisposable.Dispose"))
					.ToList();
				if (disposeMethods.Count == 0)
					continue;

				{
					LogInfo($"Patching class {type.FullName}");
					var disposeMethod = disposeMethods.FirstOrDefault(x => !x.HasParameters);
					if (disposeMethod == null)
					{
						// If the base type is not in the same assembly as the type we're processing
						// then we want to patch the Dispose method. If it is in the same
						// assembly then the patch code gets added to the Dispose method of the
						// base class, so we skip this type.
						if (type.BaseType.Scope == type.Scope)
							continue;

						disposeMethod = disposeMethods[0];
					}
					ProcessDisposeMethod(disposeMethod);

					var constructors = type.Methods.Where(x => !x.IsStatic && x.IsConstructor).ToList();
					if (constructors.Count == 0)
						continue;

					foreach (var ctor in constructors)
					{
						ProcessConstructor(ctor);
					}
				}
			}

			CleanReferences();
		}

		public override IEnumerable<string> GetAssembliesForScanning()
		{
			yield break;
		}

		private void FindCoreReferences()
		{
			var thisModule = ModuleDefinition.ReadModule(Path.Combine(AddinDirectoryPath, "Undisposed.Fody.dll"));
			var disposeTrackerType = thisModule.Types.First(x => x.FullName == "Undisposed.DisposeTracker");
			RegisterMethodReference = ModuleDefinition.ImportReference(disposeTrackerType.Find("Register", typeof(object).FullName));
			UnregisterMethodReference = ModuleDefinition.ImportReference(disposeTrackerType.Find("Unregister", typeof(object).FullName));
		}

		private IEnumerable<Instruction> GetRegisterCallInstructions()
		{
			yield return Instruction.Create(OpCodes.Nop);
			yield return Instruction.Create(OpCodes.Ldarg_0);
			yield return Instruction.Create(OpCodes.Call, RegisterMethodReference);
		}

		private IEnumerable<Instruction> GetUnregisterCallInstructions()
		{
			yield return Instruction.Create(OpCodes.Nop);
			yield return Instruction.Create(OpCodes.Ldarg_0);
			yield return Instruction.Create(OpCodes.Call, UnregisterMethodReference);
		}

		private void ProcessConstructor(MethodDefinition ctor)
		{
			var instructions = ctor.Body.Instructions;
			foreach (var instruction in instructions)
			{
				if (instruction.OpCode != OpCodes.Call)
					continue;

				if (!(instruction.Operand is MethodReference method))
					continue;

				if (!ctor.DeclaringType.IsDerivedFrom(method.DeclaringType))
					continue;

				if (method is MethodDefinition methodDefinition && methodDefinition.IsConstructor)
					return;
			}
			instructions.InsertAt(instructions.Count - 1, GetRegisterCallInstructions());
		}

		private void ProcessDisposeMethod(MethodDefinition disposeMethod)
		{
			var instructions = disposeMethod.Body.Instructions;
			var prevInstructions = new List<Instruction>(instructions);
			instructions.Clear();
			instructions.Add(GetUnregisterCallInstructions());
			instructions.Add(prevInstructions);
		}

		public MethodReference RegisterMethodReference;
		public MethodReference UnregisterMethodReference;
	}
}
