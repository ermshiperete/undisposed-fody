// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Undisposed
{
	public static class DisposeTracker
	{
		private static Dictionary<Type, int> _Registrations = new Dictionary<Type, int>();
		private static Dictionary<int, int> _ObjectNumber = new Dictionary<int, int>();
		private static Dictionary<Type, List<int>> _UndisposedObjects = new Dictionary<Type, List<int>>();

		public static TrackerOutputKind OutputKind { get; set; }

		static DisposeTracker()
		{
			OutputKind = TrackerOutputKind.Dump | TrackerOutputKind.Registration;
		}

		public static void Reset()
		{
			_Registrations = new Dictionary<Type, int>();
			_ObjectNumber = new Dictionary<int, int>();
			_UndisposedObjects = new Dictionary<Type, List<int>>();
		}

		public static void Register(object obj)
		{
			var t = obj.GetType();
			var hash = RuntimeHelpers.GetHashCode(obj);
			if (!_Registrations.ContainsKey(t))
			{
				_Registrations.Add(t, 1);
				_UndisposedObjects.Add(t, new List<int>());
			}
			var thisNumber = _Registrations[t]++;
			_ObjectNumber.Add(hash, thisNumber);
			_UndisposedObjects[t].Add(thisNumber);

			if ((OutputKind & TrackerOutputKind.Registration) != 0)
				Console.WriteLine("*** Creating {0} {1}", t.FullName, thisNumber);
		}

		public static void Unregister(object obj)
		{
			var t = obj.GetType();
			var hash = RuntimeHelpers.GetHashCode(obj);
			int thisNumber;
			if (!_ObjectNumber.TryGetValue(hash, out thisNumber))
			{
				Console.WriteLine("Disposing {0}: Error: Object was not registered", t.FullName);
				return;
			}

			if ((OutputKind & TrackerOutputKind.Registration) != 0)
				Console.WriteLine("*** Disposing {0} {1}", t.FullName, thisNumber);

			_ObjectNumber.Remove(hash);
			_UndisposedObjects[t].Remove(thisNumber);
			if (_UndisposedObjects[t].Count == 0)
				_UndisposedObjects.Remove(t);

			DumpUndisposedObjects();
		}

		private static void DumpUndisposedObjects()
		{
			if ((OutputKind & TrackerOutputKind.Dump) == 0)
				return;

			Console.WriteLine("**** Undisposed Object Dump:");
			foreach (var type in _UndisposedObjects.Keys)
			{
				Console.Write("\t{0}: ", type.FullName);
				foreach (var n in _UndisposedObjects[type])
				{
					Console.Write("{0},", n);
				}
				Console.WriteLine();
			}
		}
	}
}

