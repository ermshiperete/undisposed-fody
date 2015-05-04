// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Undisposed
{
	public static class DisposeTracker
	{
		private static Dictionary<Type, int> _Registrations = new Dictionary<Type, int>();
		private static Dictionary<int, int> _ObjectNumber = new Dictionary<int, int>();
		private static Dictionary<Type, List<int>> _UndisposedObjects = new Dictionary<Type, List<int>>();

		public static TrackerOutputKind OutputKind { get; set; }
		public static Action<string> LogWriter { get; set; }

		static DisposeTracker()
		{
			OutputKind = TrackerOutputKind.Dump | TrackerOutputKind.Registration;
			LogWriter = Console.WriteLine;
		}

		internal static void Reset()
		{
			lock (_Registrations)
			{
				_Registrations = new Dictionary<Type, int>();
				_ObjectNumber = new Dictionary<int, int>();
				_UndisposedObjects = new Dictionary<Type, List<int>>();
			}
		}

		public static void Register(object obj)
		{
			lock (_Registrations)
			{
				var t = obj.GetType();
				var hash = RuntimeHelpers.GetHashCode(obj);
				if (!_Registrations.ContainsKey(t))
					_Registrations.Add(t, 1);
				if (!_UndisposedObjects.ContainsKey(t))
					_UndisposedObjects.Add(t, new List<int>());

				var thisNumber = _Registrations[t]++;
				_ObjectNumber.Add(hash, thisNumber);
				_UndisposedObjects[t].Add(thisNumber);

				if ((OutputKind & TrackerOutputKind.Registration) != 0)
					LogWriter(string.Format("*** Creating {0} {1}", t.FullName, thisNumber));
			}
		}

		public static void Unregister(object obj)
		{
			lock (_Registrations)
			{
				var t = obj.GetType();
				var hash = RuntimeHelpers.GetHashCode(obj);
				int thisNumber;
				if (!_ObjectNumber.TryGetValue(hash, out thisNumber))
				{
					LogWriter(string.Format("Disposing {0}: Error: Object was not registered", t.FullName));
					return;
				}

				if ((OutputKind & TrackerOutputKind.Registration) != 0)
					LogWriter(string.Format("*** Disposing {0} {1}", t.FullName, thisNumber));

				_ObjectNumber.Remove(hash);
				_UndisposedObjects[t].Remove(thisNumber);
				if (_UndisposedObjects[t].Count == 0)
					_UndisposedObjects.Remove(t);

				DumpUndisposedObjects();
			}
		}

		public static void DumpUndisposedObjects()
		{
			lock (_Registrations)
			{
				if ((OutputKind & TrackerOutputKind.Dump) == 0)
					return;

				LogWriter("**** Undisposed Object Dump:");
				foreach (var type in _UndisposedObjects.Keys)
				{
					LogWriter(string.Format("\t{0}: {1}", type.FullName,
						string.Join(",", _UndisposedObjects[type].Select(n => n.ToString()))));
				}
			}
		}
	}
}

