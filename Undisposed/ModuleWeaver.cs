// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Linq;
using Mono.Cecil;
using System.Reflection;
using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace Undisposed
{
	public partial class ModuleWeaver
	{
		public Action<string> LogInfo { get; set; }

		public Action<string> LogWarning { get; set; }

		public Action<string> LogError { get; set; }

		public ModuleDefinition ModuleDefinition { get; set; }

		public IAssemblyResolver AssemblyResolver { get; set; }

		public ModuleWeaver()
		{
			LogInfo = s =>
			{
			};
			LogWarning = s =>
			{
			};
			LogError = s =>
			{
			};
		}

		public void Execute()
		{
			FindCoreReferences();

			foreach (var type in ModuleDefinition
            .GetTypes()
            .Where(x =>
                x.IsClass() &&
                !x.IsAbstract &&
                !x.IsGeneratedCode() &&
                !x.CustomAttributes.ContainsSkipWeaving()))
			{
				var disposeMethods = type.Methods
                                     .Where(x => !x.IsStatic && (x.Name == "Dispose" || x.Name == "System.IDisposable.Dispose"))
                                     .ToList();
				if (disposeMethods.Count != 0)
				{
					var disposeMethod = disposeMethods.First(x => !x.HasParameters);
					ProcessDisposeMethod(disposeMethod);

					var constructors = type.Methods.Where(x => !x.IsStatic && x.IsConstructor).ToList();
					if (constructors.Count != 0)
					{
						foreach (var ctor in constructors)
						{
							ProcessConstructor(ctor);
						}
					}
				}
			}
			//CleanReferences();
		}

		private void FindCoreReferences()
		{
			var uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
			var thisModule = ModuleDefinition.ReadModule(uri.AbsolutePath);
			var disposeTrackerType = thisModule.Types.First(x => x.FullName == "Undisposed.DisposeTracker");
			RegisterMethodReference = ModuleDefinition.Import(disposeTrackerType.Find("Register", typeof(object).FullName));
			UnregisterMethodReference = ModuleDefinition.Import(disposeTrackerType.Find("Unregister", typeof(object).FullName));

//			var assemblyResolver = ModuleDefinition.AssemblyResolver;
//			var msCoreLibDefinition = assemblyResolver.Resolve("mscorlib");
//			var msCoreTypes = msCoreLibDefinition.MainModule.Types;
//
//			ObjectFinalizeReference = ModuleDefinition.Import(ModuleDefinition.TypeSystem.Object.Resolve().Find("Finalize"));
//
//			var gcTypeDefinition = msCoreTypes.First(x => x.Name == "GC");
//			SuppressFinalizeMethodReference = ModuleDefinition.Import(gcTypeDefinition.Find("SuppressFinalize", "Object"));
//
//			var interlockedTypeDefinition = msCoreTypes.First(x => x.Name == "Interlocked");
//			ExchangeMethodReference = ModuleDefinition.Import(interlockedTypeDefinition.Find("Exchange", "Int32&", "Int32"));
//
//			var exceptionTypeDefinition = msCoreTypes.First(x => x.Name == "ObjectDisposedException");
//			ExceptionConstructorReference = ModuleDefinition.Import(exceptionTypeDefinition.Find(".ctor", "String"));
//
//			var iDisposableTypeDefinition = msCoreTypes.First(x => x.Name == "IDisposable");
//			DisposeMethodReference = ModuleDefinition.Import(iDisposableTypeDefinition.Find("Dispose"));
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
				if (instruction.OpCode == OpCodes.Call)
				{
					var method = instruction.Operand as MethodReference;
					if (method != null)
					{
						if (ctor.DeclaringType.IsDerivedFrom(method.DeclaringType))
						{
							var methodDefinition = method as MethodDefinition;
							if (methodDefinition != null && methodDefinition.IsConstructor)
								return;
						}
					}
				}
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
		//		public MethodReference ExchangeMethodReference;
		//		public MethodReference SuppressFinalizeMethodReference;
		//		public MethodReference ObjectFinalizeReference;
		//		public MethodReference DisposeMethodReference;
		//		public MethodReference ExceptionConstructorReference;

	}
}
