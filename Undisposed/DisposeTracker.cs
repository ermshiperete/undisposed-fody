// Copyright (c) 2014 Eberhard Beilharz
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Undisposed
{
	public static class DisposeTracker
	{
		private static Dictionary<Type, int> _Registrations = new Dictionary<Type, int>();
		private static Dictionary<int, int> _ObjectNumber = new Dictionary<int, int>();
		private static Dictionary<Type, List<Tuple<int, string>>>_UndisposedObjects =
			new Dictionary<Type, List<Tuple<int, string>>>();

		public static TrackerOutputKind OutputKind { get; set; }
		public static Action<string> LogWriter { get; set; }
		public static bool TrackCreationStackTrace { get; set; }

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
				_UndisposedObjects = new Dictionary<Type, List<Tuple<int, string>>>();
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
					_UndisposedObjects.Add(t, new List<Tuple<int, string>>());

				var thisNumber = _Registrations[t]++;
				_ObjectNumber.Add(hash, thisNumber);
				_UndisposedObjects[t].Add(TrackCreationStackTrace
					? new Tuple<int, string>(thisNumber, GetStackTraceString())
					: new Tuple<int, string>(thisNumber, string.Empty));

				if ((OutputKind & TrackerOutputKind.Registration) != 0)
					LogWriter($"*** Creating {t.FullName} {thisNumber}");
			}
		}

		private static string GetStackTraceString()
		{
			var stack = new StackTrace();
			var stackTraceFrames = stack.ToString().Split(new[] { Environment.NewLine },
				StringSplitOptions.None);
			return string.Join(Environment.NewLine, stackTraceFrames.Skip(2));
		}

		public static void Unregister(object obj)
		{
			lock (_Registrations)
			{
				var objType = obj.GetType();
				var hash = RuntimeHelpers.GetHashCode(obj);
				if (!_ObjectNumber.TryGetValue(hash, out var thisNumber))
				{
					LogWriter($"Disposing {objType.FullName}: Error: Object was not registered");
					return;
				}

				if ((OutputKind & TrackerOutputKind.Registration) != 0)
					LogWriter($"*** Disposing {objType.FullName} {thisNumber}");

				_ObjectNumber.Remove(hash);

				var target = _UndisposedObjects[objType].First(y => y.Item1 == thisNumber);
				_UndisposedObjects[objType].Remove(target);

				if (_UndisposedObjects[objType].Count == 0)
					_UndisposedObjects.Remove(objType);

				DumpUndisposedObjects();
			}
		}

		public static void DumpUndisposedObjects()
		{
			if (TrackCreationStackTrace)
				DumpUndiposedObjectWithStackTrace();
			else
				DumpUndiposedObjectWithoutStackTrace();
		}

		private static void DumpUndiposedObjectWithStackTrace()
		{
			lock (_Registrations)
			{
				if ((OutputKind & TrackerOutputKind.Dump) == 0)
					return;

				LogWriter("**** Undisposed Object Dump:");
				foreach (var type in _UndisposedObjects.Keys)
				{
					foreach (Tuple<int, string> entry in _UndisposedObjects[type])
					{
						LogWriter(
							$"\t{type.FullName}: {entry.Item1}\n\tStack Trace:\n{entry.Item2}");
					}
				}
			}
		}

		private static void DumpUndiposedObjectWithoutStackTrace()
		{
			lock (_Registrations)
			{
				if ((OutputKind & TrackerOutputKind.Dump) == 0)
					return;

				LogWriter("**** Undisposed Object Dump:");
				foreach (var type in _UndisposedObjects.Keys)
				{
					LogWriter(
						$"\t{type.FullName}: {string.Join(",", _UndisposedObjects[type].Select(n => n.Item1.ToString()))}");
				}
			}
		}
	}
}

