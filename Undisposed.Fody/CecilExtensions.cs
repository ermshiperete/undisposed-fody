using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System;

namespace Undisposed
{
	public static class CecilExtensions
	{
		public static bool ContainsDoNotTrack(this IEnumerable<CustomAttribute> attributes)
		{
			return attributes.Any(x => x.AttributeType.FullName == "Undisposed.DoNotTrackAttribute");
		}

		public static void RemoveDoNotTrack(this Collection<CustomAttribute> attributes)
		{
			var attribute = attributes.FirstOrDefault(x => x.AttributeType.FullName == "Undisposed.DoNotTrackAttribute");
			if (attribute != null)
			{
				attributes.Remove(attribute);
			}
		}

		public static bool IsClass(this TypeDefinition x)
		{
			return (x.BaseType != null) && !x.IsEnum && !x.IsInterface;
		}

		public static bool IsIDisposable(this TypeReference typeRef)
		{
			var type = typeRef.Resolve();
			return type.Interfaces.Any(i => i.InterfaceType.FullName.Equals("System.IDisposable"))
				|| type.BaseType != null && type.BaseType.IsIDisposable();
		}


		public static void InsertAtStart(this Collection<Instruction> collection, IEnumerable<Instruction> instructions)
		{
			collection.InsertAt(0, instructions);
		}

		public static void InsertAt(this Collection<Instruction> collection, int index, IEnumerable<Instruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				collection.Insert(index, instruction);
				index++;
			}
		}

		public static void Add(this Collection<Instruction> collection, params Instruction[] instructions)
		{
			foreach (var instruction in instructions)
			{
				collection.Add(instruction);
			}
		}

		public static void Add(this Collection<Instruction> collection, IEnumerable<Instruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				collection.Add(instruction);
			}
		}

		public static MethodDefinition Find(this TypeDefinition typeReference, string name, params string[] paramTypes)
		{
			foreach (var method in typeReference.Methods)
			{
				if (method.IsMatch(name, paramTypes))
				{
					return method;
				}
			}
			throw new ApplicationException($"Could not find '{name}' on '{typeReference.Name}'");
		}

		public static string GetName(this FieldDefinition field)
		{
			return $"{field.DeclaringType.FullName}.{field.Name}";
		}

		public static bool IsMatch(this MethodReference methodReference, string name, params string[] paramTypes)
		{
			if (methodReference.Parameters.Count != paramTypes.Length)
			{
				return false;
			}
			if (methodReference.Name != name)
			{
				return false;
			}
			for (var index = 0; index < methodReference.Parameters.Count; index++)
			{
				var parameterDefinition = methodReference.Parameters[index];
				var paramType = paramTypes[index];
				if (parameterDefinition.ParameterType.FullName != paramType)
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsGeneratedCode(this ICustomAttributeProvider value)
		{
			return value.CustomAttributes.Any(a => a.AttributeType.Name == "CompilerGeneratedAttribute" || a.AttributeType.Name == "GeneratedCodeAttribute");
		}

		public static bool IsEmptyOrNotImplemented(this MethodDefinition method)
		{
			var instructions = method.Body.Instructions.Where(i => i.OpCode != OpCodes.Nop && i.OpCode != OpCodes.Ret).ToList();

			if (instructions.Count == 0)
				return true;

			if (instructions.Count != 2 || instructions[0].OpCode != OpCodes.Newobj || instructions[1].OpCode != OpCodes.Throw)
				return false;

			var ctor = (MethodReference)instructions[0].Operand;
			return ctor.DeclaringType.FullName == "System.NotImplementedException";
		}

		public static bool IsDerivedFrom(this TypeReference t, TypeReference type)
		{
			while (true)
			{
				if (type.FullName == "System.Object")
					return false; // everything derives from object, so we ignore that here

				if (t == type)
					return true;

				if (!(t is TypeDefinition typeDef) || typeDef.BaseType == null)
					return false;

				t = typeDef.BaseType;
			}
		}
	}
}